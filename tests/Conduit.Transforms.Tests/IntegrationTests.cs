using System.Text.Json;
using Conduit.Core.Models;
using Conduit.Core.Services;
using Conduit.Services;
using Conduit.Sources.Edi834.Models;
using Conduit.Transforms;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit.Abstractions;

namespace Conduit.Transforms.Tests;

public class IntegrationTests : IDisposable
{
    private readonly string _rawDir;
    private readonly string _transformedDir;
    private readonly TransformPipeline _pipeline;
    private readonly JsonOutputWriter _rawWriter;
    private readonly JsonTransformedOutputWriter _transformedWriter;
    private readonly ITestOutputHelper _testOutput;

    public IntegrationTests(ITestOutputHelper testOutput)
    {
        _testOutput = testOutput;
        var baseDir = Path.Combine(Path.GetTempPath(), $"conduit-integration-{Guid.NewGuid()}");
        _rawDir = Path.Combine(baseDir, "raw");
        _transformedDir = Path.Combine(baseDir, "transformed");
        Directory.CreateDirectory(_rawDir);
        Directory.CreateDirectory(_transformedDir);

        _pipeline = new TransformPipeline(new List<ITransform>
        {
            new DeduplicationTransform(),
            new RssEnrichmentTransform(),
            new Edi834EnrichmentTransform(),
            new ZoteroEnrichmentTransform()
        });

        _rawWriter = new JsonOutputWriter(_rawDir, Mock.Of<ILogger<JsonOutputWriter>>());
        _transformedWriter = new JsonTransformedOutputWriter(_transformedDir,
            Mock.Of<ILogger<JsonTransformedOutputWriter>>());
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        var baseDir = Path.GetDirectoryName(_rawDir)!;
        if (Directory.Exists(baseDir))
        {
            Directory.Delete(baseDir, recursive: true);
        }
    }

    [Fact]
    public async Task EndToEnd_Rss_Dedup_And_Enrich()
    {
        var items = new List<IPipelineRecord>
        {
            new FeedItem("AI Breakthrough in Neural Networks", "https://example.com/1",
                "Researchers discover new neural network architecture", DateTime.UtcNow),
            new FeedItem("AI Breakthrough Copy", "https://example.com/1",
                "Same article from another feed", DateTime.UtcNow),
            new FeedItem("Kubernetes Security Update", "https://example.com/2",
                "Critical security patch for kubernetes clusters", DateTime.UtcNow)
        };

        // Raw write
        await _rawWriter.WriteAsync(items, "rss", "test-feed");

        // Transform
        var transformed = await _pipeline.ExecuteAsync(items);

        // Transformed write
        await _transformedWriter.WriteAsync(transformed, "rss", "test-feed");

        // Verify: dedup removed 1 of 3
        Assert.Equal(2, transformed.Count);

        // Verify: keywords enriched
        Assert.All(transformed, t => Assert.True(t.Enrichment.ContainsKey("keywords")));

        // Verify: raw has 3 items, transformed has 2
        var rawFiles = Directory.GetFiles(Path.Combine(_rawDir, "rss"), "*.json");
        var transformedFiles = Directory.GetFiles(Path.Combine(_transformedDir, "rss"), "*.json");
        Assert.Single(rawFiles);
        Assert.Single(transformedFiles);

        var rawJson = await File.ReadAllTextAsync(rawFiles[0]);
        var transformedJson = await File.ReadAllTextAsync(transformedFiles[0]);
        using var rawDoc = JsonDocument.Parse(rawJson);
        using var transformedDoc = JsonDocument.Parse(transformedJson);
        Assert.Equal(3, rawDoc.RootElement.GetArrayLength());
        Assert.Equal(2, transformedDoc.RootElement.GetArrayLength());
    }

    [Fact]
    public async Task EndToEnd_Edi834_Dedup_And_Status_Enrichment()
    {
        var items = new List<IPipelineRecord>
        {
            new EnrollmentRecord("SUB001", "SUB001", true, "Doe, Jane", "18", "021",
                new DateTime(2026, 1, 1), null, "PLAN-A"),
            new EnrollmentRecord("SUB001", "SUB001", true, "Doe, Jane Updated", "18", "024",
                new DateTime(2026, 1, 1), new DateTime(2026, 3, 15), "PLAN-A"),
            new EnrollmentRecord("SUB002", "SUB002", true, "Smith, Bob", "18", "021",
                new DateTime(2026, 1, 1), null, "PLAN-B")
        };

        var transformed = await _pipeline.ExecuteAsync(items);
        await _transformedWriter.WriteAsync(transformed, "edi834", "test-enrollment");

        // Dedup: SUB001 + PLAN-A + 2026-01-01 appears twice, keeps first
        Assert.Equal(2, transformed.Count);

        // First record is active (021 addition)
        Assert.Equal("active", transformed[0].Enrichment["enrollmentStatus"]);
        Assert.Equal("self", transformed[0].Enrichment["relationship"]);

        // Second record is SUB002
        Assert.Equal("active", transformed[1].Enrichment["enrollmentStatus"]);
    }

    [Fact]
    public async Task EndToEnd_Idempotent_On_Same_Input()
    {
        var items = new List<IPipelineRecord>
        {
            new FeedItem("Title A", "https://example.com/1", "Desc", DateTime.UtcNow),
            new FeedItem("Title B", "https://example.com/2", "Desc", DateTime.UtcNow),
            new FeedItem("Title A Dup", "https://example.com/1", "Desc", DateTime.UtcNow)
        };

        var run1 = await _pipeline.ExecuteAsync(items);
        var run2 = await _pipeline.ExecuteAsync(items);

        Assert.Equal(run1.Count, run2.Count);
        Assert.Equal(
            run1.Select(r => r.Record.Id).OrderBy(id => id),
            run2.Select(r => r.Record.Id).OrderBy(id => id));
    }

    [Fact]
    public async Task EndToEnd_Raw_Output_Preserved_Unchanged()
    {
        var items = new List<IPipelineRecord>
        {
            new FeedItem("Title", "https://example.com/1", "Desc", DateTime.UtcNow)
        };

        await _rawWriter.WriteAsync(items, "rss", "test");
        var rawFiles = Directory.GetFiles(Path.Combine(_rawDir, "rss"), "*.json");
        var rawContentBefore = await File.ReadAllTextAsync(rawFiles[0]);

        // Transform and write to separate location
        var transformed = await _pipeline.ExecuteAsync(items);
        await _transformedWriter.WriteAsync(transformed, "rss", "test");

        // Raw file unchanged
        var rawContentAfter = await File.ReadAllTextAsync(rawFiles[0]);
        Assert.Equal(rawContentBefore, rawContentAfter);

        // Transformed in separate directory
        Assert.True(Directory.Exists(Path.Combine(_transformedDir, "rss")));
    }

    [Fact]
    public async Task EndToEnd_Cross_Run_Dedup_Filters_Previously_Stored()
    {
        var enrichmentStages = new List<ITransform>
        {
            new RssEnrichmentTransform()
        };

        // First run: ingest and store two articles
        var firstBatch = new List<IPipelineRecord>
        {
            new FeedItem("Article 1", "https://example.com/1", "First article", DateTime.UtcNow),
            new FeedItem("Article 2", "https://example.com/2", "Second article", DateTime.UtcNow)
        };

        var pipeline1 = TransformPipeline.CreateForSource(
            _transformedWriter, "rss", enrichmentStages);
        var transformed1 = await pipeline1.ExecuteAsync(firstBatch);
        await _transformedWriter.WriteAsync(transformed1, "rss", "test-feed");
        Assert.Equal(2, transformed1.Count);

        // Second run: same two articles plus one new one
        var secondBatch = new List<IPipelineRecord>
        {
            new FeedItem("Article 1 Again", "https://example.com/1", "Same article", DateTime.UtcNow),
            new FeedItem("Article 2 Again", "https://example.com/2", "Same article", DateTime.UtcNow),
            new FeedItem("Article 3 New", "https://example.com/3", "Brand new", DateTime.UtcNow)
        };

        var pipeline2 = TransformPipeline.CreateForSource(
            _transformedWriter, "rss", enrichmentStages);
        var transformed2 = await pipeline2.ExecuteAsync(secondBatch);

        // Only the new article passes through
        Assert.Single(transformed2);
        Assert.Equal("https://example.com/3", transformed2[0].Record.Id);
        Assert.True(transformed2[0].Enrichment.ContainsKey("keywords"));
    }

    [Fact]
    public async Task EndToEnd_ValidationTransform_Splits_Valid_And_Invalid_To_Real_Dirs()
    {
        // Runs the full pipeline (validation → dedup → enrichment) against real output dirs.
        // Run: dotnet test --filter EndToEnd_ValidationTransform_Splits_Valid_And_Invalid_To_Real_Dirs
        // Then inspect: data/curated/edi834/ and data/rejected/edi834/
        var baseDir = Path.Combine(
            Directory.GetCurrentDirectory(), "..", "..", "..", "..", "..", "data");

        var curatedDir = Path.Combine(baseDir, "curated");
        var rejectedDir = Path.Combine(baseDir, "rejected");

        var curatedWriter = new JsonTransformedOutputWriter(curatedDir,
            NullLogger<JsonTransformedOutputWriter>.Instance);
        var rejectedWriter = new JsonRejectedOutputWriter(rejectedDir,
            NullLogger<JsonRejectedOutputWriter>.Instance);

        var validators = new List<IRecordValidator>
        {
            new EnrollmentRecordValidator()
        };

        var pipeline = PipelineFactory.CreateForSource(
            curatedWriter, rejectedWriter,
            "edi834", "benefits-enrollment",
            validators,
            [new Edi834EnrichmentTransform()]);

        var records = new List<IPipelineRecord>
        {
            // Valid: addition with correct codes
            new EnrollmentRecord("SUB001", "SUB001", true, "Smith, Alice", "18", "021",
                new DateTime(2026, 1, 1), null, "PLAN-A"),

            // Valid: termination with end date after start date
            new EnrollmentRecord("SUB002", "SUB001", false, "Jones, Bob", "01", "024",
                new DateTime(2026, 1, 1), new DateTime(2026, 12, 31), "PLAN-B"),

            // Invalid: unknown maintenance code
            new EnrollmentRecord("SUB003", "SUB003", true, "Bad, Record", "18", "999",
                new DateTime(2026, 1, 1), null, "PLAN-C"),

            // Invalid: missing member ID and member name
            new EnrollmentRecord("", "", true, "", "18", "021",
                new DateTime(2026, 1, 1), null, "PLAN-D"),

            // Invalid: end date before start date
            new EnrollmentRecord("SUB005", "SUB005", true, "Date, Problem", "18", "021",
                new DateTime(2026, 6, 1), new DateTime(2026, 1, 1), "PLAN-E"),
        };

        var transformed = await pipeline.ExecuteAsync(records);

        // Write curated output for valid records
        if (transformed.Count > 0)
            await curatedWriter.WriteAsync(transformed, "edi834", "benefits-enrollment");

        // 2 valid, 3 invalid
        Assert.Equal(2, transformed.Count);

        var curatedFiles = Directory.GetFiles(Path.Combine(curatedDir, "edi834"), "*.json");
        var rejectedFiles = Directory.GetFiles(Path.Combine(rejectedDir, "edi834"), "*.json");

        Assert.NotEmpty(curatedFiles);
        Assert.NotEmpty(rejectedFiles);

        // Find most recent rejected file (not arbitrary filesystem order)
        var mostRecentRejected = rejectedFiles
            .Select(f => new FileInfo(f))
            .OrderByDescending(fi => fi.LastWriteTime)
            .First();
        var rejectedJson = await File.ReadAllTextAsync(mostRecentRejected.FullName);
        using var doc = JsonDocument.Parse(rejectedJson);
        Assert.Equal(3, doc.RootElement.GetArrayLength());
    }

    [Fact]
    public async Task EndToEnd_RejectedWriter_Writes_To_Data_Rejected()
    {
        // Writes to the real data/rejected/ directory so you can inspect the output.
        // Run: dotnet test --filter EndToEnd_RejectedWriter_Writes_To_Data_Rejected
        // Then open: data/rejected/edi834/
        var rejectedDir = Path.Combine(
            Directory.GetCurrentDirectory(), "..", "..", "..", "..", "..", "data", "rejected");

        var writer = new JsonRejectedOutputWriter(rejectedDir, NullLogger<JsonRejectedOutputWriter>.Instance);

        var records = new List<RejectedRecord<IPipelineRecord>>
        {
            new(new EnrollmentRecord("SUB999", "SUB999", true, "Invalid, Member", "18", "999",
                    new DateTime(2026, 1, 1), new DateTime(2025, 1, 1), "PLAN-X"),
                ["MaintenanceTypeCode '999' is not a valid X12 code",
                 "CoverageEndDate is before CoverageStartDate"]),

            new(new EnrollmentRecord("SUB888", "SUB888", true, "", "18", "021",
                    new DateTime(2026, 1, 1), null, "PLAN-Y"),
                ["MemberName is required"])
        };

        await writer.WriteAsync(records, "edi834", "benefits-enrollment");

        // Find the file we just wrote (most recent by timestamp, not arbitrary order)
        var files = Directory.GetFiles(Path.Combine(rejectedDir, "edi834"), "*.json");
        var mostRecent = files
            .Select(f => new FileInfo(f))
            .OrderByDescending(fi => fi.LastWriteTime)
            .First();
        
        _testOutput.WriteLine($"Checking most recent file: {mostRecent.FullName}");
        
        var json = await File.ReadAllTextAsync(mostRecent.FullName);
        using var doc = JsonDocument.Parse(json);
        Assert.Equal(2, doc.RootElement.GetArrayLength());
    }

    [Fact]
    public async Task Pipeline_Without_Enrichment_Stages_Still_Deduplicates()
    {
        var dedupOnly = new TransformPipeline(new List<ITransform>
        {
            new DeduplicationTransform()
        });

        var items = new List<IPipelineRecord>
        {
            new FeedItem("A", "https://example.com/1", "D", DateTime.UtcNow),
            new FeedItem("B", "https://example.com/1", "D", DateTime.UtcNow),
            new FeedItem("C", "https://example.com/2", "D", DateTime.UtcNow)
        };

        var result = await dedupOnly.ExecuteAsync(items);

        Assert.Equal(2, result.Count);
        Assert.All(result, r => Assert.Empty(r.Enrichment));
    }
}

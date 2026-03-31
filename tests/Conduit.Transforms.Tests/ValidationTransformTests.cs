using Conduit.Core.Models;
using Conduit.Core.Services;
using Conduit.Services;
using Conduit.Sources.Edi834.Models;
using Conduit.Transforms;
using Moq;

namespace Conduit.Transforms.Tests;

public class ValidationTransformTests
{
    private readonly Mock<IRejectedOutputWriter> _rejectedWriter;

    public ValidationTransformTests()
    {
        _rejectedWriter = new Mock<IRejectedOutputWriter>();
        _rejectedWriter
            .Setup(w => w.WriteAsync(It.IsAny<List<RejectedRecord<IPipelineRecord>>>(),
                It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task AllValidRecords_PassThrough_NothingRejected()
    {
        var validators = new List<IRecordValidator> { new EnrollmentRecordValidator() };
        var transform = new ValidationTransform(_rejectedWriter.Object, "edi834", "test", validators);

        var records = new List<TransformedRecord<IPipelineRecord>>
        {
            new(new EnrollmentRecord("SUB001", "SUB001", true, "Doe, Jane", "18", "021",
                new DateTime(2026, 1, 1), null, "PLAN-A"))
        };

        var result = await transform.ExecuteAsync(records);

        Assert.Single(result);
        _rejectedWriter.Verify(w => w.WriteAsync(
            It.IsAny<List<RejectedRecord<IPipelineRecord>>>(),
            It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task AllInvalidRecords_ReturnEmpty_AllWrittenToRejected()
    {
        var validators = new List<IRecordValidator> { new EnrollmentRecordValidator() };
        var transform = new ValidationTransform(_rejectedWriter.Object, "edi834", "test", validators);

        var records = new List<TransformedRecord<IPipelineRecord>>
        {
            new(new EnrollmentRecord("", "", true, "Doe, Jane", "18", "021",
                new DateTime(2026, 1, 1), null, "PLAN-A")),
            new(new EnrollmentRecord("SUB002", "SUB002", true, "", "18", "021",
                new DateTime(2026, 1, 1), null, "PLAN-B"))
        };

        var result = await transform.ExecuteAsync(records);

        Assert.Empty(result);
        _rejectedWriter.Verify(w => w.WriteAsync(
            It.Is<List<RejectedRecord<IPipelineRecord>>>(r => r.Count == 2),
            "edi834", "test"), Times.Once);
    }

    [Fact]
    public async Task MixedBatch_SplitsCorrectly()
    {
        var validators = new List<IRecordValidator> { new EnrollmentRecordValidator() };
        var transform = new ValidationTransform(_rejectedWriter.Object, "edi834", "test", validators);

        var records = new List<TransformedRecord<IPipelineRecord>>
        {
            new(new EnrollmentRecord("SUB001", "SUB001", true, "Doe, Jane", "18", "021",
                new DateTime(2026, 1, 1), null, "PLAN-A")),  // valid
            new(new EnrollmentRecord("", "", true, "Bad Record", "18", "021",
                new DateTime(2026, 1, 1), null, "PLAN-B"))   // invalid — no member/subscriber ID
        };

        var result = await transform.ExecuteAsync(records);

        Assert.Single(result);
        Assert.Equal("SUB001", result[0].Record.Id);
        _rejectedWriter.Verify(w => w.WriteAsync(
            It.Is<List<RejectedRecord<IPipelineRecord>>>(r => r.Count == 1),
            "edi834", "test"), Times.Once);
    }

    [Fact]
    public async Task NoApplicableValidator_RecordPassesThrough()
    {
        // RSS validator does not apply to EnrollmentRecord
        var validators = new List<IRecordValidator> { new FeedItemValidator() };
        var transform = new ValidationTransform(_rejectedWriter.Object, "edi834", "test", validators);

        var records = new List<TransformedRecord<IPipelineRecord>>
        {
            new(new EnrollmentRecord("SUB001", "SUB001", true, "Doe, Jane", "18", "021",
                new DateTime(2026, 1, 1), null, "PLAN-A"))
        };

        var result = await transform.ExecuteAsync(records);

        Assert.Single(result);
    }

    [Fact]
    public async Task RejectedRecord_ContainsErrors()
    {
        List<RejectedRecord<IPipelineRecord>>? captured = null;
        _rejectedWriter
            .Setup(w => w.WriteAsync(It.IsAny<List<RejectedRecord<IPipelineRecord>>>(),
                It.IsAny<string>(), It.IsAny<string>()))
            .Callback<List<RejectedRecord<IPipelineRecord>>, string, string>((r, _, _) => captured = r)
            .Returns(Task.CompletedTask);

        var validators = new List<IRecordValidator> { new EnrollmentRecordValidator() };
        var transform = new ValidationTransform(_rejectedWriter.Object, "edi834", "test", validators);

        var records = new List<TransformedRecord<IPipelineRecord>>
        {
            new(new EnrollmentRecord("", "", true, "Doe, Jane", "18", "999",
                new DateTime(2026, 1, 1), null, "PLAN-A"))
        };

        await transform.ExecuteAsync(records);

        Assert.NotNull(captured);
        Assert.Single(captured);
        Assert.NotEmpty(captured[0].Errors);
    }
}

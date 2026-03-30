using Conduit.Core.Models;
using Conduit.Core.Services;

namespace Conduit.Transforms.Tests;

public class TransformPipelineTests
{
    [Fact]
    public async Task Empty_Input_Returns_Empty_Output()
    {
        var pipeline = new TransformPipeline(new List<ITransform>());
        var input = new List<IPipelineRecord>();

        var result = await pipeline.ExecuteAsync(input);

        Assert.Empty(result);
    }

    [Fact]
    public async Task No_Stages_Returns_Wrapped_Records()
    {
        var pipeline = new TransformPipeline(new List<ITransform>());
        var items = new List<IPipelineRecord>
        {
            new FeedItem("Title 1", "https://example.com/1", "Desc 1", DateTime.UtcNow),
            new FeedItem("Title 2", "https://example.com/2", "Desc 2", DateTime.UtcNow)
        };

        var result = await pipeline.ExecuteAsync(items);

        Assert.Equal(2, result.Count);
        Assert.All(result, r => Assert.Empty(r.Enrichment));
    }

    [Fact]
    public async Task Stages_Execute_In_Order()
    {
        var executionOrder = new List<string>();

        var stage1 = new TestStage("first", executionOrder);
        var stage2 = new TestStage("second", executionOrder);
        var stage3 = new TestStage("third", executionOrder);

        var pipeline = new TransformPipeline(new List<ITransform> { stage1, stage2, stage3 });
        var items = new List<IPipelineRecord>
        {
            new FeedItem("Title", "https://example.com", "Desc", DateTime.UtcNow)
        };

        await pipeline.ExecuteAsync(items);

        Assert.Equal(["first", "second", "third"], executionOrder);
    }

    [Fact]
    public async Task Stage_Can_Filter_Records()
    {
        var filterStage = new FilterStage(r => r.Id != "https://example.com/2");

        var pipeline = new TransformPipeline(new List<ITransform> { filterStage });
        var items = new List<IPipelineRecord>
        {
            new FeedItem("Keep", "https://example.com/1", "Desc", DateTime.UtcNow),
            new FeedItem("Remove", "https://example.com/2", "Desc", DateTime.UtcNow),
            new FeedItem("Keep", "https://example.com/3", "Desc", DateTime.UtcNow)
        };

        var result = await pipeline.ExecuteAsync(items);

        Assert.Equal(2, result.Count);
        Assert.DoesNotContain(result, r => r.Record.Id == "https://example.com/2");
    }

    [Fact]
    public async Task Stage_Can_Add_Enrichment()
    {
        var enrichStage = new EnrichStage("tag", "test-value");

        var pipeline = new TransformPipeline(new List<ITransform> { enrichStage });
        var items = new List<IPipelineRecord>
        {
            new FeedItem("Title", "https://example.com", "Desc", DateTime.UtcNow)
        };

        var result = await pipeline.ExecuteAsync(items);

        Assert.Single(result);
        Assert.Equal("test-value", result[0].Enrichment["tag"]);
    }

    [Fact]
    public async Task Multiple_Stages_Compose_Enrichment()
    {
        var stage1 = new EnrichStage("keywords", "ai");
        var stage2 = new EnrichStage("category", "tech");

        var pipeline = new TransformPipeline(new List<ITransform> { stage1, stage2 });
        var items = new List<IPipelineRecord>
        {
            new FeedItem("Title", "https://example.com", "Desc", DateTime.UtcNow)
        };

        var result = await pipeline.ExecuteAsync(items);

        Assert.Single(result);
        Assert.Equal("ai", result[0].Enrichment["keywords"]);
        Assert.Equal("tech", result[0].Enrichment["category"]);
    }

    // -- Test helpers --

    private sealed class TestStage : ITransform
    {
        private readonly string _name;
        private readonly List<string> _executionOrder;

        public TestStage(string name, List<string> executionOrder)
        {
            _name = name;
            _executionOrder = executionOrder;
        }

        public Task<List<TransformedRecord<IPipelineRecord>>> ExecuteAsync(
            List<TransformedRecord<IPipelineRecord>> records)
        {
            _executionOrder.Add(_name);
            return Task.FromResult(records);
        }
    }

    private sealed class FilterStage : ITransform
    {
        private readonly Func<IPipelineRecord, bool> _predicate;

        public FilterStage(Func<IPipelineRecord, bool> predicate)
        {
            _predicate = predicate;
        }

        public Task<List<TransformedRecord<IPipelineRecord>>> ExecuteAsync(
            List<TransformedRecord<IPipelineRecord>> records)
        {
            var filtered = records.Where(r => _predicate(r.Record)).ToList();
            return Task.FromResult(filtered);
        }
    }

    private sealed class EnrichStage : ITransform
    {
        private readonly string _key;
        private readonly object _value;

        public EnrichStage(string key, object value)
        {
            _key = key;
            _value = value;
        }

        public Task<List<TransformedRecord<IPipelineRecord>>> ExecuteAsync(
            List<TransformedRecord<IPipelineRecord>> records)
        {
            foreach (var record in records)
            {
                record.Enrichment[_key] = _value;
            }
            return Task.FromResult(records);
        }
    }
}

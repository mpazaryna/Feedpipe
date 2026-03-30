using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Conduit.Core.Services;
using Conduit.Sources.Rss.Services;
using Conduit.Sources.Edi834.Services;
using Conduit.Sources.Zotero.Services;
using Conduit.Transforms;

namespace Conduit.Services;

/// <summary>
/// Shared DI registration for the Conduit pipeline. Used by all entry points
/// (Console, Worker, API) to ensure consistent adapter and transform wiring.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all source adapters as keyed services, the raw output writer,
    /// the curated output writer, the rejected output writer, and the enrichment transforms.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="rawOutputDir">Directory for raw output (e.g., "data/raw").</param>
    /// <param name="curatedOutputDir">Directory for curated output (e.g., "data/curated").</param>
    /// <param name="rejectedOutputDir">Directory for rejected output (e.g., "data/rejected").</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddConduitPipeline(
        this IServiceCollection services,
        string rawOutputDir,
        string curatedOutputDir,
        string rejectedOutputDir = "data/rejected")
    {
        // Source adapters
        services.AddKeyedScoped<ISourceAdapter>("rss", (sp, _) =>
            new FeedSourceAdapter(
                sp.GetRequiredService<IHttpClientFactory>().CreateClient(),
                sp.GetRequiredService<ILogger<FeedSourceAdapter>>()));
        services.AddKeyedScoped<ISourceAdapter, Edi834SourceAdapter>("edi834");
        services.AddKeyedScoped<ISourceAdapter>("zotero", (sp, _) =>
            new ZoteroSourceAdapter(
                sp.GetRequiredService<IHttpClientFactory>().CreateClient(),
                sp.GetRequiredService<ILogger<ZoteroSourceAdapter>>()));

        // Raw output writer
        services.AddSingleton<IOutputWriter>(sp =>
            new JsonOutputWriter(rawOutputDir,
                sp.GetRequiredService<ILogger<JsonOutputWriter>>()));

        // Curated output writer
        services.AddSingleton<ITransformedOutputWriter>(sp =>
            new JsonTransformedOutputWriter(curatedOutputDir,
                sp.GetRequiredService<ILogger<JsonTransformedOutputWriter>>()));

        // Rejected output writer
        services.AddSingleton<IRejectedOutputWriter>(sp =>
            new JsonRejectedOutputWriter(rejectedOutputDir,
                sp.GetRequiredService<ILogger<JsonRejectedOutputWriter>>()));

        // Enrichment transforms (shared across all sources)
        services.AddSingleton<IReadOnlyList<ITransform>>(new List<ITransform>
        {
            new RssEnrichmentTransform(),
            new Edi834EnrichmentTransform(),
            new ZoteroEnrichmentTransform()
        });

        return services;
    }
}

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
/// <remarks>
/// <para>
/// <b>Extension method on <c>IServiceCollection</c>.</b> The <c>this</c> keyword on the
/// first parameter makes <c>AddConduitPipeline</c> callable as if it were an instance
/// method on any <c>IServiceCollection</c>. This is the standard pattern for organizing
/// DI registration — ASP.NET itself uses it (e.g., <c>services.AddLogging(...)</c>).
/// </para>
/// <para>
/// <b>Keyed services.</b> Source adapters are registered with a string key ("rss",
/// "edi834", "zotero"). At runtime, the pipeline resolves the correct adapter by calling
/// <c>GetRequiredKeyedService&lt;ISourceAdapter&gt;(source.Type)</c>. This avoids a big
/// <c>switch</c> statement and makes adding a new source type a registration-only change.
/// </para>
/// <para>
/// <b>Service lifetimes.</b> Adapters are registered as <c>Scoped</c> — a new instance
/// per DI scope (one request or one pipeline run). Writers are <c>Singleton</c> — one
/// instance for the application's lifetime, shared across all sources. Transforms and
/// validators are also registered as singletons because they are stateless.
/// </para>
/// </remarks>
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

        // Record validators (shared across all sources; each declares which type it handles)
        services.AddSingleton<IReadOnlyList<IRecordValidator>>(new List<IRecordValidator>
        {
            new FeedItemValidator(),
            new EnrollmentRecordValidator(),
            new ResearchRecordValidator()
        });

        return services;
    }
}

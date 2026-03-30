using System.Text.RegularExpressions;
using Conduit.Core.Models;
using Conduit.Core.Services;
using Conduit.Sources.Zotero.Models;

namespace Conduit.Transforms;

/// <summary>
/// Enrichment stage that derives research domain tags from Zotero
/// paper abstracts using keyword matching against known domain vocabularies.
/// </summary>
public partial class ZoteroEnrichmentTransform : ITransform
{
    private static readonly Dictionary<string, string[]> DomainVocabulary = new()
    {
        ["machine-learning"] = ["machine learning", "deep learning", "neural network", "training", "classification", "regression", "supervised", "unsupervised", "reinforcement learning", "gradient"],
        ["natural-language-processing"] = ["natural language", "nlp", "text mining", "sentiment", "language model", "tokenization", "embedding", "transformer"],
        ["computer-vision"] = ["image", "object detection", "segmentation", "convolutional", "visual", "recognition", "pixel"],
        ["bioinformatics"] = ["protein", "genome", "dna", "rna", "gene", "molecular", "biological", "amino acid", "sequence alignment"],
        ["drug-discovery"] = ["drug", "pharmaceutical", "compound", "binding", "inhibitor", "therapeutic", "clinical trial"],
        ["robotics"] = ["robot", "autonomous", "navigation", "manipulation", "sensor", "actuator", "control system"],
        ["security"] = ["cryptograph", "encryption", "vulnerabilit", "attack", "malware", "authentication", "privacy"],
        ["distributed-systems"] = ["distributed", "consensus", "replication", "fault tolerance", "scalab", "cluster", "parallel"],
        ["quantum-computing"] = ["quantum", "qubit", "superposition", "entanglement", "quantum circuit"],
        ["data-science"] = ["dataset", "statistical", "analytics", "visualization", "correlation", "hypothesis"]
    };

    /// <inheritdoc />
    public Task<List<TransformedRecord<IPipelineRecord>>> ExecuteAsync(
        List<TransformedRecord<IPipelineRecord>> records)
    {
        foreach (var record in records)
        {
            if (record.Record is ResearchRecord research)
            {
                record.Enrichment["domainTags"] = ExtractDomainTags(research);
            }
        }

        return Task.FromResult(records);
    }

    private static List<string> ExtractDomainTags(ResearchRecord record)
    {
        var text = record.Abstract.ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(text))
        {
            return [];
        }

        var matchedDomains = new List<string>();

        foreach (var (domain, terms) in DomainVocabulary)
        {
            if (terms.Any(term => text.Contains(term, StringComparison.Ordinal)))
            {
                matchedDomains.Add(domain);
            }
        }

        return matchedDomains;
    }
}

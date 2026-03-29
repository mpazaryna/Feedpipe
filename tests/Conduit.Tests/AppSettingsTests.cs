using Conduit.Models;

namespace Conduit.Tests;

/// <summary>
/// Tests for configuration models to verify defaults and initialization.
/// </summary>
public class AppSettingsTests
{
    [Fact]
    public void AppSettings_HasCorrectDefaults()
    {
        var settings = new AppSettings();

        Assert.Equal("data", settings.OutputDir);
        Assert.Equal("logs", settings.LogsDir);
        Assert.Empty(settings.Sources);
    }

    [Fact]
    public void AppSettings_AcceptsCustomValues()
    {
        var settings = new AppSettings
        {
            OutputDir = "custom-output",
            LogsDir = "custom-logs",
            Sources = [new SourceSettings { Location = "https://example.com", Name = "test" }]
        };

        Assert.Equal("custom-output", settings.OutputDir);
        Assert.Equal("custom-logs", settings.LogsDir);
        Assert.Single(settings.Sources);
    }

    [Fact]
    public void SourceSettings_DefaultsTypeToRss()
    {
        var source = new SourceSettings { Location = "https://example.com", Name = "test" };

        Assert.Equal("rss", source.Type);
    }

    [Fact]
    public void SourceSettings_AcceptsCustomType()
    {
        var source = new SourceSettings { Location = "data/file.edi", Name = "enrollment", Type = "edi834" };

        Assert.Equal("edi834", source.Type);
        Assert.Equal("data/file.edi", source.Location);
        Assert.Equal("enrollment", source.Name);
    }
}

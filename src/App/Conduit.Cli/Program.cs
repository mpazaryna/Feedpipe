// -----------------------------------------------------------------------
// Conduit CLI - Command-Line Tool for Querying Pipeline Output
//
// A read-only tool that works with the JSON files produced by the pipeline.
// Operates entirely on local data -- no network requests.
//
// COMMANDS:
//   conduit search <term> [--dir <path>]     Search items by keyword
//   conduit list [--dir <path>] [--limit <n>] List recent items
//   conduit stats [--dir <path>]              Show ingestion statistics
//
// RUN WITH: dotnet run --project src/Conduit.Cli -- list
//           dotnet run --project src/Conduit.Cli -- search "AI"
//           dotnet run --project src/Conduit.Cli -- stats
// -----------------------------------------------------------------------

using System.CommandLine;
using System.Text.Json;

var dirOption = new Option<string>("--dir", () => "data/curated", "Directory containing curated output JSON files");

var rootCommand = new RootCommand("Conduit CLI - search and filter pipeline output");

// ---- SEARCH COMMAND ----
var searchTerm = new Argument<string>("term", "Text to search for in titles and descriptions");
var searchCommand = new Command("search", "Search items by keyword") { searchTerm, dirOption };
searchCommand.SetHandler((term, dir) =>
{
    if (!Directory.Exists(dir))
    {
        Console.Error.WriteLine($"Directory not found: {dir}");
        return;
    }

    var files = Directory.EnumerateFiles(dir, "*.json", SearchOption.AllDirectories).ToArray();
    var matchCount = 0;

    foreach (var file in files)
    {
        var json = File.ReadAllText(file);
        using var doc = JsonDocument.Parse(json);

        foreach (var element in doc.RootElement.EnumerateArray())
        {
            var record = element.TryGetProperty("record", out var r) ? r : element;
            var title = record.TryGetProperty("title", out var t) ? t.GetString() ?? "" : "";
            var description = record.TryGetProperty("description", out var d) ? d.GetString() ?? "" : "";
            var link = record.TryGetProperty("link", out var l) ? l.GetString() ?? "" : "";
            var date = record.TryGetProperty("publishedDate", out var p) ? p.GetString() ?? "" : "";

            if (title.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                description.Contains(term, StringComparison.OrdinalIgnoreCase))
            {
                matchCount++;
                Console.WriteLine($"[{date}] {title}");
                if (!string.IsNullOrEmpty(link))
                    Console.WriteLine($"  {link}");

                if (element.TryGetProperty("enrichment", out var enrichment))
                {
                    foreach (var prop in enrichment.EnumerateObject())
                    {
                        Console.WriteLine($"  {prop.Name}: {prop.Value}");
                    }
                }
                Console.WriteLine();
            }
        }
    }

    Console.WriteLine($"Found {matchCount} result(s).");
}, searchTerm, dirOption);

// ---- LIST COMMAND ----
var limitOption = new Option<int>("--limit", () => 10, "Number of items to show");
var listCommand = new Command("list", "List recent items") { dirOption, limitOption };
listCommand.SetHandler((dir, limit) =>
{
    if (!Directory.Exists(dir))
    {
        Console.Error.WriteLine($"Directory not found: {dir}");
        return;
    }

    var latestFile = Directory.EnumerateFiles(dir, "*.json", SearchOption.AllDirectories)
        .OrderByDescending(f => f)
        .FirstOrDefault();

    if (latestFile is null)
    {
        Console.WriteLine("No data found.");
        return;
    }

    var json = File.ReadAllText(latestFile);
    using var doc = JsonDocument.Parse(json);
    var items = doc.RootElement.EnumerateArray().ToList();

    Console.WriteLine($"Latest output: {Path.GetFileName(latestFile)}");
    Console.WriteLine(new string('-', 60));

    foreach (var element in items.Take(limit))
    {
        var record = element.TryGetProperty("record", out var r) ? r : element;
        var title = record.TryGetProperty("title", out var t) ? t.GetString() ?? "" : "";
        var link = record.TryGetProperty("link", out var l) ? l.GetString() ?? "" : "";
        var date = record.TryGetProperty("publishedDate", out var p) ? p.GetString() ?? "" : "";

        Console.WriteLine($"[{date}] {title}");
        if (!string.IsNullOrEmpty(link))
            Console.WriteLine($"  {link}");

        if (element.TryGetProperty("enrichment", out var enrichment))
        {
            foreach (var prop in enrichment.EnumerateObject())
            {
                Console.WriteLine($"  {prop.Name}: {prop.Value}");
            }
        }
        Console.WriteLine();
    }

    Console.WriteLine($"Showing {Math.Min(limit, items.Count)} of {items.Count} items.");
}, dirOption, limitOption);

// ---- STATS COMMAND ----
var statsCommand = new Command("stats", "Show ingestion statistics") { dirOption };
statsCommand.SetHandler((dir) =>
{
    if (!Directory.Exists(dir))
    {
        Console.Error.WriteLine($"Directory not found: {dir}");
        return;
    }

    var files = Directory.EnumerateFiles(dir, "*.json", SearchOption.AllDirectories).ToArray();
    var totalItems = 0;

    foreach (var file in files)
    {
        var json = File.ReadAllText(file);
        using var doc = JsonDocument.Parse(json);
        totalItems += doc.RootElement.GetArrayLength();
    }

    Console.WriteLine($"Output files:   {files.Length}");
    Console.WriteLine($"Total items:    {totalItems}");
    Console.WriteLine($"Directory:      {Path.GetFullPath(dir)}");
}, dirOption);

rootCommand.AddCommand(searchCommand);
rootCommand.AddCommand(listCommand);
rootCommand.AddCommand(statsCommand);

return await rootCommand.InvokeAsync(args);

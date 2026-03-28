// -----------------------------------------------------------------------
// Feedpipe CLI - Command-Line Tool for Querying Fetched Data
//
// A read-only tool that works with the JSON files produced by the pipeline.
// Unlike the other projects, this doesn't fetch from the network -- it
// operates entirely on local data, making it fast and offline-capable.
//
// COMMANDS:
//   feedpipe search <term> [--dir <path>]     Search items by keyword
//   feedpipe list [--dir <path>] [--limit <n>] List recent items
//   feedpipe stats [--dir <path>]              Show fetch statistics
//
// BUILT WITH System.CommandLine:
//
// System.CommandLine is Microsoft's official CLI parsing library. It handles
// argument parsing, help text generation, tab completion, and validation.
// The pattern is:
//   1. Define Options (--flag) and Arguments (positional values)
//   2. Create Commands and attach options/arguments to them
//   3. Set a handler (lambda) for each command
//   4. Add commands to a RootCommand and invoke it
//
// This is analogous to Python's click or argparse libraries. The key
// difference is that System.CommandLine is strongly typed -- option values
// are parsed directly into their target types (string, int, etc.).
//
// NOTE: This project only depends on Feedpipe.Core (for the FeedItem model),
// not on Feedpipe (the main app). It reads JSON files directly rather than
// going through IFeedWriter. This is intentional -- the CLI is a lightweight
// consumer of the pipeline's output, not a participant in the pipeline itself.
//
// RUN WITH: dotnet run --project src/Feedpipe.Cli -- list
//           dotnet run --project src/Feedpipe.Cli -- search "AI"
//           dotnet run --project src/Feedpipe.Cli -- stats
// -----------------------------------------------------------------------

using System.CommandLine;
using System.CommandLine.Invocation;
using System.Text.Json;
using Feedpipe.Core.Models;

// JsonSerializerOptions with case-insensitive matching. This ensures
// deserialization works regardless of whether the JSON uses PascalCase
// (C# default) or camelCase (common in web APIs).
var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

// Shared option used by all commands. Defining it once avoids duplication
// and ensures consistent behavior across commands.
var dirOption = new Option<string>("--dir", () => "fetched", "Directory containing fetched JSON files");

var rootCommand = new RootCommand("Feedpipe CLI - search and filter fetched feed data");

// ---- SEARCH COMMAND ----
// Scans all JSON files in the directory for items matching the search term
// in either the title or description (case-insensitive).
var searchTerm = new Argument<string>("term", "Text to search for in titles and descriptions");
var searchCommand = new Command("search", "Search fetched items by keyword") { searchTerm, dirOption };
searchCommand.SetHandler((term, dir) =>
{
    if (!Directory.Exists(dir))
    {
        Console.Error.WriteLine($"Directory not found: {dir}");
        return;
    }

    var files = Directory.GetFiles(dir, "*.json");
    var matchCount = 0;

    foreach (var file in files)
    {
        var json = File.ReadAllText(file);
        var items = JsonSerializer.Deserialize<List<FeedItem>>(json, jsonOptions) ?? [];

        var matches = items.Where(i =>
            i.Title.Contains(term, StringComparison.OrdinalIgnoreCase) ||
            i.Description.Contains(term, StringComparison.OrdinalIgnoreCase));

        foreach (var item in matches)
        {
            matchCount++;
            Console.WriteLine($"[{item.PublishedDate:yyyy-MM-dd}] {item.Title}");
            Console.WriteLine($"  {item.Link}");
            Console.WriteLine();
        }
    }

    Console.WriteLine($"Found {matchCount} result(s).");
}, searchTerm, dirOption);

// ---- LIST COMMAND ----
// Shows items from the most recent fetch file, sorted by filename
// (which includes a timestamp). The --limit option caps the output.
var limitOption = new Option<int>("--limit", () => 10, "Number of items to show");
var listCommand = new Command("list", "List recent fetched items") { dirOption, limitOption };
listCommand.SetHandler((dir, limit) =>
{
    if (!Directory.Exists(dir))
    {
        Console.Error.WriteLine($"Directory not found: {dir}");
        return;
    }

    var latestFile = Directory.GetFiles(dir, "*.json")
        .OrderByDescending(f => f)
        .FirstOrDefault();

    if (latestFile is null)
    {
        Console.WriteLine("No fetched data found.");
        return;
    }

    var json = File.ReadAllText(latestFile);
    var items = JsonSerializer.Deserialize<List<FeedItem>>(json, jsonOptions) ?? [];

    Console.WriteLine($"Latest fetch: {Path.GetFileName(latestFile)}");
    Console.WriteLine(new string('-', 60));

    foreach (var item in items.Take(limit))
    {
        Console.WriteLine($"[{item.PublishedDate:yyyy-MM-dd}] {item.Title}");
        Console.WriteLine($"  {item.Link}");
        Console.WriteLine();
    }

    Console.WriteLine($"Showing {Math.Min(limit, items.Count)} of {items.Count} items.");
}, dirOption, limitOption);

// ---- STATS COMMAND ----
// Provides a quick summary of the fetched data directory: file count,
// total items, and the resolved directory path.
var statsCommand = new Command("stats", "Show fetch statistics") { dirOption };
statsCommand.SetHandler((dir) =>
{
    if (!Directory.Exists(dir))
    {
        Console.Error.WriteLine($"Directory not found: {dir}");
        return;
    }

    var files = Directory.GetFiles(dir, "*.json");
    var totalItems = 0;

    foreach (var file in files)
    {
        var json = File.ReadAllText(file);
        var items = JsonSerializer.Deserialize<List<FeedItem>>(json, jsonOptions) ?? [];
        totalItems += items.Count;
    }

    Console.WriteLine($"Fetched files:  {files.Length}");
    Console.WriteLine($"Total items:    {totalItems}");
    Console.WriteLine($"Directory:      {Path.GetFullPath(dir)}");
}, dirOption);

rootCommand.AddCommand(searchCommand);
rootCommand.AddCommand(listCommand);
rootCommand.AddCommand(statsCommand);

return await rootCommand.InvokeAsync(args);

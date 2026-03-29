# Learning: .NET Fundamentals

A reference for the core .NET concepts encountered while building Conduit.

## The .NET CLI

Everything runs through one tool: `dotnet`. No separate package manager, no separate build tool, no separate test runner.

```bash
dotnet new console -n MyApp     # Create a project
dotnet restore                  # Install dependencies (happens automatically on build)
dotnet build                    # Compile
dotnet test                     # Run tests
dotnet run --project src/App    # Run a specific project
dotnet publish -c Release       # Package for deployment
```

Coming from Python: `dotnet` replaces `uv`, `pytest`, `ruff`, and the interpreter itself. One tool, one command surface.

## Project Files (.csproj)

The `.csproj` file is the equivalent of `pyproject.toml`. It declares:
- Target framework (`net10.0`)
- Dependencies (NuGet packages via `<PackageReference>`)
- Project references (other projects in the solution via `<ProjectReference>`)
- Build settings (`<OutputType>`, `<RootNamespace>`, etc.)

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <ItemGroup>
    <PackageReference Include="Serilog" Version="4.2.0" />
    <ProjectReference Include="../Conduit.Core/Conduit.Core.csproj" />
  </ItemGroup>
</Project>
```

## Solution Files (.slnx)

A solution groups multiple projects. It's the level above `pyproject.toml` -- like a monorepo workspace. `dotnet build` from the solution root builds everything. `dotnet test` runs all test projects.

Solution folders (like `/src/Core/`, `/src/Adapters/`) organize projects visually in IDEs but don't affect namespaces or build output.

## Directory.Build.props

Shared build settings that apply to every project in the solution. Define it once at the root, and all `.csproj` files inherit from it. We use it for:
- `TreatWarningsAsErrors` -- no warnings slip through
- `AnalysisLevel` -- static code analysis
- `GenerateDocumentationFile` -- enforces XML doc comments
- `TargetFramework` -- so individual projects don't repeat it

This is similar to a root-level linter config that applies to all packages.

## Namespaces

Namespaces organize code logically. They don't have to match folder structure, but by convention they do. A file in `src/Adapters/Conduit.Sources.Rss/Services/` has namespace `Conduit.Sources.Rss.Services`.

`using` statements import namespaces (like Python's `import`). `ImplicitUsings` auto-imports common ones (`System`, `System.Collections.Generic`, `System.Linq`, etc.) so you don't write them in every file.

## Records vs Classes

```csharp
// Record -- immutable, value equality, one line
public record FeedItem(string Title, string Link, string Description, DateTime PublishedDate);

// Class -- mutable by default, reference equality, more ceremony
public class AppSettings
{
    public string OutputDir { get; init; } = "data";
    public List<SourceSettings> Sources { get; init; } = [];
}
```

Use records for data that shouldn't change after creation (like pipeline records). Use classes for configuration and services. Records are the .NET equivalent of Python's `@dataclass(frozen=True)`.

## async/await

Almost identical to Python's `asyncio`, but built into the runtime rather than an event loop you opt into.

```csharp
// .NET
public async Task<List<FeedItem>> IngestAsync(string location)
{
    var response = await _httpClient.GetStringAsync(location);
    return ParseItems(response);
}
```

```python
# Python equivalent
async def ingest(self, location: str) -> list[FeedItem]:
    response = await self._client.get(location)
    return self.parse_items(response.text)
```

Key difference: .NET methods return `Task<T>` (a promise of a value). Python returns a coroutine. Both use `await` to unwrap the result.

## Nullable Reference Types

When `<Nullable>enable</Nullable>` is set (which it is in our project), the compiler warns if you might hit a null reference. The `?` suffix marks a type as explicitly nullable:

```csharp
string name = "hello";      // Cannot be null -- compiler enforces this
string? maybe = null;        // Explicitly nullable -- compiler knows
```

The `?.` (null-conditional) and `??` (null-coalescing) operators handle nulls safely:

```csharp
var title = item.Element("title")?.Value ?? "(no title)";
// If Element returns null, ?. short-circuits. ?? provides a fallback.
```

This is a safety net Python doesn't have -- null reference bugs are caught at compile time, not at runtime.

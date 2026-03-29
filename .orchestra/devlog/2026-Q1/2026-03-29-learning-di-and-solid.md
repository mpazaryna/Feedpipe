# Learning: Dependency Injection and SOLID Patterns

How .NET wires things together, and why it matters.

## Dependency Injection (DI)

In Python, you import a module and call its functions. In .NET, you declare what you need in your constructor, and the DI container provides it.

```csharp
// The class declares its dependencies
public class RssSourceAdapter : ISourceAdapter
{
    public RssSourceAdapter(HttpClient httpClient, ILogger<RssSourceAdapter> logger)
    {
        _httpClient = httpClient;  // I need an HTTP client
        _logger = logger;          // I need a logger
    }
}
```

```csharp
// Somewhere else, the DI container is told how to create things
services.AddHttpClient();
services.AddKeyedScoped<ISourceAdapter, RssSourceAdapter>("rss");
```

The class never creates its own dependencies. It receives them. This means:
- **Tests can substitute mocks** without changing the class
- **Swapping implementations** is a config change, not a code change
- **Dependencies are explicit** -- you can see what a class needs by reading its constructor

## The DI Container (ServiceCollection)

```csharp
var services = new ServiceCollection();

// Register a service -- "when someone asks for ISourceAdapter, give them RssSourceAdapter"
services.AddKeyedScoped<ISourceAdapter, RssSourceAdapter>("rss");

// Register with a factory -- "when someone asks for IOutputWriter, create it like this"
services.AddSingleton<IOutputWriter>(sp =>
    new JsonOutputWriter(outputDir, sp.GetRequiredService<ILogger<JsonOutputWriter>>()));

var provider = services.BuildServiceProvider();

// Resolve -- "give me the thing registered for ISourceAdapter with key 'rss'"
var adapter = provider.GetRequiredKeyedService<ISourceAdapter>("rss");
```

Lifetimes:
- `AddSingleton` -- one instance for the entire app lifetime
- `AddScoped` -- one instance per scope (per request in a web app)
- `AddTransient` -- new instance every time

## Keyed Services

When you have multiple implementations of the same interface, keyed services let you register them by name:

```csharp
services.AddKeyedScoped<ISourceAdapter, RssSourceAdapter>("rss");
services.AddKeyedScoped<ISourceAdapter, Edi834SourceAdapter>("edi834");
services.AddKeyedScoped<ISourceAdapter, ZoteroSourceAdapter>("zotero");

// Resolve the right one based on config
var adapter = provider.GetRequiredKeyedService<ISourceAdapter>(source.Type);
```

This is how Conduit routes to the correct adapter at runtime without knowing the concrete types at compile time.

## SOLID Principles (as seen in Conduit)

### Single Responsibility
`RssSourceAdapter` fetches and parses RSS. `JsonOutputWriter` writes JSON. They don't do each other's job.

### Open/Closed
Adding the EDI 834 adapter required zero changes to the existing RSS adapter or the pipeline. We extended the system by adding new code, not modifying existing code.

### Liskov Substitution
Anywhere the pipeline expects `ISourceAdapter`, any implementation works. The pipeline calls `adapter.IngestAsync(location)` and doesn't care if it's RSS, 834, or Zotero behind the interface.

### Interface Segregation
`ISourceAdapter` has one method: `IngestAsync`. `IOutputWriter` has one method: `WriteAsync`. Small, focused interfaces rather than one big interface with 20 methods.

### Dependency Inversion
`Program.cs` depends on `ISourceAdapter`, not `RssSourceAdapter`. The DI container decides which concrete class to use. High-level code (pipeline) doesn't depend on low-level code (adapters).

## Interfaces vs Abstract Classes

In Python, you'd use ABC (abstract base class) or Protocol. In .NET:

```csharp
// Interface -- defines a contract, no implementation
public interface ISourceAdapter
{
    Task<List<IPipelineRecord>> IngestAsync(string location);
}

// Any class can implement it
public class RssSourceAdapter : ISourceAdapter { ... }
public class Edi834SourceAdapter : ISourceAdapter { ... }
```

Interfaces are preferred over abstract classes in .NET because:
- A class can implement multiple interfaces but only extend one base class
- Interfaces work naturally with DI
- They define the smallest possible contract

## IOptions<T> Pattern

Instead of injecting config values directly, .NET uses a wrapper:

```csharp
public class Worker(IOptions<AppSettings> settings, ...)
{
    // settings.Value.Sources gives you the typed config
    foreach (var source in settings.Value.Sources) { ... }
}
```

This supports validation, hot-reload, and named options. It's more ceremony than Python's `os.getenv()`, but it catches config errors at startup rather than at runtime.

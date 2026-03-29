# Learning: .NET Testing Patterns

How testing works in .NET, compared to Python/pytest.

## Framework: xUnit

xUnit is the most common .NET test framework. It's the closest in philosophy to pytest.

| pytest | xUnit |
|--------|-------|
| `def test_something():` | `[Fact] public async Task Something()` |
| `assert x == y` | `Assert.Equal(expected, actual)` |
| `@pytest.fixture` | Constructor + `IDisposable` |
| `conftest.py` | `GlobalUsings.cs` |
| `@pytest.mark.integration` | `[Trait("Category", "Integration")]` |
| `tmp_path` | `Path.GetTempPath()` + manual cleanup |

## Test Structure

```csharp
public class RssSourceAdapterTests
{
    [Fact]  // Marks this as a test (like @pytest.mark or def test_)
    public async Task IngestAsync_ParsesItemsFromRss()
    {
        // Arrange
        var httpClient = CreateMockHttpClient(SampleRss);
        var adapter = new RssSourceAdapter(httpClient, NullLogger<RssSourceAdapter>.Instance);

        // Act
        var items = await adapter.IngestAsync("https://example.com/feed");

        // Assert
        Assert.Equal(2, items.Count);
    }
}
```

The naming convention `MethodUnderTest_Scenario` makes test output readable.

## Mocking HTTP with Moq

In Python, you'd use `unittest.mock.patch` to replace a module-level function. In .NET, you mock the `HttpMessageHandler` that sits behind `HttpClient`:

```csharp
private static HttpClient CreateMockHttpClient(string responseContent)
{
    var handler = new Mock<HttpMessageHandler>();
    handler.Protected()
        .Setup<Task<HttpResponseMessage>>(
            "SendAsync",
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>())
        .ReturnsAsync(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(responseContent, Encoding.UTF8, "application/xml")
        });

    return new HttpClient(handler.Object);
}
```

This looks verbose, but it's the standard pattern. You see it in every .NET codebase that tests HTTP-dependent code. The `.Protected()` call is needed because `SendAsync` is a protected method on `HttpMessageHandler`.

## NullLogger<T>

When testing a class that requires `ILogger<T>`, you don't want log output cluttering your tests. `NullLogger<T>.Instance` is a built-in no-op logger:

```csharp
var adapter = new RssSourceAdapter(httpClient, NullLogger<RssSourceAdapter>.Instance);
```

This satisfies the dependency without producing any output. It's the .NET equivalent of not configuring a logger in your test setup.

## Test Cleanup with IDisposable

xUnit creates a new test class instance for each test (like pytest). For cleanup:

```csharp
public class JsonOutputWriterTests : IDisposable
{
    private readonly string _tempDir;

    public JsonOutputWriterTests()  // Runs before each test
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"conduit-tests-{Guid.NewGuid()}");
    }

    public void Dispose()  // Runs after each test
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
        GC.SuppressFinalize(this);
    }
}
```

The `Guid.NewGuid()` ensures each test gets a unique directory, preventing test interference when running in parallel.

## Test Project Structure

Each source project has its own test project that mirrors the structure:

```
src/Adapters/Conduit.Sources.Rss/          tests/Adapters/Conduit.Sources.Rss.Tests/
src/Adapters/Conduit.Sources.Edi834/       tests/Adapters/Conduit.Sources.Edi834.Tests/
src/Adapters/Conduit.Sources.Zotero/       tests/Adapters/Conduit.Sources.Zotero.Tests/
src/App/Conduit/                           tests/Conduit.Tests/
```

Each test project references its source project via `<ProjectReference>` and has its own NuGet packages (xUnit, Moq, etc.).

`dotnet test` from the solution root discovers and runs all test projects automatically.

## TDD in Practice

The cycle we followed throughout the build:

1. **Write the test** -- define what the code should do
2. **Watch it fail** -- confirm the test is actually testing something
3. **Write the implementation** -- make the test pass
4. **Refactor** -- clean up without changing behavior (tests catch regressions)

Example from the 834 adapter: we wrote 12 tests covering parsing, field extraction, error handling, and edge cases before writing the adapter. Each test was a specification of behavior.

## Coverage Target

For a pipeline project like Conduit:
- 90%+ on core logic (adapters, models, transformations)
- 100% on the happy path -- every feature has at least one test
- Edge cases covered -- empty inputs, malformed data, missing fields
- Don't test plumbing -- DI wiring, framework code, Program.cs

The real metric: can you refactor with confidence? If you can change the internals and trust the tests to catch regressions, your coverage is good enough.

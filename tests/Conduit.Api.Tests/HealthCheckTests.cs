using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;

namespace Conduit.Api.Tests;

public class HealthCheckTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public HealthCheckTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Get_Health_Returns200()
    {
        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Get_Health_ReturnsHealthyStatus()
    {
        var response = await _client.GetAsync("/health");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Contains("healthy", body);
    }
}

using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Projects;

namespace Comnix.Tests;

public sealed class ComnixApiTests : IAsyncLifetime
{
    private DistributedApplication _app = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Comnix_AppHost>();

        _app = await appHost.BuildAsync();
        await _app.StartAsync();

        using var readyCts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
        await _app.ResourceNotifications.WaitForResourceHealthyAsync("comnix", readyCts.Token);

        _client = _app.CreateHttpClient("comnix");
    }

    public async Task DisposeAsync()
    {
        _client?.Dispose();

        if (_app is not null)
            await _app.DisposeAsync();
    }

    [Fact]
    public async Task GetKnownRoute_ShouldExecuteSshCommand_AndReturnSuccess()
    {
        // Arrange
        const string route = "/api/test";

        // Act
        var response = await _client.GetAsync(route);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        content.Should().Contain("\"success\": true");
    }

    [Fact]
    public async Task GetUnknownRoute_ShouldReturnFailure_WithCommandNotFoundMessage()
    {
        // Arrange
        const string route = "/api/unknown-route";

        // Act
        var response = await _client.GetAsync(route);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        content.Should().Contain("\"success\": false");
        content.Should().Contain("Command not found for route: unknown-route");
    }
}

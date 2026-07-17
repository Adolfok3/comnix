namespace Comnix.Tests;

public sealed class ComnixCustomCommandsTests(CustomCommandsAppFixture fixture) : IClassFixture<CustomCommandsAppFixture>
{
    [Fact]
    public async Task GetEchoTestRoute_ShouldExecuteCommandFromCustomCommandsJson_AndReturnSuccess()
    {
        // Arrange
        const string route = "/api/echo-test";

        // Act
        var response = await fixture.HttpClient.GetAsync(route);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        content.Should().Contain("\"success\": true");
        content.Should().Contain("comnix-custom-config");
    }

    [Fact]
    public async Task GetWhoamiRoute_ShouldExecuteCommandFromCustomCommandsJson_AndReturnSshUser()
    {
        // Arrange
        const string route = "/api/whoami";

        // Act
        var response = await fixture.HttpClient.GetAsync(route);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        content.Should().Contain("\"success\": true");
        content.Should().Contain("test");
    }

    [Fact]
    public async Task GetDefaultAppHostRoute_ShouldReturnCommandNotFound_BecauseCommandsJsonIsCustom()
    {
        // Arrange
        const string route = "/api/test";

        // Act
        var response = await fixture.HttpClient.GetAsync(route);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        content.Should().Contain("\"success\": false");
        content.Should().Contain("Command not found for route: test");
    }
}

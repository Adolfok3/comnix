namespace Comnix.Tests;

public sealed class ComnixResponseHtmlTests(ResponseHtmlAppFixture fixture) : IClassFixture<ResponseHtmlAppFixture>
{
    private const string ResponseHtmlTemplate =
        """
        <html><body><p data-success="{{success}}">{{result}}</p></body></html>
        """;

    private readonly ResponseHtmlAppFixture _fixture = fixture;
    private readonly string _responseHtmlPath = Path.Combine(fixture.ConfigDirectory, "response.html");

    [Fact]
    public async Task GetRoute_WithoutResponseHtmlFile_ShouldReturnJsonFallback()
    {
        // Arrange
        File.Delete(_responseHtmlPath);

        // Act
        var response = await _fixture.HttpClient.GetAsync("/api/html-test");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
        content.Should().Contain("\"success\": true");
        content.Should().Contain("<hello & world>");
    }

    [Fact]
    public async Task GetRoute_WithResponseHtmlFile_ShouldReturnHtmlWithSubstitutedPlaceholders()
    {
        // Arrange
        await File.WriteAllTextAsync(_responseHtmlPath, ResponseHtmlTemplate);

        // Act
        var response = await _fixture.HttpClient.GetAsync("/api/html-test");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.Content.Headers.ContentType?.MediaType.Should().Be("text/html");
        content.Should().Contain("data-success=\"true\"");
        content.Should().Contain("&lt;hello &amp; world&gt;");
        content.Should().NotContain("{{success}}");
        content.Should().NotContain("{{result}}");
    }
}

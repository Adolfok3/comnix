using System.Text.Json;

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
        ComnixConfigAppFixture.MakeAccessibleToContainer(_responseHtmlPath);

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

    [Fact]
    public async Task GetRoute_WithQuotesAndSlashesInOutput_ShouldReturnValidJson_WhenNoResponseHtmlFile()
    {
        // Arrange
        File.Delete(_responseHtmlPath);

        // Act
        var response = await _fixture.HttpClient.GetAsync("/api/special-chars-test");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");

        var parseJson = () => JsonDocument.Parse(content);
        using var json = parseJson.Should().NotThrow("quotes and slashes in the command output must not break the JSON response").Subject;

        json.RootElement.GetProperty("success").GetBoolean().Should().BeTrue();
        json.RootElement.GetProperty("result").GetString().Should().Contain("""He said "hi" at /var/log/app.log""");
    }

    [Fact]
    public async Task GetRoute_WithQuotesAndSlashesInOutput_ShouldHtmlEncodeQuotes_WhenResponseHtmlFileExists()
    {
        // Arrange
        await File.WriteAllTextAsync(_responseHtmlPath, ResponseHtmlTemplate);
        ComnixConfigAppFixture.MakeAccessibleToContainer(_responseHtmlPath);

        // Act
        var response = await _fixture.HttpClient.GetAsync("/api/special-chars-test");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.Content.Headers.ContentType?.MediaType.Should().Be("text/html");
        content.Should().Contain("&quot;hi&quot;");
        content.Should().Contain("/var/log/app.log");
    }
}

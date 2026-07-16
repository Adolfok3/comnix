namespace Comnix.Tests;

public sealed class ResponseHtmlAppFixture : ComnixConfigAppFixture
{
    protected override string CommandsJson =>
        """
        {
          "commands": [
            { "route": "html-test", "command": "echo '<hello & world>'" }
          ]
        }
        """;
}

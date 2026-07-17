namespace Comnix.Tests;

public sealed class ResponseHtmlAppFixture : ComnixConfigAppFixture
{
    protected override string CommandsJson =>
        """
        {
          "commands": [
            { "route": "html-test", "command": "echo '<hello & world>'" },
            { "route": "special-chars-test", "command": "echo 'He said \"hi\" at /var/log/app.log'" }
          ]
        }
        """;
}

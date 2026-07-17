namespace Comnix.Tests;

public sealed class CustomCommandsAppFixture : ComnixConfigAppFixture
{
    protected override string CommandsJson =>
        """
        {
          "commands": [
            { "route": "echo-test", "command": "echo comnix-custom-config" },
            { "route": "whoami", "command": "whoami" }
          ]
        }
        """;
}

namespace Comnix.Options;

public sealed class CommandOptions
{
    public string Route { get; set; } = null!;

    public string Command { get; set; } = null!;

    public string? Connection { get; set; }
}

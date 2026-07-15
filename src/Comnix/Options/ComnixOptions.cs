namespace Comnix.Options;

public sealed class ComnixOptions
{
    public IReadOnlyList<CommandOptions> Commands { get; set; } = [];

    public IDictionary<string, SshOptions> Ssh { get; set; } = new Dictionary<string, SshOptions>(StringComparer.OrdinalIgnoreCase);
}

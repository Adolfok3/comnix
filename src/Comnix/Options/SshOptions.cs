namespace Comnix.Options;

public sealed class SshOptions
{
    public string Host { get; set; } = null!;

    public string User { get; set; } = null!;

    public int Port { get; set; } = 22;

    public string? KeyPath { get; set; }

    public string? Password { get; set; }

    public int CommandTimeout { get; set; } = 120;
}

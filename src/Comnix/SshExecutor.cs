using Comnix.Options;
using Microsoft.Extensions.Options;
using Renci.SshNet;

namespace Comnix;

internal sealed class SshExecutor(IOptionsMonitor<ComnixOptions> options)
{
    private const string DefaultConnection = "default";

    public async Task<(bool Success, string Result)> ExecuteAsync(string route, CancellationToken cancellationToken)
    {
        var command = GetCommand(route);
        if (command is null)
            return (false, $"Command not found for route: {route}");

        return !options.CurrentValue.Ssh.TryGetValue(command.Connection ?? DefaultConnection, out var ssh)
            ? ((bool Success, string Result))(false, $"SSH configuration is missing for connection: {command.Connection ?? DefaultConnection}")
            : await ExecuteCommandAsync(command, ssh, cancellationToken);
    }

    private static async Task<(bool Success, string Result)> ExecuteCommandAsync(CommandOptions command, SshOptions ssh, CancellationToken cancellationToken)
    {
        using var client = CreateClient(ssh);
        await client.ConnectAsync(cancellationToken);

        var sshCommand = client.CreateCommand(command.Command);
        sshCommand.CommandTimeout = TimeSpan.FromSeconds(ssh.CommandTimeout);
        await sshCommand.ExecuteAsync(cancellationToken);

        return (string.IsNullOrEmpty(sshCommand.Error), sshCommand.Result);
    }

    private static SshClient CreateClient(SshOptions ssh)
        => !string.IsNullOrWhiteSpace(ssh.KeyPath)
            ? CreateSshClientWithPrivateKeyFile(ssh)
            : CreateSshClientWithPassword(ssh);

    private static SshClient CreateSshClientWithPassword(SshOptions ssh)
        => new(ssh.Host, ssh.Port, ssh.User, ssh.Password!);

    private static SshClient CreateSshClientWithPrivateKeyFile(SshOptions ssh)
        => new(ssh.Host, ssh.Port, ssh.User, new PrivateKeyFile(ssh.KeyPath!));

    private CommandOptions? GetCommand(string route)
        => options.CurrentValue.Commands.FirstOrDefault(x => x.Route.Equals(route, StringComparison.OrdinalIgnoreCase));
}
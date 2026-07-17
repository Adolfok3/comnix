using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Projects;

namespace Comnix.Tests;

public abstract class ComnixConfigAppFixture : IAsyncLifetime
{
    private const string SshConfigJson =
        """
        {
          "ssh": {
            "default": {
              "host": "ssh-server",
              "user": "test",
              "port": 2222,
              "password": "pass123"
            }
          }
        }
        """;

    private DistributedApplication _app = null!;

    public string ConfigDirectory { get; } = Directory.CreateTempSubdirectory("comnix-tests-").FullName;

    public HttpClient HttpClient { get; private set; } = null!;

    protected abstract string CommandsJson { get; }

    // Comnix runs as a non-root UID that won't match this host path's owner, and CreateTempSubdirectory
    // defaults to 0700 on Unix, so the bind-mounted path must be relaxed or the container can't read it.
    public static void MakeAccessibleToContainer(string path)
    {
        if (OperatingSystem.IsWindows())
            return;

        File.SetUnixFileMode(
            path,
            UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute |
            UnixFileMode.GroupRead | UnixFileMode.GroupWrite | UnixFileMode.GroupExecute |
            UnixFileMode.OtherRead | UnixFileMode.OtherWrite | UnixFileMode.OtherExecute);
    }

    public async Task InitializeAsync()
    {
        MakeAccessibleToContainer(ConfigDirectory);

        var commandsPath = Path.Combine(ConfigDirectory, "commands.json");
        var sshPath = Path.Combine(ConfigDirectory, "ssh.json");

        await File.WriteAllTextAsync(commandsPath, CommandsJson);
        await File.WriteAllTextAsync(sshPath, SshConfigJson);
        MakeAccessibleToContainer(commandsPath);
        MakeAccessibleToContainer(sshPath);

        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Comnix_AppHost>(
            [$"ConfigVolumePath={ConfigDirectory}"]);

        _app = await appHost.BuildAsync();
        await _app.StartAsync();

        using var readyCts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
        await _app.ResourceNotifications.WaitForResourceHealthyAsync("comnix", readyCts.Token);

        HttpClient = _app.CreateHttpClient("comnix");
    }

    public async Task DisposeAsync()
    {
        HttpClient?.Dispose();

        if (_app is not null)
            await _app.DisposeAsync();

        Directory.Delete(ConfigDirectory, recursive: true);
    }
}

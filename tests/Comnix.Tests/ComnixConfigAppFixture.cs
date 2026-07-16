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

    public async Task InitializeAsync()
    {
        await File.WriteAllTextAsync(Path.Combine(ConfigDirectory, "commands.json"), CommandsJson);
        await File.WriteAllTextAsync(Path.Combine(ConfigDirectory, "ssh.json"), SshConfigJson);

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

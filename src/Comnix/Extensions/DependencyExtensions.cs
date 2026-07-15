using Comnix.Options;

namespace Comnix.Extensions;

public static class DependencyExtensions
{
    public static void AddAppDependencies(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<ComnixOptions>(builder.Configuration);
        builder.Services.AddSingleton<SshExecutor>();
    }
}

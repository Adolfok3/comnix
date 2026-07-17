using Comnix.Utils;

namespace Comnix.Extensions;

public static class ConfigurationExtensions
{
    private const string CommandsFilePath = $"{AppConsts.ConfigDir}/{AppConsts.CommandsFile}";
    private const string SshFilePath = $"{AppConsts.ConfigDir}/{AppConsts.SshFile}";

    public static IConfigurationBuilder AddAppConfiguration(this IConfigurationBuilder builder)
    {
        RequireConfigAppFiles();

        builder.AddJsonFile(SshFilePath, optional: false, reloadOnChange: true);
        builder.AddJsonFile(CommandsFilePath, optional: false, reloadOnChange: true);

        return builder;
    }

    private static void RequireConfigAppFiles()
    {
        if (!Directory.Exists(AppConsts.ConfigDir))
            Directory.CreateDirectory(AppConsts.ConfigDir);

        if (!File.Exists(CommandsFilePath))
        {
            File.WriteAllText(
                CommandsFilePath,
                """
                {
                  "commands": []
                }
                """);
        }

        if (!File.Exists(SshFilePath))
        {
            File.WriteAllText(
                SshFilePath,
                """
                {
                  "ssh": {}
                }
                """);
        }
    }
}

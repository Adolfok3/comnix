using Comnix.Utils;

namespace Comnix.Endpoints;

public static class AppEndpoints
{
    public static void MapAppEndpoints(this WebApplication app)
        => app.MapGet("/api/{**route}", async (string route, SshExecutor executor, CancellationToken token) =>
        {
            string result = string.Empty;
            bool success = false;

            try
            {
                (success, result) = await executor.ExecuteAsync(route, token);
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }

            return Content(success, result);
        });

    private static IResult Content(bool success, string result)
    {
        var templatePath = Path.Combine(AppConsts.ConfigDir, "response.html");
        if (!File.Exists(templatePath))
            return Results.Content($"{{\"success\": {success.ToString().ToLower()}, \"result\": \"{result}\"}}", "application/json");

        var html = File.ReadAllText(templatePath);
        html = html
            .Replace("{{success}}", success.ToString().ToLower(), StringComparison.OrdinalIgnoreCase)
            .Replace("{{result}}", System.Net.WebUtility.HtmlEncode(result), StringComparison.OrdinalIgnoreCase);

        return Results.Content(html, "text/html");
    }
}
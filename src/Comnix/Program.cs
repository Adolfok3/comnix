using Comnix.Endpoints;
using Comnix.Extensions;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Configuration.AddAppConfiguration();
builder.AddAppDependencies();

var app = builder.Build();

app.MapAppEndpoints();

await app.RunAsync();
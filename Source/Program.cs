// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;
using Cratis.Chronicle.Mcp;
using Cratis.Chronicle.Mcp.Api;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

var builder = Host.CreateApplicationBuilder(args);

string[] configSectionPath = ["Cratis", "Chronicle", "Mcp"];

var configSection = ConfigurationPath.Combine(configSectionPath);

builder.Services
    .AddOptions<McpServerOptions>()
    .BindConfiguration(configSection);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Logging.AddConsole(logging => logging.LogToStandardErrorThreshold = LogLevel.Trace);

builder.Services.AddSingleton(sp =>
{
    var options = sp.GetRequiredService<IOptions<McpServerOptions>>().Value;
    return new ChronicleClient(options.ConnectionString);
});
builder.Services.AddSingleton<IChronicleClient>(sp => sp.GetRequiredService<ChronicleClient>());

builder.Services.AddHttpClient<ChronicleApiClient>((sp, client) =>
{
    var options = sp.GetRequiredService<IOptions<McpServerOptions>>().Value;
    client.BaseAddress = new Uri(options.ManagementUrl);
});

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

var host = builder.Build();
await host.RunAsync();

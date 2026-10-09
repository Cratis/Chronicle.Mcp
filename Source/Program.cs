// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Mcp.Authorization;
using Cratis.Chronicle.Mcp.Configuration;
using Cratis.Chronicle.Mcp.Connection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.AddDeploymentPolicy(builder.Configuration.GetSection(McpOptionsBinding.SectionPath));

// The MCP server communicates over stdio, so all logging must go to stderr to avoid corrupting the protocol stream.
builder.Logging.AddConsole(logging => logging.LogToStandardErrorThreshold = LogLevel.Trace);

builder.Services.AddChronicleMcpConnection();

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithDeploymentTools()
    .WithPromptsFromAssembly();

var host = builder.Build();
await host.RunAsync();

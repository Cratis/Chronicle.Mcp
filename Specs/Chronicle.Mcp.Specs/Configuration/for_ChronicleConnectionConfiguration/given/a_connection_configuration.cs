// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using Cratis.Chronicle.Connections;
using Microsoft.Extensions.Options;

namespace Cratis.Chronicle.Mcp.Configuration.for_ChronicleConnectionConfiguration.given;

public class a_connection_configuration : Configuration.given.a_process_configuration
{
    protected McpServerOptions _options = new() { ConnectionString = "chronicle://server.example:35000" };
    protected ChronicleCliContext _context = new()
    {
        Server = "chronicle://server.example:35000",
        AccessToken = "synthetic-login-token",
        TokenExpiry = DateTimeOffset.UtcNow.AddHours(1).ToString("O"),
        TokenServer = "server.example:35000"
    };
    protected ChronicleConnectionString _result;
    protected Exception? _exception;

    protected void EnableCli()
    {
        _options.UseCliConfiguration = true;
        _options.CliConfigurationPath = Path.Combine(_directory, "cli.json");
        File.WriteAllText(_options.CliConfigurationPath, JsonSerializer.Serialize(new
        {
            contexts = new { @default = _context }
        }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
    }

    protected void Resolve() => _result = new ChronicleConnectionString(new ChronicleConnectionConfiguration(Options.Create(_options)).ResolveConnectionString());
}

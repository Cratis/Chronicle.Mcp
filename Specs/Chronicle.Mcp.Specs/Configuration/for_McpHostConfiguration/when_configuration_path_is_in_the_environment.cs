// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Configuration.for_McpHostConfiguration;

public class when_configuration_path_is_in_the_environment : given.a_process_configuration
{
    string? _result;

    void Establish()
    {
        var path = Path.Combine(_directory, "settings.json");
        File.WriteAllText(path, "{\"explicitSetting\":\"from-environment-path\"}");
        SetEnvironment("CHRONICLE_MCP_CONFIG", path);
    }

    void Because()
    {
        var builder = McpHostConfiguration.CreateBuilder([]);
        _result = builder.Configuration["explicitSetting"];
        using var host = builder.Build();
    }

    [Fact] void should_read_the_explicit_environment_path() => _result.ShouldEqual("from-environment-path");
}

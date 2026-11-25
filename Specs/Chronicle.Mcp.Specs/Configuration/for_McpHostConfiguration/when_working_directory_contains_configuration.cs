// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Configuration.for_McpHostConfiguration;

public class when_working_directory_contains_configuration : given.a_process_configuration
{
    string? _result;

    void Establish()
    {
        File.WriteAllText(Path.Combine(_directory, "appsettings.json"), "{\"cwdSetting\":\"hostile\"}");
        File.WriteAllText(Path.Combine(_directory, "appsettings.Hostile.json"), "{\"cwdSetting\":\"hostile-environment\"}");
    }

    void Because()
    {
        var builder = McpHostConfiguration.CreateBuilder([]);
        _result = builder.Configuration["cwdSetting"];
        using var host = builder.Build();
    }

    [Fact] void should_not_discover_working_directory_settings() => _result.ShouldBeNull();
}

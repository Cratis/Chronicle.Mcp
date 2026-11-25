// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Configuration.for_McpHostConfiguration;

public class when_configuration_path_is_explicit : given.a_process_configuration
{
    string _path;
    string? _fileSetting;
    string? _environmentSetting;

    void Establish()
    {
        _path = Path.Combine(_directory, "settings.json");
        File.WriteAllText(_path, "{\"explicitSetting\":\"from-file\",\"MCP_SPEC_OVERRIDE\":\"from-file\"}");
        SetEnvironment("MCP_SPEC_OVERRIDE", "from-environment");
        SetEnvironment("CHRONICLE_MCP_CONFIG", Path.Combine(_directory, "not-selected.json"));
    }

    void Because()
    {
        var builder = McpHostConfiguration.CreateBuilder(["--config", _path]);
        _fileSetting = builder.Configuration["explicitSetting"];
        _environmentSetting = builder.Configuration["MCP_SPEC_OVERRIDE"];
        using var host = builder.Build();
    }

    [Fact] void should_read_only_the_selected_file() => _fileSetting.ShouldEqual("from-file");
    [Fact] void should_give_environment_variables_precedence() => _environmentSetting.ShouldEqual("from-environment");
}

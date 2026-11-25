// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Configuration.given;

[Collection("MCP process configuration")]
public class a_process_configuration : Specs.given.a_configuration_directory
{
    string _previousDirectory;
    readonly Dictionary<string, string?> _environment = new();

    void Establish()
    {
        _previousDirectory = Directory.GetCurrentDirectory();
        Directory.SetCurrentDirectory(_directory);
        SetEnvironment("CHRONICLE_CONNECTION_STRING", null);
        SetEnvironment("CHRONICLE_MCP_CONFIG", null);
        SetEnvironment("DOTNET_ENVIRONMENT", "Hostile");
    }

    protected void SetEnvironment(string name, string? value)
    {
        _environment.TryAdd(name, Environment.GetEnvironmentVariable(name));
        Environment.SetEnvironmentVariable(name, value);
    }

    void Destroy()
    {
        Directory.SetCurrentDirectory(_previousDirectory);
        foreach (var variable in _environment)
        {
            Environment.SetEnvironmentVariable(variable.Key, variable.Value);
        }
    }
}

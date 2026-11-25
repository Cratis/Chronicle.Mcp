// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Cratis.Chronicle.Mcp.Configuration.for_McpHostConfiguration;

public class when_logging_is_configured_in_the_environment : given.a_process_configuration
{
    bool _informationEnabled;
    bool _errorEnabled;

    void Establish() => SetEnvironment("Logging__LogLevel__Default", "Error");

    void Because()
    {
        var builder = McpHostConfiguration.CreateBuilder([]);
        builder.Logging.AddConsole().SetMinimumLevel(LogLevel.Information);
        using var host = builder.Build();
        var logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger("MCP configuration spec");
        _informationEnabled = logger.IsEnabled(LogLevel.Information);
        _errorEnabled = logger.IsEnabled(LogLevel.Error);
    }

    [Fact] void should_apply_the_configured_filter() => _informationEnabled.ShouldBeFalse();
    [Fact] void should_allow_the_configured_level() => _errorEnabled.ShouldBeTrue();
}

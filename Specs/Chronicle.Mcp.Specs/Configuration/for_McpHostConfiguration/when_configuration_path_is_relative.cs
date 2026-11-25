// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Configuration.for_McpHostConfiguration;

public class when_configuration_path_is_relative : given.a_process_configuration
{
    Exception? _exception;

    void Because() => _exception = Catch.Exception(() => McpHostConfiguration.CreateBuilder(["--config", "settings.json"]));

    [Fact] void should_require_an_absolute_path() => _exception.ShouldBeOfExactType<ArgumentException>();
}

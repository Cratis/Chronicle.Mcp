// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Configuration.for_McpHostConfiguration;

public class when_selected_configuration_is_missing : given.a_process_configuration
{
    Exception? _exception;

    void Because() => _exception = Catch.Exception(() => McpHostConfiguration.CreateBuilder(["--config", Path.Combine(_directory, "missing.json")]));

    [Fact] void should_not_fall_back_to_other_configuration() => _exception.ShouldBeOfExactType<FileNotFoundException>();
}

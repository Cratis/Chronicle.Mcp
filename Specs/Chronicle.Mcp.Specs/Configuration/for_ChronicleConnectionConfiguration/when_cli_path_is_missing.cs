// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Configuration.for_ChronicleConnectionConfiguration;

public class when_cli_path_is_missing : given.a_connection_configuration
{
    void Establish() => _options.UseCliConfiguration = true;
    void Because() => _exception = Catch.Exception(Resolve);

    [Fact] void should_require_the_cli_path() => _exception!.Message.ShouldContain("CliConfigurationPath");
}

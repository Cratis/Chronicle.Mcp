// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Configuration.for_McpHostConfiguration;

public class when_command_line_overrides_are_supplied : given.a_process_configuration
{
    Exception? _exception;

    void Because() => _exception = Catch.Exception(() => McpHostConfiguration.CreateBuilder(["--Cratis:Chronicle:Mcp:ConnectionString=chronicle://other.example:35000"]));

    [Fact] void should_reject_unsupported_overrides() => _exception.ShouldBeOfExactType<ArgumentException>();
    [Fact] void should_explain_the_supported_argument() => _exception!.Message.ShouldContain("--config");
}

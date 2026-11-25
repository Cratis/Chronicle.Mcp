// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Configuration.for_ChronicleConnectionConfiguration;

public class when_credentials_are_missing : given.a_connection_configuration
{
    void Because() => _exception = Catch.Exception(Resolve);

    [Fact] void should_refuse_implicit_development_credentials() => _exception.ShouldBeOfExactType<InvalidOperationException>();
    [Fact] void should_explain_the_opt_in() => _exception!.Message.ShouldContain("UseDevelopmentCredentials=true");
}

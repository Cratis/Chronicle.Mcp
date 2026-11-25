// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Configuration.for_ChronicleConnectionConfiguration;

public class when_login_has_no_destination : given.a_connection_configuration
{
    void Establish()
    {
        _context.TokenServer = null;
        EnableCli();
    }

    void Because() => _exception = Catch.Exception(Resolve);

    [Fact] void should_refuse_the_unbound_token() => _exception.ShouldBeOfExactType<InvalidOperationException>();
}

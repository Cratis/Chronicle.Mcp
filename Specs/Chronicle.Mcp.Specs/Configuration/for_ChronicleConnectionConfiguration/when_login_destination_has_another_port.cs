// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Configuration.for_ChronicleConnectionConfiguration;

public class when_login_destination_has_another_port : given.a_connection_configuration
{
    void Establish()
    {
        _context.TokenServer = "server.example:35001";
        EnableCli();
    }

    void Because() => _exception = Catch.Exception(Resolve);

    [Fact] void should_not_send_the_login_token() => _exception.ShouldBeOfExactType<InvalidOperationException>();
}

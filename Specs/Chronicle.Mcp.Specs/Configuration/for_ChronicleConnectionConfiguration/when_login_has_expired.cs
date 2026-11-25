// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Configuration.for_ChronicleConnectionConfiguration;

public class when_login_has_expired : given.a_connection_configuration
{
    void Establish()
    {
        _context.TokenExpiry = DateTimeOffset.UtcNow.AddMinutes(-1).ToString("O");
        EnableCli();
    }

    void Because() => _exception = Catch.Exception(Resolve);

    [Fact] void should_refuse_the_expired_token() => _exception.ShouldBeOfExactType<InvalidOperationException>();
}

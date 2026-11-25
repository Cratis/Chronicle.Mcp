// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Configuration.for_ChronicleConnectionConfiguration;

public class when_connection_is_missing : given.a_connection_configuration
{
    void Establish() => _options.ConnectionString = null;
    void Because() => _exception = Catch.Exception(Resolve);

    [Fact] void should_require_an_explicit_connection() => _exception!.Message.ShouldContain("ConnectionString explicitly");
}

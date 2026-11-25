// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Connections;

namespace Cratis.Chronicle.Mcp.Configuration.for_ChronicleDestination;

public class when_srv_discovery_is_configured : Specification
{
    Exception? _exception;

    void Because() => _exception = Catch.Exception(() => ChronicleDestination.For(new ChronicleConnectionString("chronicle+srv://server.example")));

    [Fact] void should_require_a_fixed_authentication_destination() => _exception.ShouldBeOfExactType<InvalidOperationException>();
}

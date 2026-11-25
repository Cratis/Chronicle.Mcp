// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Connections;

namespace Cratis.Chronicle.Mcp.Configuration.for_ChronicleDestination;

public class when_a_load_balancer_override_is_configured : Specification
{
    Exception? _exception;

    void Because() => _exception = Catch.Exception(() => ChronicleDestination.For(new ChronicleConnectionString("chronicle://server.example:35000?loadBalancer=other.example:35000")));

    [Fact] void should_not_send_a_token_to_a_different_transport_endpoint() => _exception.ShouldBeOfExactType<InvalidOperationException>();
}

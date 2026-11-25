// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Configuration.for_ChronicleConnectionConfiguration;

public class when_multiple_destinations_are_configured : given.a_connection_configuration
{
    void Establish()
    {
        _options.ConnectionString = "chronicle://server.example:35000,other.example:35000?skipTlsValidation=false";
        EnableCli();
    }

    void Because() => _exception = Catch.Exception(Resolve);

    [Fact] void should_refuse_to_send_a_token_to_failover_destinations() => _exception!.Message.ShouldContain("single Chronicle destination");
}

// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Configuration.for_ChronicleConnectionConfiguration;

public class when_multiple_hosts_have_explicit_client_credentials : given.a_connection_configuration
{
    void Establish()
    {
        _options.ConnectionString = "chronicle://first.example:35000,second.example:35000";
        _options.ClientId = ReservedClientId;
        _options.ClientSecret = ReservedClientSecret;
    }

    void Because() => Resolve();

    [Fact] void should_preserve_the_client() => _result.Username.ShouldEqual(ReservedClientId);
    [Fact] void should_preserve_the_secret() => _result.Password.ShouldEqual(ReservedClientSecret);
    [Fact] void should_not_use_a_fixed_destination_cache() => ChronicleDestination.IsFixed(_result).ShouldBeFalse();
}

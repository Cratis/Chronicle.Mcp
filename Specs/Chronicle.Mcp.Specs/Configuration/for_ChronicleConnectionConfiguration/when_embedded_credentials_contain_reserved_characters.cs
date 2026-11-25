// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Configuration.for_ChronicleConnectionConfiguration;

public class when_embedded_credentials_contain_reserved_characters : given.a_connection_configuration
{
    void Establish() => _options.ConnectionString = $"chronicle+srv://{Uri.EscapeDataString(ReservedClientId)}:{Uri.EscapeDataString(ReservedClientSecret)}@server.example";
    void Because() => Resolve();

    [Fact] void should_preserve_the_client_id() => _result.Username.ShouldEqual(ReservedClientId);
    [Fact] void should_preserve_the_complete_secret() => _result.Password.ShouldEqual(ReservedClientSecret);
    [Fact] void should_preserve_the_explicit_topology() => _result.IsSrv.ShouldBeTrue();
}

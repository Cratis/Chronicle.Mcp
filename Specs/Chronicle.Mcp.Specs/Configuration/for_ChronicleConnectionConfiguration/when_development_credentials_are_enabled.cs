// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Connections;

namespace Cratis.Chronicle.Mcp.Configuration.for_ChronicleConnectionConfiguration;

public class when_development_credentials_are_enabled : given.a_connection_configuration
{
    void Establish()
    {
        _options.ConnectionString = null;
        _options.UseDevelopmentCredentials = true;
    }

    void Because() => Resolve();

    [Fact] void should_use_the_local_server() => _result.ServerAddress.Host.ShouldEqual("localhost");
    [Fact] void should_use_development_credentials() => _result.Username.ShouldEqual(ChronicleConnectionString.DevelopmentClient);
}

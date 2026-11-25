// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;

namespace Cratis.Chronicle.Mcp.Configuration.for_ChronicleConnectionConfiguration;

public class when_legacy_login_matches_the_destination : given.a_connection_configuration
{
    void Establish()
    {
        EnableCli();
        File.WriteAllText(_options.CliConfigurationPath!, JsonSerializer.Serialize(new
        {
            defaultServer = _context.Server,
            accessToken = _context.AccessToken,
            tokenExpiry = _context.TokenExpiry,
            tokenServer = _context.TokenServer
        }));
    }

    void Because() => Resolve();

    [Fact] void should_preserve_the_legacy_destination_binding() => _result.ApiKey.ShouldEqual("synthetic-login-token");
}

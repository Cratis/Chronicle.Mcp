// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Configuration.for_ChronicleConnectionConfiguration;

public class when_login_matches_the_destination : given.a_connection_configuration
{
    void Establish()
    {
        _options.ConnectionString = "chronicle://SERVER.EXAMPLE:35000";
        EnableCli();
    }

    void Because() => Resolve();

    [Fact] void should_use_the_bound_login() => _result.ApiKey.ShouldEqual("synthetic-login-token");
}

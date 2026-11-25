// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Configuration.for_ChronicleConnectionConfiguration;

public class when_explicit_credentials_override_login : given.a_connection_configuration
{
    void Establish()
    {
        _options.ClientId = "explicit-client";
        _options.ClientSecret = "synthetic-secret";
        EnableCli();
    }

    void Because() => Resolve();

    [Fact] void should_use_the_explicit_client() => _result.Username.ShouldEqual("explicit-client");
    [Fact] void should_not_use_the_login_token() => _result.ApiKey.ShouldBeNull();
}

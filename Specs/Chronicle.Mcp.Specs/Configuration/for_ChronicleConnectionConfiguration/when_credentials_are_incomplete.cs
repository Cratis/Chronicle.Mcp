// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Configuration.for_ChronicleConnectionConfiguration;

public class when_credentials_are_incomplete : given.a_connection_configuration
{
    void Establish()
    {
        _options.ClientId = "explicit-client";
        _options.UseDevelopmentCredentials = true;
    }

    void Because() => _exception = Catch.Exception(Resolve);

    [Fact] void should_not_replace_partial_credentials_with_development_credentials() => _exception!.Message.ShouldContain("both ClientId and ClientSecret");
}

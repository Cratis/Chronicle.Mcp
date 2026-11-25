// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Configuration.for_ChronicleConnectionConfiguration;

public class when_context_credentials_match_the_destination : given.a_context_with_credentials
{
    void Establish()
    {
        _options.ConnectionString = "chronicle://SERVER.EXAMPLE:35000?skipTlsValidation=false";
        EnableCli();
    }

    void Because() => Resolve();

    [Fact] void should_use_the_context_client() => _result.Username.ShouldEqual(ReservedClientId);
    [Fact] void should_preserve_reserved_characters_in_the_secret() => _result.Password.ShouldEqual(ReservedClientSecret);
}

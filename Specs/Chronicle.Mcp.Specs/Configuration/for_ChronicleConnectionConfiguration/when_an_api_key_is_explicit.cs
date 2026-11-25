// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Configuration.for_ChronicleConnectionConfiguration;

public class when_an_api_key_is_explicit : given.a_connection_configuration
{
    void Establish() => _options.ApiKey = "explicit-key";
    void Because() => Resolve();

    [Fact] void should_use_the_explicit_key() => _result.ApiKey.ShouldEqual("explicit-key");
}

// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Configuration.for_ChronicleConnectionConfiguration;

public class when_multiple_hosts_have_an_explicit_api_key : given.a_connection_configuration
{
    void Establish()
    {
        _options.ConnectionString = "chronicle://first.example:35000,second.example:35000";
        _options.ApiKey = "explicit-key";
    }

    void Because() => Resolve();

    [Fact] void should_preserve_the_explicit_key() => _result.ApiKey.ShouldEqual("explicit-key");
    [Fact] void should_preserve_both_hosts() => _result.ServerAddresses.Count.ShouldEqual(2);
}

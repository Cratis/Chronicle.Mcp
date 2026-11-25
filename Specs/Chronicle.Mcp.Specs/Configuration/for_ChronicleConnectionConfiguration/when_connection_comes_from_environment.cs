// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Configuration.for_ChronicleConnectionConfiguration;

public class when_connection_comes_from_environment : given.a_connection_configuration
{
    void Establish()
    {
        _options.ConnectionString = null;
        _options.ApiKey = "explicit-key";
        SetEnvironment("CHRONICLE_CONNECTION_STRING", "chronicle://environment.example:35000");
    }

    void Because() => Resolve();

    [Fact] void should_use_the_explicit_environment_destination() => _result.ServerAddress.Host.ShouldEqual("environment.example");
}

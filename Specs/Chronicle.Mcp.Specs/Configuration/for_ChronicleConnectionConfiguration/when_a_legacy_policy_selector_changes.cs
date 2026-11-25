// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Configuration.for_ChronicleConnectionConfiguration;

public class when_a_legacy_policy_selector_changes : given.a_connection_configuration
{
    string _before;
    string _after;

    void Establish()
    {
        _options.ConnectionString = "chronicle://localhost:35000";
        _options.ClientId = "spec-client";
        _options.ClientSecret = "spec-secret";
        _options.Authorization.Principal = "first-local-selector";
    }

    void Because()
    {
        Resolve();
        _before = _result.ToString();
        _options.Authorization.Principal = "second-local-selector";
        Resolve();
        _after = _result.ToString();
    }

    [Fact] void should_not_change_the_connection_identity() => _after.ShouldEqual(_before);
}

// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Authorization.for_MutationAuthorizer.when_authorizing_with_an_allowlist;

public class and_legacy_allows_but_the_allowlist_does_not : given.an_allowlist
{
    void Establish()
    {
        _settings.Authorization.Principal = Principal;
        _settings.Authorization.Grants = [_grant];
        _entry.EventStore = "other";
    }

    void Because() => _decision = _authorizer.Authorize(_catalog.Find("stop_job"), _arguments, _settings);
    [Fact] void should_intersect_both_policies() => _decision.Code.ShouldEqual(DenialCodes.NotAuthorized);
}

// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Authorization.for_MutationAuthorizer.when_authorizing_with_an_allowlist;

public class and_legacy_target_does_not_match : given.an_allowlist
{
    void Establish()
    {
        _settings.Authorization.Principal = Principal;
        _grant.Targets = ["other"];
        _settings.Authorization.Grants = [_grant];
    }

    void Because() => _decision = _authorizer.Authorize(_catalog.Find("stop_job"), _arguments, _settings);
    [Fact] void should_not_bypass_legacy_restrictions() => _decision.Code.ShouldEqual(DenialCodes.NotAuthorized);
}

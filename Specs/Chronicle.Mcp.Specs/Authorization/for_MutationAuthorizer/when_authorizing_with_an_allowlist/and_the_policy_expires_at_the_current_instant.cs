// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Authorization.for_MutationAuthorizer.when_authorizing_with_an_allowlist;

public class and_the_policy_expires_at_the_current_instant : given.an_allowlist
{
    void Establish() => _settings.Authorization.ExpiresAt = Now;
    void Because() => _decision = _authorizer.Authorize(_catalog.Find("stop_job"), _arguments, _settings);
    [Fact] void should_deny_at_the_boundary() => _decision.Code.ShouldEqual(DenialCodes.PolicyExpired);
}

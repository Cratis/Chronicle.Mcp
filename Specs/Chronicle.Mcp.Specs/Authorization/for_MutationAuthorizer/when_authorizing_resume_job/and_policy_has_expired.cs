// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Authorization.for_MutationAuthorizer.when_authorizing_resume_job;

public class and_policy_has_expired : given.an_authorizer
{
    void Establish() { _settings.Authorization.ExpiresAt = Now.AddSeconds(-1); }
    void Because() => _decision = _authorizer.Authorize(_catalog.Find("resume_job"), _arguments, _settings);
    [Fact] void should_deny_resume_job() => _decision.Code.ShouldEqual(DenialCodes.PolicyExpired);
}

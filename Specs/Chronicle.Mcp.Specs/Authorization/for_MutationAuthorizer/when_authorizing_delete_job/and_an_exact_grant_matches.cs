// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Authorization.for_MutationAuthorizer.when_authorizing_delete_job;

public class and_an_exact_grant_matches : given.an_authorizer
{
    void Because() => _decision = _authorizer.Authorize(_catalog.Find("delete_job"), _arguments, _settings);
    [Fact] void should_allow_delete_job() => _decision.IsAllowed.ShouldBeTrue();
}

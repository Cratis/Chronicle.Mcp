// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Authorization.for_MutationAuthorizer.when_authorizing_with_an_allowlist;

public class and_an_operation_is_unknown : given.an_allowlist
{
    void Establish() => _entry.Operations.Add("invented_operation");
    void Because() => _decision = _authorizer.Authorize(_catalog.Find("stop_job"), _arguments, _settings);
    [Fact] void should_invalidate_the_entire_policy() => _decision.Code.ShouldEqual(DenialCodes.PolicyInvalid);
}

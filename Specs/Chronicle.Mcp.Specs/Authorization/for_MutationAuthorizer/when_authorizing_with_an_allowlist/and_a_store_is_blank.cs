// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Authorization.for_MutationAuthorizer.when_authorizing_with_an_allowlist;

public class and_a_store_is_blank : given.an_allowlist
{
    void Establish() => _entry.EventStore = " ";
    void Because() => _decision = _authorizer.Authorize(_catalog.Find("stop_job"), _arguments, _settings);
    [Fact] void should_fail_closed() => _decision.Code.ShouldEqual(DenialCodes.PolicyInvalid);
}

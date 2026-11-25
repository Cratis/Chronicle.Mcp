// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Authorization.for_MutationAuthorizer.when_authorizing_with_an_allowlist;

public class and_dimensions_match_different_entries : given.an_allowlist
{
    void Establish()
    {
        _entry.Namespace = "other";
        _settings.Authorization.Allowlist.Add(new() { EventStore = "other", Namespace = Namespace, Operations = ["stop_job"] });
    }

    void Because() => _decision = _authorizer.Authorize(_catalog.Find("stop_job"), _arguments, _settings);
    [Fact] void should_require_one_complete_match() => _decision.Code.ShouldEqual(DenialCodes.NotAuthorized);
}

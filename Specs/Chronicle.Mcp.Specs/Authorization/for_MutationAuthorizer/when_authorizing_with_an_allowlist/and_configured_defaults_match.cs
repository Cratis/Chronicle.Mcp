// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;

namespace Cratis.Chronicle.Mcp.Authorization.for_MutationAuthorizer.when_authorizing_with_an_allowlist;

public class and_configured_defaults_match : given.an_allowlist
{
    void Establish()
    {
        _arguments["eventStore"] = JsonSerializer.SerializeToElement(" ");
        _arguments.Remove("namespace");
    }

    void Because() => _decision = _authorizer.Authorize(_catalog.Find("stop_job"), _arguments, _settings);
    [Fact] void should_use_the_tool_scope_defaults() => _decision.IsAllowed.ShouldBeTrue();
}

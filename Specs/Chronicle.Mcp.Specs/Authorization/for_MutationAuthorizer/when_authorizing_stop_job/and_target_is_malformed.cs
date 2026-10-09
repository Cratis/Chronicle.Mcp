// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Authorization.for_MutationAuthorizer.when_authorizing_stop_job;

public class and_target_is_malformed : given.an_authorizer
{
    void Establish() { _arguments["jobId"] = System.Text.Json.JsonSerializer.SerializeToElement("not-a-guid"); }
    void Because() => _decision = _authorizer.Authorize(_catalog.Find("stop_job"), _arguments, _settings);
    [Fact] void should_deny_stop_job() => _decision.Code.ShouldEqual(DenialCodes.BadRequest);
}

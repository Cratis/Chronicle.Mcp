// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;

namespace Cratis.Chronicle.Mcp.Authorization.for_MutationAuthorizer.when_authorizing_with_an_allowlist;

public class and_the_job_id_is_malformed : given.an_allowlist
{
    void Establish() => _arguments["jobId"] = JsonSerializer.SerializeToElement("not-a-guid");
    void Because() => _decision = _authorizer.Authorize(_catalog.Find("stop_job"), _arguments, _settings);
    [Fact] void should_still_require_a_valid_job_id() => _decision.Code.ShouldEqual(DenialCodes.BadRequest);
}

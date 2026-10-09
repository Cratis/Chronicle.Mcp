// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Contracts.Jobs;
using Cratis.Chronicle.Contracts.Queries;
using NSubstitute;

namespace Cratis.Chronicle.Mcp.Authorization.for_AuthorizedToolCalls.when_calling_a_tool;

public class and_read_policy_is_invalid : given.a_filter
{
    void Establish()
    {
        _settings.Authorization = new();
        _services.Jobs.AllJobs(Arg.Any<AllJobsRequest>()).Returns(QueryResult<IEnumerable<JobSummaryResponse>>.Success(Guid.Empty, []));
    }
    async Task Because() => _result = await Call("list_jobs");
    [Fact] void should_allow_the_read() => (_result.IsError ?? false).ShouldBeFalse();
    [Fact] void should_reach_chronicle() => _services.Jobs.Received(1).AllJobs(Arg.Any<AllJobsRequest>());
    [Fact] void should_not_read_the_mutation_policy() => _ = _monitor.DidNotReceive().CurrentValue;
}

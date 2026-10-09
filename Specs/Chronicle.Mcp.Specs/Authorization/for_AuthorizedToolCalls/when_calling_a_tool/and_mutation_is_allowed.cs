// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Contracts.Jobs;
using NSubstitute;

namespace Cratis.Chronicle.Mcp.Authorization.for_AuthorizedToolCalls.when_calling_a_tool;

public class and_mutation_is_allowed : given.a_filter
{
    async Task Because() => _result = await Call("stop_job");
    [Fact] void should_reach_the_job_tool() => _services.Jobs.Received(1).StopJob(Arg.Is<StopJobRequest>(request => request.JobId == _arguments["jobId"].GetGuid() && request.EventStore == Store && request.Namespace == Namespace));
    [Fact] void should_not_return_an_error() => (_result.IsError ?? false).ShouldBeFalse();
}

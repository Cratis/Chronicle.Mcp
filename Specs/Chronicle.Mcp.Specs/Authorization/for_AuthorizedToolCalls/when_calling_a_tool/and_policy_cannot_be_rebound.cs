// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Contracts.Jobs;
using Cratis.Chronicle.Mcp.Configuration;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Cratis.Chronicle.Mcp.Authorization.for_AuthorizedToolCalls.when_calling_a_tool;

public class and_policy_cannot_be_rebound : given.a_filter
{
    void Establish() => _monitor.CurrentValue.Returns(_ => throw new InvalidMcpConfiguration(McpOptionsBinding.SectionPath));
    async Task Because() => _result = await Call("stop_job");
    [Fact] void should_not_invoke_the_tool() => _services.Jobs.DidNotReceive().StopJob(Arg.Any<StopJobRequest>());
    [Fact] void should_report_an_invalid_policy() => _result.StructuredContent!.Value.GetProperty("error").GetProperty("code").GetString().ShouldEqual(DenialCodes.PolicyInvalid);
}

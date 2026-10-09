// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Contracts.Jobs;
using Cratis.Chronicle.Mcp.Configuration;
using NSubstitute;

namespace Cratis.Chronicle.Mcp.Authorization.for_AuthorizedToolCalls.when_calling_a_tool;

public class and_principal_is_revoked_between_calls : given.a_filter
{
    async Task Because()
    {
        await Call("resume_job");
        _monitor.CurrentValue.Returns(new McpServerOptions
        {
            Profile = DeploymentProfile.Mutation,
            Authorization = new() { Principal = Principal, ExpiresAt = Now.AddHours(1), Grants = [_grant], RevokedPrincipals = [Principal] }
        });
        _result = await Call("resume_job");
    }
    [Fact] void should_only_invoke_the_first_call() => _services.Jobs.Received(1).ResumeJob(Arg.Any<ResumeJobRequest>());
    [Fact] void should_deny_the_second_call() => _result.StructuredContent!.Value.GetProperty("error").GetProperty("code").GetString().ShouldEqual(DenialCodes.PrincipalRevoked);
}

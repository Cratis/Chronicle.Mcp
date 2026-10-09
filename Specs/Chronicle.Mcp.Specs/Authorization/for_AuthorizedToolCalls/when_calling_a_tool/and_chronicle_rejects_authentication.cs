// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Contracts.Jobs;
using Grpc.Core;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Cratis.Chronicle.Mcp.Authorization.for_AuthorizedToolCalls.when_calling_a_tool;

public class and_chronicle_rejects_authentication : given.a_filter
{
    void Establish() => _services.Jobs.StopJob(Arg.Any<StopJobRequest>()).ThrowsAsync(new RpcException(new(StatusCode.Unauthenticated, "sensitive-exception-text")));
    async Task Because() => _result = await Call("stop_job");
    [Fact] void should_report_chronicle_denied() => _result.StructuredContent!.Value.GetProperty("error").GetProperty("code").GetString().ShouldEqual(DenialCodes.ChronicleDenied);
    [Fact] void should_return_an_error() => _result.IsError.ShouldEqual(true);
}

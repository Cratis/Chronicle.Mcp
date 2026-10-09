// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Contracts.Jobs;
using Grpc.Core;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Cratis.Chronicle.Mcp.Authorization.for_AuthorizedToolCalls.when_calling_a_tool;

public class and_chronicle_denies_a_read : given.a_filter
{
    void Establish() => _services.Jobs.AllJobs(Arg.Any<AllJobsRequest>()).ThrowsAsync(new RpcException(new(StatusCode.PermissionDenied, "sensitive-exception-text")));
    async Task Because() => _result = await Call("list_jobs");
    [Fact] void should_report_chronicle_denied() => _result.StructuredContent!.Value.GetProperty("error").GetProperty("code").GetString().ShouldEqual(DenialCodes.ChronicleDenied);
    [Fact] void should_return_an_error() => _result.IsError.ShouldEqual(true);
    [Fact] void should_not_expose_exception_text() => _result.StructuredContent!.Value.GetRawText().Contains("sensitive-exception-text", StringComparison.Ordinal).ShouldBeFalse();
}

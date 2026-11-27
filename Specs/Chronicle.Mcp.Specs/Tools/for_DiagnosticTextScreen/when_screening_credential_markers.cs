// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Contracts.Observation;
using Cratis.Chronicle.Mcp.Specs.Tools.for_FailedPartitionTools.given;
using Cratis.Chronicle.Mcp.Tools;
using NSubstitute;
using ProtoBuf.Grpc;

namespace Cratis.Chronicle.Mcp.Specs.Tools.for_DiagnosticTextScreen;

public class when_screening_credential_markers : a_failed_partition_query
{
    static readonly string[] _markers =
    [
        "accessToken = synthetic-marker",
        "refreshToken: synthetic-marker",
        "clientSecret=synthetic-marker",
        "client_secret: synthetic-marker",
        "client secret = synthetic-marker",
        "apiKey: synthetic-marker",
        "API_KEY = synthetic-marker",
        "secret=synthetic-marker",
        "\"secret\": \"synthetic-marker\"",
        "access_token=synthetic-marker",
        "refresh_token=synthetic-marker",
        "Authorization: synthetic-marker",
        "Bearer synthetic-marker",
        "Password=synthetic-marker",
        "-----BEGIN ENCRYPTED PRIVATE KEY-----"
    ];
    static readonly string[] _prose = ["token expired", "password policy", "client secret rotation", "secretary: example", "unrelated_api_key: example"];

    FailedPartitionDescriptor[] _rows;

    void Establish() => _failures.GetFailedPartitions(Arg.Any<GetFailedPartitionsRequest>(), Arg.Any<CallContext>())
        .Returns(_markers.Concat(_prose).Select(Failure).ToArray());

    async Task Because() => _rows = (await FailedPartitionTools.ListFailedPartitions(_services, _configuration)).ToArray();

    [Fact] void should_withhold_each_credential_marker() =>
        _rows.Where(row => row.DiagnosticsRedacted).Select(row => row.LastFailureReason).ShouldEqual(Enumerable.Repeat("[withheld]", _markers.Length));

    [Fact] void should_preserve_prose_and_unrelated_keys() =>
        _rows.Where(row => !row.DiagnosticsRedacted).Select(row => row.LastFailureReason).ShouldEqual(_prose);

    static FailedPartition Failure(string text)
    {
        var attempt = AttemptAt(1000);
        attempt.Messages = [text];
        return new() { ObserverId = "observer", Partition = "partition", Attempts = [attempt] };
    }
}

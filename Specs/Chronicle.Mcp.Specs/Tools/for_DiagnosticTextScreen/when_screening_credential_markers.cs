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
        "Authorization: Basic dXNlcjpwYXNz",
        "BEARER eyJhbGciOifake..",
        "Password=synthetic-marker",
        "-----BEGIN ENCRYPTED PRIVATE KEY-----",
        "-----BEGIN PGP PRIVATE KEY BLOCK-----",
        "DB_PASSWORD=synthetic-marker",
        "AWS_SECRET_ACCESS_KEY=synthetic-marker",
        "aws-secret-access-key: synthetic-marker",
        "client.secret=synthetic-marker",
        "ClientSecret: synthetic-marker",
        "\"idToken\": \"synthetic-marker\"",
        "GITHUB_TOKEN=synthetic-marker",
        "userPassword: synthetic-marker",
        "café2Password: synthetic-marker",
        "config.idToken = synthetic-marker",
        "client-secret: synthetic-marker",
        "unrelated_api_key: example",
        "X-Api-Key: synthetic-marker",
        "X-Auth-Token: synthetic-marker",
        """{\"password\":\"synthetic-marker\"}""",
        """{\'password\':\'synthetic-marker\'}""",
        "Server=example.invalid;Pwd=synthetic-marker;Database=sample",
        "Server=example.invalid;Password=synthetic-marker;User ID=sample",
        "AccountName=sample;AccountKey=synthetic-marker",
        "Endpoint=example.invalid;SharedAccessKey=synthetic-marker",
        "Endpoint=example.invalid;SharedAccessSignature=synthetic-marker",
        "cloud.accessKey: synthetic-marker",
        "Secret-Key: synthetic-marker",
        "user.passwd=synthetic-marker",
        "serviceCredential: synthetic-marker",
        "serviceCredentials: synthetic-marker",
        "signing.privateKey: synthetic-marker",
        "Cookie: session=synthetic-marker",
        "Set-Cookie: session=synthetic-marker",
        "redis://:pass@host",
        "scheme://user:pass@host"
    ];
    static readonly string[] _prose =
    [
        "token expired",
        "password policy updated",
        "the secret sauce",
        "access key rotation scheduled",
        "Basic validation failed",
        "Bearer token missing",
        "Bearer 123456789012345",
        "client secret rotation",
        "secretary: example",
        "tokenCount: 3",
        "User ID=sample",
        "https://example.invalid:443/path",
        "https://example.invalid:443/users/member@example.invalid",
        "https://example.invalid:443?member=sample@example.invalid",
        "https://example.invalid:443#member@example.invalid",
        "https://sample@example.invalid"
    ];

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

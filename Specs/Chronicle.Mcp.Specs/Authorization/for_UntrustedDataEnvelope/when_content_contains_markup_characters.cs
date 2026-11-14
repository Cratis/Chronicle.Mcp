// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using ModelContextProtocol.Protocol;

namespace Cratis.Chronicle.Mcp.Authorization.for_UntrustedDataEnvelope;

public class when_content_contains_markup_characters : given.a_result
{
    const string Payload = "<>&'\n\u2028\u2029";

    void Establish() => _original.Content = [new TextContentBlock { Text = "\"<>&'\\n\u2028\u2029\"" }];
    void Because() => _result = UntrustedDataEnvelope.Wrap(_original, Tool);

    [Fact] void should_round_trip_characters() => JsonSerializer.Deserialize<string>(Body).ShouldEqual(Payload);
    [Fact] void should_escape_markup() => Body.ShouldContain("\\u003C\\u003E\\u0026\\u0027");
    [Fact] void should_escape_line_separators() => Body.ShouldContain("\\u2028\\u2029");
}

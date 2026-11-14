// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ModelContextProtocol.Protocol;

namespace Cratis.Chronicle.Mcp.Authorization.for_UntrustedDataEnvelope;

public class when_result_has_no_structured_content : given.a_result
{
    void Establish() => _original = new() { Content = [new TextContentBlock { Text = "[1,{\"event\":\"stored\"}]" }] };
    void Because() => _result = UntrustedDataEnvelope.Wrap(_original, Tool);

    [Fact] void should_parse_the_text_as_json() => Envelope.GetProperty("content")[1].GetProperty("event").GetString().ShouldEqual("stored");
}

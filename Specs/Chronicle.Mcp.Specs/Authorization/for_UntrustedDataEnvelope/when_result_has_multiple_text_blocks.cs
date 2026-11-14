// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ModelContextProtocol.Protocol;

namespace Cratis.Chronicle.Mcp.Authorization.for_UntrustedDataEnvelope;

public class when_result_has_multiple_text_blocks : given.a_result
{
    void Establish() => _original = new() { Content = [new TextContentBlock { Text = "1" }, new TextContentBlock { Text = "second" }] };
    void Because() => _result = UntrustedDataEnvelope.Wrap(_original, Tool);

    [Fact] void should_keep_both_blocks() => _result.Content.Count.ShouldEqual(2);
    [Fact] void should_wrap_every_text_block() => _result.Content.Cast<TextContentBlock>().All(block => block.Text.StartsWith(UntrustedDataEnvelope.Notice, StringComparison.Ordinal)).ShouldBeTrue();
    [Fact] void should_use_one_nonce_per_call() => ((TextContentBlock)_result.Content[1]).Text.Split('\n')[1].ShouldEqual(Opening);
    [Fact] void should_preserve_multiple_payloads_in_order() => Envelope.GetProperty("content")[0].GetInt32().ShouldEqual(1);
    [Fact] void should_preserve_the_second_payload() => Envelope.GetProperty("content")[1].GetString().ShouldEqual("second");
}

// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;

namespace Cratis.Chronicle.Mcp.Authorization.for_UntrustedDataEnvelope;

public class when_wrapping_a_successful_result : given.a_result
{
    void Because() => _result = UntrustedDataEnvelope.Wrap(_original, Tool);

    [Fact] void should_preserve_the_payload() => Envelope.GetProperty("content").GetProperty("message").GetString().ShouldEqual("stored event");
    [Fact] void should_identify_the_source() => Envelope.GetProperty("source").GetString().ShouldEqual("chronicle");
    [Fact] void should_identify_the_tool() => Envelope.GetProperty("tool").GetString().ShouldEqual(Tool);
    [Fact] void should_include_the_notice() => Envelope.GetProperty("notice").GetString().ShouldEqual(UntrustedDataEnvelope.Notice);
    [Fact] void should_start_with_the_notice() => Text.StartsWith(UntrustedDataEnvelope.Notice + "\n", StringComparison.Ordinal).ShouldBeTrue();
    [Fact] void should_use_matching_delimiters() => Closing.ShouldEqual(Opening.Replace("<<<CHRONICLE_", "<<<END_CHRONICLE_", StringComparison.Ordinal));
    [Fact] void should_use_a_128_bit_hex_nonce() => Convert.FromHexString(Opening.Split("id=", StringSplitOptions.None)[1][..^3]).Length.ShouldEqual(16);
    [Fact] void should_keep_the_text_payload() => JsonDocument.Parse(Body).RootElement.GetProperty("message").GetString().ShouldEqual("stored event");
    [Fact] void should_preserve_the_error_flag() => _result.IsError.ShouldBeNull();
}

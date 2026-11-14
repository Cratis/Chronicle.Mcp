// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ModelContextProtocol.Protocol;

namespace Cratis.Chronicle.Mcp.Authorization.for_UntrustedDataEnvelope;

public class when_result_has_non_text_blocks : given.a_result
{
    ImageContentBlock _image;

    void Establish()
    {
        _image = new() { Data = new byte[] { 1 }, MimeType = "image/png" };
        _original.Content.Add(_image);
    }

    void Because() => _result = UntrustedDataEnvelope.Wrap(_original, Tool);

    [Fact] void should_preserve_the_image() => _result.Content[1].ShouldBeSame(_image);
    [Fact] void should_still_wrap_text() => Text.StartsWith(UntrustedDataEnvelope.Notice, StringComparison.Ordinal).ShouldBeTrue();
}

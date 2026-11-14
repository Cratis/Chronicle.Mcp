// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ModelContextProtocol.Protocol;

namespace Cratis.Chronicle.Mcp.Authorization.for_UntrustedDataEnvelope;

public class when_wrapping_twice_nonces_differ : given.a_result
{
    string _first;
    string _second;

    void Because()
    {
        _first = ((TextContentBlock)UntrustedDataEnvelope.Wrap(new() { Content = [new TextContentBlock { Text = "data" }] }, Tool).Content[0]).Text.Split('\n')[1];
        _second = ((TextContentBlock)UntrustedDataEnvelope.Wrap(new() { Content = [new TextContentBlock { Text = "data" }] }, Tool).Content[0]).Text.Split('\n')[1];
    }

    [Fact] void should_generate_a_new_nonce_per_call() => _first.ShouldNotEqual(_second);
}

// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Contracts.Host;
using ModelContextProtocol.Protocol;
using NSubstitute;

namespace Cratis.Chronicle.Mcp.Authorization.for_AuthorizedToolCalls.when_calling_a_tool;

public class and_a_server_tool_is_trusted : given.a_filter
{
    void Establish()
    {
        _arguments.Clear();
        _services.Server.GetVersionInfo().Returns(new ServerVersionInfo { Version = "1.2.3", CommitSha = "test-commit" });
    }

    async Task Because() => _result = await Call("get_server_version");

    [Fact] void should_not_wrap_server_information() => ((TextContentBlock)_result.Content[0]).Text.ShouldNotContain("CHRONICLE_UNTRUSTED_DATA");
    [Fact] void should_preserve_server_information() => ((TextContentBlock)_result.Content[0]).Text.ShouldContain("1.2.3");
    [Fact] void should_confirm_the_sdk_uses_text_without_structured_content() => _result.StructuredContent.ShouldBeNull();
}

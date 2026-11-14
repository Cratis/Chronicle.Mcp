// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ModelContextProtocol.Protocol;

namespace Cratis.Chronicle.Mcp.Authorization.for_ToolCatalog;

public class when_classifying_untrusted_results : Specs.given.a_policy
{
    string[] _wrapped;
    string[] _trusted;

    void Because()
    {
        var classified = _catalog.Tools.Select(tool => new
        {
            tool.Name,
            IsWrapped = UntrustedDataEnvelope.Wrap(new() { Content = [new TextContentBlock { Text = "payload" }] }, tool.Name).StructuredContent.HasValue
        }).ToArray();
        _wrapped = classified.Where(tool => tool.IsWrapped).Select(tool => tool.Name).ToArray();
        _trusted = classified.Where(tool => !tool.IsWrapped).Select(tool => tool.Name).ToArray();
    }

    [Fact] void should_check_a_nonempty_catalog() => _wrapped.ShouldNotBeEmpty();
    [Fact] void should_trust_only_server_information() => _trusted.ShouldContainOnly("get_server_version");
    [Fact] void should_wrap_every_other_tool() => _wrapped.Length.ShouldEqual(_catalog.Tools.Count() - 1);
    [Fact] void should_wrap_future_tools_by_default() => UntrustedDataEnvelope.Wrap(new(), "a_future_tool").StructuredContent.HasValue.ShouldBeTrue();
}

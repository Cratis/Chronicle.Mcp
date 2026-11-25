// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Mcp.Configuration;

namespace Cratis.Chronicle.Mcp.Connection.for_McpTokenCache;

public class when_selecting_cache_paths : Specification
{
    string _selected;
    string _otherServer;
    string _otherContext;
    string _otherClient;

    void Because()
    {
        _selected = McpTokenCache.GetPath("server.example:35000", "default", "client");
        _otherServer = McpTokenCache.GetPath("other.example:35000", "default", "client");
        _otherContext = McpTokenCache.GetPath("server.example:35000", "other", "client");
        _otherClient = McpTokenCache.GetPath("server.example:35000", "default", "other");
    }

    [Fact] void should_separate_destinations() => _selected.ShouldNotEqual(_otherServer);
    [Fact] void should_separate_contexts() => _selected.ShouldNotEqual(_otherContext);
    [Fact] void should_separate_clients() => _selected.ShouldNotEqual(_otherClient);
    [Fact] void should_not_use_the_shared_cli_cache() => Path.GetDirectoryName(_selected).ShouldNotEqual(Path.GetDirectoryName(ChronicleCliConfiguration.GetTokenCachePath("default_client")));
}

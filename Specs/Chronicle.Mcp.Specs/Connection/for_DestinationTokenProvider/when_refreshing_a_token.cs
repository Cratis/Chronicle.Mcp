// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Connection.for_DestinationTokenProvider;

public class when_refreshing_a_token : given.a_token_provider
{
    void Establish() => Cache(Destination);
    async Task Because() => _result = await _provider.Refresh();

    [Fact] void should_replace_the_cached_token() => _result.ShouldEqual("refreshed-token");
    [Fact] void should_bind_the_refreshed_token() => ReadDestination().ShouldEqual(Destination);
}

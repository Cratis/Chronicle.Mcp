// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Connection.for_DestinationTokenProvider;

public class when_cache_belongs_to_another_destination : given.a_token_provider
{
    void Establish() => Cache("other.example:35000");
    async Task Because() => _result = await _provider.GetAccessToken();

    [Fact] void should_not_reuse_the_other_servers_token() => _result.ShouldEqual("fresh-token");
    [Fact] void should_record_the_selected_destination() => ReadDestination().ShouldEqual(Destination);
}

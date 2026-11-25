// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Connection.for_DestinationTokenProvider;

public class when_cache_has_expired : given.a_token_provider
{
    void Establish() => Cache(Destination, DateTimeOffset.UtcNow.AddMinutes(-1));
    async Task Because() => _result = await _provider.GetAccessToken();

    [Fact] void should_acquire_a_fresh_token() => _result.ShouldEqual("fresh-token");
}

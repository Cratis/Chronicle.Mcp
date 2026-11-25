// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using NSubstitute;

namespace Cratis.Chronicle.Mcp.Connection.for_DestinationTokenProvider;

public class when_cache_matches_the_destination : given.a_token_provider
{
    void Establish() => Cache(Destination);
    async Task Because() => _result = await _provider.GetAccessToken();

    [Fact] void should_reuse_the_matching_token() => _result.ShouldEqual("cached-token");
    [Fact] void should_not_reauthenticate() => _inner.DidNotReceiveWithAnyArgs().GetAccessToken(TestContext.Current.CancellationToken);
}

// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Connection.for_DestinationTokenProvider;

public class when_cache_is_malformed : given.a_token_provider
{
    void Establish() => File.WriteAllText(_cachePath, "not-json");
    async Task Because() => _result = await _provider.GetAccessToken();

    [Fact] void should_authenticate_instead_of_reusing_the_cache() => _result.ShouldEqual("fresh-token");
}

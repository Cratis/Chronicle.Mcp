// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Connection.for_DestinationTokenProvider;

public class when_cache_has_no_destination : given.a_token_provider
{
    void Establish() => File.WriteAllText(_cachePath, $"{{\"AccessToken\":\"unscoped-cli-token\",\"Expiry\":\"{DateTimeOffset.UtcNow.AddHours(1):O}\"}}");
    async Task Because() => _result = await _provider.GetAccessToken();

    [Fact] void should_never_reuse_an_unscoped_token() => _result.ShouldEqual("fresh-token");
}

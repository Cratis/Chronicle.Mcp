// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Configuration.for_DeploymentProfileStartup.when_starting;

public class and_an_api_key_is_used : given.a_startup
{
    void Establish() => _settings.ApiKey = "synthetic-api-key";
    async Task Because() => _error = await Catch.Exception(() => _startup.StartAsync(CancellationToken.None));
    [Fact] void should_start() => _error.ShouldBeNull();
    [Fact] void should_recommend_dedicated_client_credentials() => Warnings.Any(message => message.StartsWith("API keys and reused CLI login tokens", StringComparison.Ordinal)).ShouldBeTrue();
    [Fact] void should_not_log_the_key() => Warnings.Any(message => message.Contains("synthetic-api-key", StringComparison.Ordinal)).ShouldBeFalse();
}

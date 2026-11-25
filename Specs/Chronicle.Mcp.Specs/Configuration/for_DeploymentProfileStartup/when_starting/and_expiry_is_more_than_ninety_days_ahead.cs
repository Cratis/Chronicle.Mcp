// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Configuration.for_DeploymentProfileStartup.when_starting;

public class and_expiry_is_more_than_ninety_days_ahead : given.a_startup
{
    void Establish() => _settings.Authorization.ExpiresAt = Now.AddDays(91);
    async Task Because() => _error = await Catch.Exception(() => _startup.StartAsync(CancellationToken.None));
    [Fact] void should_start() => _error.ShouldBeNull();
    [Fact] void should_warn_about_the_long_lived_policy() => Warnings.Any(message => message.Contains("90 days", StringComparison.Ordinal)).ShouldBeTrue();
}

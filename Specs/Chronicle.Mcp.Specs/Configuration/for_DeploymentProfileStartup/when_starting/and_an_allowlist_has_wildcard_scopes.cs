// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Configuration.for_DeploymentProfileStartup.when_starting;

public class and_an_allowlist_has_wildcard_scopes : given.a_startup
{
    void Establish() => _settings.Authorization = new() { ExpiresAt = Now.AddHours(1), Allowlist = [new() { EventStore = "*", Namespace = "*", Operations = ["stop_job"] }] };
    async Task Because() => _error = await Catch.Exception(() => _startup.StartAsync(CancellationToken.None));
    [Fact] void should_start_without_a_principal() => _error.ShouldBeNull();
    [Fact] void should_warn_about_the_unbounded_scope() => Warnings.Any(message => message.Contains("wildcard", StringComparison.Ordinal)).ShouldBeTrue();
    [Fact] void should_not_warn_about_legacy_selectors() => Warnings.Any(message => message.StartsWith("Legacy", StringComparison.Ordinal)).ShouldBeFalse();
}

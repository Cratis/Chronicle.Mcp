// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Options;

namespace Cratis.Chronicle.Mcp.Configuration.for_DeploymentProfileStartup.when_starting;

public class and_an_allowlist_expires_now : given.a_startup
{
    void Establish() => _settings.Authorization = new() { ExpiresAt = Now, Allowlist = [new() { EventStore = Store, Namespace = Namespace, Operations = ["stop_job"] }] };
    async Task Because() => _error = await Catch.Exception(() => _startup.StartAsync(CancellationToken.None));
    [Fact] void should_reject_an_already_expired_policy() => _error.ShouldBeOfExactType<OptionsValidationException>();
    [Fact] void should_report_the_expiration_path() => ((OptionsValidationException)_error).Failures.ShouldEqual(new[] { $"{McpOptionsBinding.SectionPath}:Authorization:ExpiresAt" });
}

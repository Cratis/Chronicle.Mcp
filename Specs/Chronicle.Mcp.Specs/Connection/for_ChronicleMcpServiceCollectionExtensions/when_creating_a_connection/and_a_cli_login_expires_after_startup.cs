// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Connections;
using Cratis.Chronicle.Mcp.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Cratis.Chronicle.Mcp.Connection.for_ChronicleMcpServiceCollectionExtensions.when_creating_a_connection;

public class and_a_cli_login_expires_after_startup : given.a_delayed_connection
{
    bool _initiallyUsedBearer;
    bool _fellBackToDevelopment;

    async Task Because()
    {
        _startupError = await Catch.Exception(Start);
        var credentials = _provider.GetRequiredService<MutationCredentialPolicy>();
        _initiallyUsedBearer = credentials.UsesBearerCredential;
        _clock.GetUtcNow().Returns(_now.AddMinutes(1));
        _fellBackToDevelopment = credentials.UsesDevelopmentCredentials;
        _connectionError = Catch.Exception(() => _provider.GetRequiredService<IChronicleConnection>());
    }

    [Fact] void should_accept_the_initial_destination_bound_login() => _startupError.ShouldBeNull();
    [Fact] void should_initially_use_a_bearer_token() => _initiallyUsedBearer.ShouldBeTrue();
    [Fact] void should_reproduce_the_fallback_at_the_expiry_safety_boundary() => _fellBackToDevelopment.ShouldBeTrue();
    [Fact] void should_reject_the_actual_connection_before_creating_a_provider() => _connectionError.ShouldBeOfExactType<OptionsValidationException>();
    [Fact] void should_report_only_the_opt_in_setting() => ((OptionsValidationException)_connectionError).Failures.ShouldEqual(new[] { $"{McpOptionsBinding.SectionPath}:Authorization:AllowDevelopmentCredentials" });
}

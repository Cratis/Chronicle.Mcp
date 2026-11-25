// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Mcp.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Cratis.Chronicle.Mcp.Configuration.for_DeploymentProfileStartup.when_starting;

public class and_a_cli_login_token_is_reused : for_ChronicleConnectionConfiguration.given.a_connection_configuration
{
    DeploymentProfileStartup _startup;
    ILogger<MutationPolicyWarnings> _logger;
    Exception? _error;

    void Establish()
    {
        EnableCli();
        _options.Profile = DeploymentProfile.Mutation;
        _options.Authorization = new() { ExpiresAt = DateTimeOffset.UtcNow.AddHours(1), Allowlist = [new() { EventStore = "orders", Namespace = "support", Operations = ["stop_job"] }] };
        var monitor = Substitute.For<IOptionsMonitor<McpServerOptions>>();
        monitor.CurrentValue.Returns(_options);
        var credentials = new MutationCredentialPolicy(new(Options.Create(_options)));
        _logger = Substitute.For<ILogger<MutationPolicyWarnings>>();
        _logger.IsEnabled(LogLevel.Warning).Returns(true);
        _startup = new(monitor, new(new(typeof(ToolCatalog).Assembly)), TimeProvider.System, credentials, new(credentials, _logger, TimeProvider.System));
    }

    async Task Because() => _error = await Catch.Exception(() => _startup.StartAsync(CancellationToken.None));
    [Fact] void should_accept_the_destination_bound_login() => _error.ShouldBeNull();
    [Fact] void should_warn_about_the_deprecated_credential_type() => _logger.ReceivedCalls().Any(call => call.GetMethodInfo().Name == "Log" && call.GetArguments()[2]!.ToString()!.StartsWith("API keys and reused CLI login tokens", StringComparison.Ordinal)).ShouldBeTrue();
    void Destroy() => _startup.Dispose();
}

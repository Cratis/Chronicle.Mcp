// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Mcp.Authorization;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Cratis.Chronicle.Mcp.Configuration;

/// <summary>
/// Refuses startup without a valid deployment policy. Runtime decisions remain per-call.
/// </summary>
/// <param name="options">The current deployment settings.</param>
/// <param name="policy">The shared grant validator.</param>
/// <param name="timeProvider">The clock used for expiration.</param>
public class DeploymentProfileStartup(IOptionsMonitor<McpServerOptions> options, MutationPolicy policy, TimeProvider timeProvider) : IHostedService
{
    /// <inheritdoc/>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        var settings = options.CurrentValue;
        var failures = new List<string>();
        if (!Enum.IsDefined(settings.Profile)) failures.Add($"{McpOptionsBinding.SectionPath}:Profile");
        if (settings.Profile == DeploymentProfile.Mutation)
        {
            var authorization = settings.Authorization;
            if (string.IsNullOrWhiteSpace(authorization.Principal)) failures.Add($"{McpOptionsBinding.SectionPath}:Authorization:Principal");
            if (authorization.ExpiresAt is null || authorization.ExpiresAt <= timeProvider.GetUtcNow()) failures.Add($"{McpOptionsBinding.SectionPath}:Authorization:ExpiresAt");
            failures.AddRange(policy.InvalidSettings(authorization));
        }

        if (failures.Count > 0)
        {
            throw new OptionsValidationException(Options.DefaultName, typeof(McpServerOptions), failures);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

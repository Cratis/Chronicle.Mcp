// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Mcp.Authorization;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Cratis.Chronicle.Mcp.Configuration;

/// <summary>
/// Validates mutation authority before accepting tool calls. Read-only needs no mutation policy.
/// </summary>
/// <param name="options">The bound options.</param>
/// <param name="policy">The policy validator.</param>
/// <param name="timeProvider">The clock used for expiration.</param>
/// <param name="credentials">The effective credential classifier.</param>
/// <param name="warnings">The operator warnings.</param>
public class DeploymentProfileStartup(IOptionsMonitor<McpServerOptions> options, MutationPolicy policy, TimeProvider timeProvider, MutationCredentialPolicy credentials, MutationPolicyWarnings warnings) : IHostedService, IDisposable
{
    IDisposable? _subscription;

    /// <inheritdoc/>
    public void Dispose() => _subscription?.Dispose();

    /// <inheritdoc/>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        Dispose();
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        var settings = options.CurrentValue;
        var failures = new List<string>();
        if (!Enum.IsDefined(settings.Profile)) failures.Add($"{McpOptionsBinding.SectionPath}:Profile");
        if (settings.Profile == DeploymentProfile.Mutation)
        {
            var authorization = settings.Authorization;
            if (authorization.UsesLegacyPolicy && string.IsNullOrWhiteSpace(authorization.Principal)) failures.Add($"{McpOptionsBinding.SectionPath}:Authorization:Principal");
            if (authorization.ExpiresAt is null || authorization.ExpiresAt <= timeProvider.GetUtcNow()) failures.Add($"{McpOptionsBinding.SectionPath}:Authorization:ExpiresAt");
            failures.AddRange(policy.InvalidSettings(authorization));
            ValidateCredentials(authorization, failures);
        }

        if (failures.Count > 0) throw new OptionsValidationException(nameof(McpServerOptions), typeof(McpServerOptions), failures);
        warnings.Report(settings);
        _subscription = options.OnChange((updated, _) => warnings.Report(updated));
        return Task.CompletedTask;
    }

    void ValidateCredentials(MutationAuthorization authorization, List<string> failures)
    {
        try
        {
            if (credentials.UsesDevelopmentCredentials && !authorization.AllowDevelopmentCredentials) failures.Add($"{McpOptionsBinding.SectionPath}:Authorization:AllowDevelopmentCredentials");
        }
        catch (Exception)
        {
            failures.Add($"{McpOptionsBinding.SectionPath}:ConnectionString");
        }
    }
}

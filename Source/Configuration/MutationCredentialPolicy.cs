// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Connections;
using Microsoft.Extensions.Options;

namespace Cratis.Chronicle.Mcp.Configuration;

/// <summary>
/// Checks the effective credentials without changing connection resolution or exposing their values.
/// </summary>
/// <param name="configuration">Lazily obtains the shared connection resolver only when credentials are needed.</param>
public class MutationCredentialPolicy(Func<ChronicleConnectionConfiguration> configuration)
{
    /// <summary>
    /// Gets whether the effective credentials use an API key or a reused CLI login token.
    /// </summary>
    public bool UsesBearerCredential => !string.IsNullOrWhiteSpace(new ChronicleConnectionString(configuration().ResolveConnectionString()).ApiKey);

    /// <summary>
    /// Gets whether the effective credentials are the well-known development client pair.
    /// </summary>
    public bool UsesDevelopmentCredentials => IsDevelopmentCredentials(new(configuration().ResolveConnectionString()));

    /// <summary>
    /// Validates the exact resolved credentials that will be used to create the connection.
    /// </summary>
    /// <param name="connection">The resolved connection, without resolving credentials again.</param>
    /// <param name="settings">The connection's deployment settings.</param>
    /// <exception cref="OptionsValidationException">Mutation would use development credentials without an explicit opt-in.</exception>
    public static void ValidateConnection(ChronicleConnectionString connection, McpServerOptions settings)
    {
        if (settings.Profile == DeploymentProfile.Mutation && !settings.Authorization.AllowDevelopmentCredentials && IsDevelopmentCredentials(connection))
        {
            throw new OptionsValidationException(nameof(McpServerOptions), typeof(McpServerOptions), [$"{McpOptionsBinding.SectionPath}:Authorization:AllowDevelopmentCredentials"]);
        }
    }

    static bool IsDevelopmentCredentials(ChronicleConnectionString connection) =>
        string.Equals(Uri.UnescapeDataString(connection.Username ?? string.Empty), ChronicleConnectionString.DevelopmentClient, StringComparison.Ordinal) &&
        string.Equals(Uri.UnescapeDataString(connection.Password ?? string.Empty), ChronicleConnectionString.DevelopmentClientSecret, StringComparison.Ordinal);
}

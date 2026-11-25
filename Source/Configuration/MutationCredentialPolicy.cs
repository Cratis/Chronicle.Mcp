// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Connections;

namespace Cratis.Chronicle.Mcp.Configuration;

/// <summary>
/// Checks the effective credentials without changing connection resolution or exposing their values.
/// </summary>
/// <param name="configuration">The shared connection resolver.</param>
public class MutationCredentialPolicy(ChronicleConnectionConfiguration configuration)
{
    /// <summary>
    /// Gets whether the effective credentials use an API key or a reused CLI login token.
    /// </summary>
    public bool UsesBearerCredential => !string.IsNullOrWhiteSpace(new ChronicleConnectionString(configuration.ResolveConnectionString()).ApiKey);

    /// <summary>
    /// Gets whether the effective credentials are the well-known development client pair.
    /// </summary>
    public bool UsesDevelopmentCredentials
    {
        get
        {
            var connection = new ChronicleConnectionString(configuration.ResolveConnectionString());
            return string.Equals(Uri.UnescapeDataString(connection.Username ?? string.Empty), ChronicleConnectionString.DevelopmentClient, StringComparison.Ordinal) &&
                string.Equals(Uri.UnescapeDataString(connection.Password ?? string.Empty), ChronicleConnectionString.DevelopmentClientSecret, StringComparison.Ordinal);
        }
    }
}

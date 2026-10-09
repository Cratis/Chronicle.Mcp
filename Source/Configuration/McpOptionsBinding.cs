// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Configuration;

namespace Cratis.Chronicle.Mcp.Configuration;

/// <summary>
/// Binds deployment settings without exposing invalid configuration values in errors.
/// </summary>
public static class McpOptionsBinding
{
    /// <summary>
    /// The root configuration path.
    /// </summary>
    public const string SectionPath = "Cratis:Chronicle:Mcp";

    /// <summary>
    /// Binds settings, rejecting unknown profiles before the enum binder runs.
    /// </summary>
    /// <param name="section">The MCP configuration section.</param>
    /// <param name="options">The options to populate.</param>
    /// <exception cref="InvalidMcpConfiguration">A setting cannot be bound.</exception>
    public static void Bind(IConfiguration section, McpServerOptions options)
    {
        var profile = section[nameof(McpServerOptions.Profile)];
        if (profile is not null && (!Enum.TryParse<DeploymentProfile>(profile, out var parsed) || !Enum.IsDefined(parsed)))
        {
            throw new InvalidMcpConfiguration($"{SectionPath}:Profile");
        }

        try
        {
            section.Bind(options);
        }
        catch (InvalidOperationException)
        {
            // Binding failures can include credentials or policy values; expose only the owning path.
            throw new InvalidMcpConfiguration(SectionPath);
        }
    }
}

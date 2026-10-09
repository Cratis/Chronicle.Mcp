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

        AuthorizationConfigurationShape.Validate(section);

        try
        {
            // Only the authorization policy is bound strictly: a dropped value there could fail open, while an
            // obsolete connection setting (such as the removed ManagementPort) must not stop an upgraded server.
            section.Bind(options);
            var authorization = new MutationAuthorization();
            section.GetSection(nameof(McpServerOptions.Authorization)).Bind(authorization, binder => binder.ErrorOnUnknownConfiguration = true);
            options.Authorization = authorization;
        }
        catch (InvalidOperationException)
        {
            // Binding failures can include credentials or policy values; expose only the owning path.
            throw new InvalidMcpConfiguration(SectionPath);
        }
    }
}

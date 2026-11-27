// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;
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
            RejectNonIndexedModels(section, options);
            var authorization = new MutationAuthorization();
            section.GetSection(nameof(McpServerOptions.Authorization)).Bind(authorization, binder => binder.ErrorOnUnknownConfiguration = true);
            var sectionPolicy = section.GetSection(nameof(McpServerOptions.Authorization));
            authorization.LegacyConfigured = sectionPolicy.GetChildren().Any(child =>
                new[] { "Principal", "Grants", "RevokedPrincipals" }.Contains(child.Key, StringComparer.OrdinalIgnoreCase));
            if (sectionPolicy.GetChildren().Any(child => string.Equals(child.Key, "Allowlist", StringComparison.OrdinalIgnoreCase))) authorization.Allowlist ??= [];
            options.Authorization = authorization;
        }
        catch (InvalidOperationException)
        {
            // Binding failures can include credentials or policy values; expose only the owning path.
            throw new InvalidMcpConfiguration(SectionPath);
        }
    }

    static void RejectNonIndexedModels(IConfiguration section, McpServerOptions options)
    {
        // Bind model names as values: configuration keys are case-insensitive and merge across declarations.
        var models = section.GetSection(nameof(McpServerOptions.ModelCatalog)).GetSection(nameof(ScreenplayModelCatalogOptions.Models));
        if (IsIndexedModelList(models, options)) return;
        options.ModelCatalog = new();
    }

    static bool IsIndexedModelList(IConfigurationSection models, McpServerOptions options)
    {
        if (models.Value is not null) return false;
        var entries = models.GetChildren().ToArray();
        return entries.Length == options.ModelCatalog?.Models?.Count && entries.All(IsIndexedModel);
    }

    static bool IsIndexedModel(IConfigurationSection entry) =>
        entry.Value is null && uint.TryParse(entry.Key, NumberStyles.None, CultureInfo.InvariantCulture, out var index) &&
        entry.Key == index.ToString(CultureInfo.InvariantCulture);
}

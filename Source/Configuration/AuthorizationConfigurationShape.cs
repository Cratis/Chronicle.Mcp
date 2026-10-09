// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;
using Microsoft.Extensions.Configuration;

namespace Cratis.Chronicle.Mcp.Configuration;

/// <summary>
/// Rejects authorization shapes the configuration binder would otherwise silently ignore.
/// </summary>
public static class AuthorizationConfigurationShape
{
    static readonly IReadOnlyDictionary<string, Action<IConfigurationSection>> _grantSettings = new Dictionary<string, Action<IConfigurationSection>>(StringComparer.OrdinalIgnoreCase)
    {
        [nameof(MutationGrant.Principal)] = ValidateScalar,
        [nameof(MutationGrant.EventStore)] = ValidateScalar,
        [nameof(MutationGrant.Namespace)] = ValidateScalar,
        [nameof(MutationGrant.Operations)] = section => ValidateCollection(section, ValidateScalarElement),
        [nameof(MutationGrant.Targets)] = section => ValidateCollection(section, ValidateScalarElement)
    };

    static readonly IReadOnlyDictionary<string, Action<IConfigurationSection>> _authorizationSettings = new Dictionary<string, Action<IConfigurationSection>>(StringComparer.OrdinalIgnoreCase)
    {
        [nameof(MutationAuthorization.Principal)] = ValidateScalar,
        [nameof(MutationAuthorization.ExpiresAt)] = ValidateScalar,
        [nameof(MutationAuthorization.RevokedPrincipals)] = section => ValidateCollection(section, ValidateScalarElement),
        [nameof(MutationAuthorization.Grants)] = section => ValidateCollection(section, ValidateGrant)
    };

    /// <summary>
    /// Checks objects, indexed collections, and scalar leaves before binding the policy.
    /// </summary>
    /// <param name="section">The MCP configuration section.</param>
    /// <exception cref="InvalidMcpConfiguration">An authorization setting has an invalid shape or name.</exception>
    public static void Validate(IConfiguration section) => ValidateObject(section.GetSection(nameof(McpServerOptions.Authorization)), _authorizationSettings);

    static void ValidateGrant(IConfigurationSection section) => ValidateObject(section, _grantSettings);

    static void ValidateObject(IConfigurationSection section, IReadOnlyDictionary<string, Action<IConfigurationSection>> settings)
    {
        if (!string.IsNullOrEmpty(section.Value))
        {
            throw Invalid(section);
        }

        foreach (var child in section.GetChildren())
        {
            if (!settings.TryGetValue(child.Key, out var validateSetting))
            {
                throw Invalid(child);
            }

            validateSetting(child);
        }
    }

    static void ValidateCollection(IConfigurationSection section, Action<IConfigurationSection> validateElement)
    {
        if (!string.IsNullOrEmpty(section.Value))
        {
            throw Invalid(section);
        }

        foreach (var child in section.GetChildren())
        {
            if (!int.TryParse(child.Key, NumberStyles.None, CultureInfo.InvariantCulture, out _))
            {
                throw Invalid(child);
            }

            validateElement(child);
        }
    }

    static void ValidateScalarElement(IConfigurationSection section)
    {
        if (section.Value is null)
        {
            throw Invalid(section);
        }

        ValidateScalar(section);
    }

    static void ValidateScalar(IConfigurationSection section)
    {
        if (section.GetChildren().Any())
        {
            throw Invalid(section);
        }
    }

    static InvalidMcpConfiguration Invalid(IConfigurationSection section) => new(
        section.Path.StartsWith($"{McpOptionsBinding.SectionPath}:", StringComparison.OrdinalIgnoreCase)
            ? section.Path
            : $"{McpOptionsBinding.SectionPath}:{section.Path}");
}

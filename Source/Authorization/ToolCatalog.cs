// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;
using Cratis.Chronicle.Mcp.Configuration;
using ModelContextProtocol.Server;

namespace Cratis.Chronicle.Mcp.Authorization;

/// <summary>
/// Owns tool discovery and the classification shared by registration and authorization.
/// </summary>
/// <param name="assembly">The assembly containing tool methods.</param>
public class ToolCatalog(Assembly assembly)
{
    const BindingFlags DeclaredMethods = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly;

    readonly CatalogTool[] _tools = assembly.GetTypes()
        .SelectMany(type => type.GetMethods(DeclaredMethods))
        .Where(method => method.IsDefined(typeof(McpServerToolAttribute)))
        .Select(Describe)
        .ToArray();

    /// <summary>
    /// Gets all discovered tools.
    /// </summary>
    public IEnumerable<CatalogTool> Tools => _tools;

    /// <summary>
    /// Selects the tools exposed by a deployment profile.
    /// </summary>
    /// <param name="profile">The startup profile.</param>
    /// <returns>The permitted tools; an unknown profile exposes none.</returns>
    public IEnumerable<CatalogTool> ForProfile(DeploymentProfile profile) => _tools.Where(tool =>
        profile == DeploymentProfile.Mutation || (profile == DeploymentProfile.ReadOnly && tool.IsReadOnly));

    /// <summary>
    /// Finds a tool by its exact protocol name.
    /// </summary>
    /// <param name="name">The protocol name.</param>
    /// <returns>The tool, or null if not registered in the catalog.</returns>
    public CatalogTool? Find(string name) => _tools.FirstOrDefault(tool => string.Equals(tool.Name, name, StringComparison.Ordinal));

    static CatalogTool Describe(MethodInfo method)
    {
        var attribute = method.GetCustomAttributesData().Single(attribute => attribute.AttributeType == typeof(McpServerToolAttribute));
        var isReadOnly = attribute.NamedArguments.Any(argument => argument.MemberName == nameof(McpServerToolAttribute.ReadOnly) && argument.TypedValue.Value is true);
        var tool = method.IsStatic
            ? McpServerTool.Create(method, (object?)null)
            : McpServerTool.Create(method, _ => null!);

        return new(tool.ProtocolTool.Name, method, isReadOnly);
    }
}

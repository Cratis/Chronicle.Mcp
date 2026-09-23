// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;
using ModelContextProtocol.Server;

namespace Cratis.Chronicle.Mcp.Specs.Tools.for_mcp_server_tools.given;

/// <summary>
/// Represents a method exposed as an MCP tool, with the annotation properties it explicitly declares.
/// </summary>
/// <param name="Name">The tool name, or the declaring type and method name when no name is declared.</param>
/// <param name="Annotations">The explicitly declared named arguments of its <see cref="McpServerToolAttribute"/>.</param>
public record DeclaredTool(string Name, IReadOnlyDictionary<string, object?> Annotations)
{
    /// <summary>
    /// Creates a <see cref="DeclaredTool"/> from a method and its <see cref="McpServerToolAttribute"/> data.
    /// </summary>
    /// <param name="method">The method carrying the attribute.</param>
    /// <param name="attribute">The <see cref="CustomAttributeData"/> of the attribute.</param>
    /// <returns>The <see cref="DeclaredTool"/>.</returns>
    public static DeclaredTool From(MethodInfo method, CustomAttributeData attribute)
    {
        var annotations = attribute.NamedArguments.ToDictionary(argument => argument.MemberName, argument => argument.TypedValue.Value, StringComparer.Ordinal);
        var name = annotations.TryGetValue(nameof(McpServerToolAttribute.Name), out var declaredName) && declaredName is string toolName
            ? toolName
            : $"{method.DeclaringType?.Name}.{method.Name}";

        return new(name, annotations);
    }

    /// <summary>
    /// Checks whether the tool explicitly declares an annotation property, whatever its value.
    /// </summary>
    /// <param name="property">The name of the <see cref="McpServerToolAttribute"/> property.</param>
    /// <returns>True if the property is explicitly declared, false if it is left at its default.</returns>
    public bool Declares(string property) => Annotations.ContainsKey(property);

    /// <summary>
    /// Checks whether the tool explicitly declares an annotation property as <see langword="true"/>.
    /// </summary>
    /// <param name="property">The name of the <see cref="McpServerToolAttribute"/> property.</param>
    /// <returns>True if the property is explicitly declared as true.</returns>
    public bool DeclaresTrue(string property) => Annotations.TryGetValue(property, out var value) && value is true;

    /// <summary>
    /// Checks whether the tool explicitly declares an annotation property as <see langword="false"/>.
    /// </summary>
    /// <param name="property">The name of the <see cref="McpServerToolAttribute"/> property.</param>
    /// <returns>True if the property is explicitly declared as false.</returns>
    public bool DeclaresFalse(string property) => Annotations.TryGetValue(property, out var value) && value is false;
}

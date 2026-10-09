// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;

namespace Cratis.Chronicle.Mcp.Authorization;

/// <summary>
/// Describes a discovered tool and its explicit read-only classification.
/// </summary>
/// <param name="Name">The protocol tool name.</param>
/// <param name="Method">The method implementing the tool.</param>
/// <param name="IsReadOnly">Whether ReadOnly = true was explicitly declared.</param>
public record CatalogTool(string Name, MethodInfo Method, bool IsReadOnly)
{
    /// <summary>
    /// Gets the parameter scoped by mutation grants, if there is exactly one.
    /// </summary>
    public ParameterInfo? Target => Method.GetParameters().Where(parameter => parameter.IsDefined(typeof(AuthorizationTargetAttribute))).ToArray() is [var target] ? target : null;
}

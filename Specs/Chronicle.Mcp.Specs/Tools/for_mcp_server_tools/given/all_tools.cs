// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;
using Cratis.Chronicle.Mcp.Tools;
using ModelContextProtocol.Server;

namespace Cratis.Chronicle.Mcp.Specs.Tools.for_mcp_server_tools.given;

public class all_tools : Specification
{
    const BindingFlags DeclaredMethods = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly;

    protected IReadOnlyList<DeclaredTool> _tools;

    void Establish() => _tools = typeof(ToolContext).Assembly
        .GetTypes()
        .SelectMany(type => type.GetMethods(DeclaredMethods))
        .SelectMany(method => method.GetCustomAttributesData()
            .Where(attribute => attribute.AttributeType == typeof(McpServerToolAttribute))
            .Select(attribute => DeclaredTool.From(method, attribute)))
        .ToList();
}

// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Configuration;

/// <summary>
/// The exception that is thrown when an MCP setting cannot be bound safely.
/// </summary>
/// <param name="path">The setting path, without its value.</param>
public class InvalidMcpConfiguration(string path) : Exception($"Invalid setting: {path}");

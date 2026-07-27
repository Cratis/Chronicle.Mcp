// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Tools.Design;

/// <summary>
/// A single recorded cause for an event.
/// </summary>
/// <param name="Type">The kind of cause (e.g. a command or an upstream event).</param>
/// <param name="Occurred">When the cause occurred.</param>
/// <param name="Properties">The details of the cause.</param>
public record CausationEntry(string Type, DateTimeOffset? Occurred, IReadOnlyDictionary<string, string> Properties);

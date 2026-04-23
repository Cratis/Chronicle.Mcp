// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Api;

/// <summary>
/// Represents a causation entry for an event.
/// </summary>
/// <param name="Occurred">When the causation occurred.</param>
/// <param name="Type">The type of causation.</param>
/// <param name="Properties">The properties associated with the causation.</param>
public record Causation(DateTimeOffset Occurred, string Type, IDictionary<string, string> Properties);

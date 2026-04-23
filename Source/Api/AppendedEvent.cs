// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Api;

/// <summary>
/// Represents an event that has been appended to an event sequence.
/// </summary>
/// <param name="Context">The context for the event.</param>
/// <param name="Content">The JSON representation content of the event.</param>
public record AppendedEvent(EventContext Context, string Content);

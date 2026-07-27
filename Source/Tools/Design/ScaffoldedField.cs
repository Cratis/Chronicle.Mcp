// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Tools.Design;

/// <summary>
/// A single field chosen for a scaffolded read model, grounded in an event's real schema.
/// </summary>
/// <param name="Name">The read model property name.</param>
/// <param name="ClrType">The suggested C# type.</param>
/// <param name="SourceEventType">The event type the field was taken from.</param>
public record ScaffoldedField(string Name, string ClrType, string SourceEventType);

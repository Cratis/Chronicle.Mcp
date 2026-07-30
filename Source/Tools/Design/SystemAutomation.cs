// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Tools.Design;

/// <summary>
/// An automation in the system — a reactor or external observer that acts when events occur.
/// Automations reveal what the system does on its own, without a user asking.
/// </summary>
/// <param name="Identifier">The observer identifier.</param>
/// <param name="Type">The observer type (Reactor, External).</param>
/// <param name="EventTypes">The event types that trigger the automation.</param>
public record SystemAutomation(
    string Identifier,
    string Type,
    IReadOnlyList<string> EventTypes);

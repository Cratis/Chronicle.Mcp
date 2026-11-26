// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Screenplay.Observations;

namespace Cratis.Chronicle.Mcp.Tools.Design.Conformance;

/// <summary>
/// The exact sequence target and the position and time bounds for an audit.
/// </summary>
/// <param name="EventStore">The exact event store.</param>
/// <param name="Namespace">The exact namespace.</param>
/// <param name="Sequence">The exact sequence identifier.</param>
/// <param name="Positions">The inclusive sequence positions.</param>
/// <param name="Window">The optional half-open occurrence window.</param>
internal sealed record ConformanceScope(string EventStore, string Namespace, string Sequence, ObservedRange Positions, (DateTimeOffset? From, DateTimeOffset? To) Window)
{
    /// <summary>
    /// Gets a value indicating whether the scope satisfies the audit's input limits.
    /// </summary>
    internal bool IsValid => ValidTarget() && Positions.From >= 0 && Positions.To >= Positions.From && ValidWindow();

    /// <summary>
    /// Checks that an identifier is nonempty, bounded, and free of control characters.
    /// </summary>
    /// <param name="value">The identifier to check.</param>
    /// <returns>Whether the identifier is admissible.</returns>
    internal static bool Identifier(string? value) => !string.IsNullOrWhiteSpace(value) && value.Length <= 256 && !value.Any(char.IsControl);

    /// <summary>
    /// Determines if an append falls in the requested occurrence window.
    /// </summary>
    /// <param name="occurred">The append's occurrence time.</param>
    /// <returns>Whether the append counts as examined evidence.</returns>
    internal bool Includes(DateTimeOffset occurred) => Window.From is null || (occurred >= Window.From && occurred < Window.To);

    bool ValidTarget() => Identifier(EventStore) && Identifier(Namespace) && Identifier(Sequence);

    bool ValidWindow() => Window switch
    {
        (null, null) => true,
        ({ Offset.Ticks: 0 } from, { Offset.Ticks: 0 } to) => from < to && to - from <= TimeSpan.FromHours(24),
        _ => false
    };
}

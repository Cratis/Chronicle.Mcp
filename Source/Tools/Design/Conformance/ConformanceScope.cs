// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Screenplay.Observations;

namespace Cratis.Chronicle.Mcp.Tools.Design.Conformance;

internal sealed record ConformanceScope(string EventStore, string Namespace, string Sequence, ObservedRange Positions, (DateTimeOffset? From, DateTimeOffset? To) Window)
{
    internal long From => Positions.From;
    internal long To => Positions.To;
    internal DateTimeOffset? OccurredFrom => Window.From;
    internal DateTimeOffset? OccurredTo => Window.To;
    internal bool IsValid => ValidTarget() && From >= 0 && To >= From && ValidWindow();

    internal static bool Identifier(string? value) => !string.IsNullOrWhiteSpace(value) && value.Length <= 256 && !value.Any(char.IsControl);

    internal bool Includes(DateTimeOffset occurred) => OccurredFrom is null || (occurred >= OccurredFrom && occurred < OccurredTo);

    bool ValidTarget() => Identifier(EventStore) && Identifier(Namespace) && Identifier(Sequence);

    bool ValidWindow() => Window switch
    {
        (null, null) => true,
        ({ Offset: { Ticks: 0 } } from, { Offset: { Ticks: 0 } } to) => from < to && to - from <= TimeSpan.FromHours(24),
        _ => false
    };
}

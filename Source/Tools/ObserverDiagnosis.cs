// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Tools;

/// <summary>
/// A scoped, non-atomic view of what Chronicle reported for one observer.
/// </summary>
/// <param name="EventStore">The selected event store (display only).</param>
/// <param name="Namespace">The selected namespace (display only).</param>
/// <param name="ObserverId">The selected observer identifier (display only).</param>
/// <param name="EventSequenceId">The selected sequence identifier (display only).</param>
public sealed record ObserverDiagnosis(string EventStore, string Namespace, string ObserverId, string EventSequenceId)
{
    /// <summary>Gets the overall diagnosis outcome.</summary>
    public string Status { get; init; } = "partial";

    /// <summary>Gets whether the observer read succeeded.</summary>
    public string ObserverStatus { get; init; } = "not_queried";

    /// <summary>Gets a fixed reason for incomplete observer evidence.</summary>
    public string? ObserverReason { get; init; }

    /// <summary>Gets the projected facts for the uniquely identified observer, if any.</summary>
    public ObserverPositionEvidence? Observer { get; init; }

    /// <summary>Gets the outcome of the observer recheck.</summary>
    public string Freshness { get; init; } = "not_queried";

    /// <summary>Gets whether the independent tail lookup succeeded.</summary>
    public string TailStatus { get; init; } = "not_queried";

    /// <summary>Gets a fixed reason for an absent tail.</summary>
    public string? TailReason { get; init; }

    /// <summary>Gets the independently read sequence tail position, if actual.</summary>
    public ulong? SequenceTail { get; init; }

    /// <summary>Gets the positional comparison, or unknown without two actual positions.</summary>
    public string PositionRelativeToTail { get; init; } = "unknown";

    /// <summary>Gets the progress classification, always unknown without sampled progress.</summary>
    public string Progress => "unknown";

    /// <summary>Gets the reason no progress classification can be made.</summary>
    public string ProgressReason => "sampled_progress_not_available";

    /// <summary>Gets the fixed window of unresolved failure evidence.</summary>
    public ObserverFailureWindow<ObserverFailureEvidence> Failures { get; init; } = ObserverDiagnosisFormatting.EmptyFailures("not_queried");

    /// <summary>Gets the capability status for linking jobs to the observer.</summary>
    public ObserverFailureWindow<string> Jobs => new("unknown", "no_typed_observer_target", 0, [], false);

    /// <summary>Gets the capability status for linking recommendations to the observer.</summary>
    public ObserverFailureWindow<string> Recommendations => Jobs;

    /// <summary>Gets a fixed investigation suggestion, not a recovery action.</summary>
    public string? InvestigationHint { get; init; }

    /// <summary>Gets whether any displayed identifier was shortened.</summary>
    public bool IdentifiersTruncated { get; init; }
}

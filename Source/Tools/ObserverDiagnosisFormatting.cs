// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.RegularExpressions;
using Cratis.Chronicle.Contracts.Observation;
using static Cratis.Chronicle.Mcp.Tools.FailedPartitionExplanationFormatting;

namespace Cratis.Chronicle.Mcp.Tools;

/// <summary>
/// Projects observer facts and bounded failure evidence without inferring progress.
/// </summary>
internal static partial class ObserverDiagnosisFormatting
{
    const int FailureLimit = 5;
    const ulong FirstNonPosition = ulong.MaxValue - 2;

    [GeneratedRegex(
        """\b(?:authorization["']?\s*[:=]|bearer\s+|(?:password|token|access_token|refresh_token)["']?\s*[:=])|-----BEGIN [A-Z0-9 ]*PRIVATE KEY-----""",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.NonBacktracking)]
    private static partial Regex CredentialMarker { get; }

    /// <summary>Creates the base diagnosis for the selected scope.</summary>
    /// <param name="scope">The resolved event store and namespace.</param>
    /// <param name="observerId">The full observer identifier.</param>
    /// <param name="sequenceId">The full selected sequence identifier.</param>
    /// <returns>A diagnosis without evidence.</returns>
    internal static ObserverDiagnosis ForScope(AllObserversRequest scope, string observerId, string sequenceId) =>
        new(Display(scope.EventStore), Display(scope.Namespace), Display(observerId), Display(sequenceId))
        {
            IdentifiersTruncated = IdentifiersTooLong(scope.EventStore, scope.Namespace, observerId, sequenceId)
        };

    /// <summary>Copies the facts to be compared before any display normalization.</summary>
    /// <param name="observer">The matched metadata row.</param>
    /// <returns>A snapshot with full sequence identifier and raw positions.</returns>
    internal static ObserverPositionEvidence Snapshot(ObserverInformation observer) => new(
        observer.EventSequenceId ?? string.Empty,
        observer.Type.ToString(),
        observer.Owner.ToString(),
        observer.RunningState.ToString(),
        (DateTimeOffset?)observer.RunningStateChangedAt,
        observer.IsReplayable,
        Quarantine(observer.RunningState),
        observer.NextEventSequenceNumber,
        observer.LastHandledEventSequenceNumber,
        observer.TailEventSequenceNumber,
        observer.HandledEventCount);

    /// <summary>Adds bounded observer facts to the result.</summary>
    /// <param name="result">The diagnosis to enrich.</param>
    /// <param name="snapshot">The full observer snapshot.</param>
    /// <returns>The diagnosis with display-safe facts.</returns>
    internal static ObserverDiagnosis WithObserver(ObserverDiagnosis result, ObserverPositionEvidence snapshot) => result with
    {
        ObserverStatus = "available",
        Observer = snapshot with
        {
            EventSequenceId = Display(snapshot.EventSequenceId),
            NextEventSequenceNumber = ActualPosition(snapshot.NextEventSequenceNumber),
            LastHandledEventSequenceNumber = ActualPosition(snapshot.LastHandledEventSequenceNumber),
            TailEventSequenceNumber = ActualPosition(snapshot.TailEventSequenceNumber)
        },
        IdentifiersTruncated = result.IdentifiersTruncated || IdentifiersTooLong(snapshot.EventSequenceId)
    };

    /// <summary>Creates an empty failure window without claiming a successful lookup.</summary>
    /// <param name="status">The source status.</param>
    /// <param name="reason">The reason the window is empty, if applicable.</param>
    /// <returns>An empty failure window.</returns>
    internal static ObserverFailureWindow<ObserverFailureEvidence> EmptyFailures(string status, string? reason = null) =>
        new(status, reason, FailureLimit, [], false);

    /// <summary>Projects the fixed window of unresolved partitions for the exact observer.</summary>
    /// <param name="result">The diagnosis to enrich.</param>
    /// <param name="failures">The returned partition rows.</param>
    /// <param name="observerId">The full observer identifier.</param>
    /// <returns>The diagnosis with bounded evidence.</returns>
    internal static ObserverDiagnosis WithFailures(ObserverDiagnosis result, IEnumerable<FailedPartition> failures, string observerId)
    {
        var window = failures.Where(failure => !failure.IsResolved && string.Equals(failure.ObserverId, observerId, StringComparison.Ordinal))
            .Take(FailureLimit + 1).ToArray();
        var retained = window.Take(FailureLimit).ToArray();
        return result with
        {
            Failures = new("available", null, FailureLimit, retained.Select(DescribeFailure).ToArray(), window.Length > FailureLimit),
            IdentifiersTruncated = result.IdentifiersTruncated || retained.Any(failure => IdentifiersTooLong(failure.Partition ?? string.Empty))
        };
    }

    /// <summary>Adds a successful scalar tail read and compares actual positions only.</summary>
    /// <param name="result">The diagnosis with observer evidence.</param>
    /// <param name="tail">The reported sequence tail.</param>
    /// <returns>The diagnosis with independent tail evidence.</returns>
    internal static ObserverDiagnosis WithTail(ObserverDiagnosis result, ulong tail)
    {
        var position = ActualPosition(tail);
        return result with
        {
            TailStatus = "available",
            TailReason = position is null ? "no_tail" : null,
            SequenceTail = position,
            PositionRelativeToTail = ComparePositions(result.Observer?.LastHandledEventSequenceNumber, position)
        };
    }

    static ObserverFailureEvidence DescribeFailure(FailedPartition failure)
    {
        var redacted = false;
        var attempt = LatestAttempt(failure);
        var evidence = attempt is null ? null : DescribeAttempt(attempt, Screen);
        return new(failure.Id, Display(failure.Partition ?? string.Empty), failure.IsQuarantined, attempt is null ? "no_attempt" : "recorded", evidence, redacted);

        string Screen(string text)
        {
            if (!CredentialMarker.IsMatch(text)) return text;
            redacted = true;
            return "[withheld]";
        }
    }

    static bool? Quarantine(ObserverRunningState state) => state switch
    {
        ObserverRunningState.Quarantined => true,
        ObserverRunningState.Active or ObserverRunningState.Suspended or ObserverRunningState.Replaying or ObserverRunningState.Disconnected => false,
        _ => null
    };

    static ulong? ActualPosition(ulong? position) => position < FirstNonPosition ? position : null;

    static string ComparePositions(ulong? handled, ulong? tail)
    {
        if (handled is null || tail is null) return "unknown";
        return handled.Value.CompareTo(tail.Value) switch
        {
            < 0 => "before_tail",
            > 0 => "after_tail",
            _ => "at_tail"
        };
    }
}

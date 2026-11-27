// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Contracts;
using Cratis.Chronicle.Contracts.Observation;
using Cratis.Chronicle.Contracts.Sequences;
using Grpc.Core;
using static Cratis.Chronicle.Mcp.Tools.ObserverDiagnosisFormatting;

namespace Cratis.Chronicle.Mcp.Tools;

/// <summary>
/// Reads one scoped observer diagnosis using only the authenticated read services.
/// </summary>
/// <param name="services">The authenticated Chronicle services.</param>
/// <param name="scope">The resolved store and namespace.</param>
/// <param name="observerId">The exact observer identifier.</param>
/// <param name="sequenceId">The expected sequence identifier.</param>
/// <param name="cancellationToken">Cancellation for the reads.</param>
internal sealed class ObserverDiagnosisReader(IServices services, AllObserversRequest scope, string observerId, string sequenceId, CancellationToken cancellationToken)
{
    /// <summary>Reads membership, independent evidence, and one membership recheck.</summary>
    /// <returns>The evidence and its freshness status.</returns>
    internal async Task<ObserverDiagnosis> Read()
    {
        var diagnosis = ForScope(scope, observerId, sequenceId);
        var matches = await ReadMatches();
        if (matches is not [var match]) return WithoutUniqueObserver(diagnosis, matches);

        var first = WithObserver(diagnosis, match);
        if (string.IsNullOrWhiteSpace(match.EventSequenceId))
            return first with { ObserverReason = "observer_sequence_missing", InvestigationHint = "Check the observer's sequence metadata." };
        if (!string.Equals(match.EventSequenceId, sequenceId, StringComparison.Ordinal))
            return first with { Status = "sequence_mismatch", InvestigationHint = "Check the selected sequence against the observer's reported sequence." };

        var evidence = await ReadTail(await ReadFailures(first));
        return Recheck(diagnosis, evidence, match, await ReadMatches());
    }

    static ObserverDiagnosis WithoutUniqueObserver(ObserverDiagnosis result, ObserverPositionEvidence[]? matches) => matches switch
    {
        null => result with { ObserverStatus = "unavailable", ObserverReason = "rpc_failed", InvestigationHint = "Check the observer metadata read." },
        [] => result with { Status = "not_found", ObserverStatus = "available", InvestigationHint = "Check the selected store, namespace, and exact observer ID." },
        _ => result with { Status = "ambiguous_observer", ObserverStatus = "available", ObserverReason = "multiple_matches", InvestigationHint = "Check the observer definitions in this scope." }
    };

    static ObserverDiagnosis Recheck(ObserverDiagnosis diagnosis, ObserverDiagnosis evidence, ObserverPositionEvidence first, ObserverPositionEvidence[]? matches) => matches switch
    {
        null => evidence with { Status = "partial", Freshness = "unverified", ObserverReason = "recheck_rpc_failed", InvestigationHint = "Check the observer metadata recheck." },
        [] => DiscardIntermediate(diagnosis, "disappeared"),
        [var current] when current == first => Finish(evidence),
        [var current] => DiscardIntermediate(WithObserver(diagnosis, current), "changed"),
        _ => DiscardIntermediate(diagnosis with { ObserverReason = "multiple_matches" }, "changed")
    };

    static ObserverDiagnosis DiscardIntermediate(ObserverDiagnosis result, string status) => result with
    {
        Status = status,
        Freshness = status,
        ObserverStatus = "available",
        Failures = EmptyFailures("unavailable", "observer_changed"),
        TailStatus = "unavailable",
        TailReason = "observer_changed",
        InvestigationHint = "Check the observer metadata again before relying on associations."
    };

    static ObserverDiagnosis Finish(ObserverDiagnosis result) => result with
    {
        Status = result.TailStatus == "available" && result.Failures.Status == "available" ? "observed" : "partial",
        Freshness = "unchanged",
        InvestigationHint = InvestigationHint(result)
    };

    static string? InvestigationHint(ObserverDiagnosis result)
    {
        if (result.Failures.Status == "unavailable") return "Check the failed-partition read.";
        if (result.TailStatus == "unavailable") return "Check the sequence-tail read.";
        if (result.Failures.Rows.Count > 0) return "Inspect a reported failure ID with explain_failed_partition.";
        if (result.Observer?.RunningState == nameof(ObserverRunningState.Replaying)) return "Inspect the reported running state again later.";
        return null;
    }

    async Task<ObserverPositionEvidence[]?> ReadMatches()
    {
        var observers = await ReadRpc(() => services.Observers.GetObservers(scope, cancellationToken));
        return observers?.Where(observer => string.Equals(observer.Id, observerId, StringComparison.Ordinal)).Take(2).Select(Snapshot).ToArray();
    }

    async Task<ObserverDiagnosis> ReadFailures(ObserverDiagnosis result)
    {
        var request = new GetFailedPartitionsRequest { EventStore = scope.EventStore, Namespace = scope.Namespace, ObserverId = observerId };
        var failures = await ReadRpc(() => services.FailedPartitions.GetFailedPartitions(request, cancellationToken));
        return failures is null ? result with { Failures = EmptyFailures("unavailable", "rpc_failed") } : WithFailures(result, failures, observerId);
    }

    async Task<ObserverDiagnosis> ReadTail(ObserverDiagnosis result)
    {
        var request = new TailSequenceNumberRequest { EventStore = scope.EventStore, Namespace = scope.Namespace, EventSequenceId = sequenceId };
        var tail = await ReadRpc(() => services.Sequences.TailSequenceNumber(request, cancellationToken));
        if (tail is null) return result with { TailStatus = "unavailable", TailReason = "rpc_failed" };
        if (!tail.IsAuthorized) throw new RpcException(new(StatusCode.PermissionDenied, "Chronicle denied the sequence-tail read."));
        return tail.IsSuccess
            ? WithTail(result, tail.Data.SequenceNumber)
            : result with { TailStatus = "unavailable", TailReason = "query_failed" };
    }

    async Task<T?> ReadRpc<T>(Func<Task<T>> read)
        where T : class
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            return await read();
        }
        catch (RpcException exception) when (exception.StatusCode is not (StatusCode.Cancelled or StatusCode.Unauthenticated or StatusCode.PermissionDenied))
        {
            cancellationToken.ThrowIfCancellationRequested();
            return null;
        }
    }
}

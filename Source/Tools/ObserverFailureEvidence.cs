// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Tools;

/// <summary>
/// Evidence for one unresolved failed partition, as of the failure read.
/// </summary>
/// <param name="Id">The exact failed partition identifier.</param>
/// <param name="Partition">The bounded display name of the partition.</param>
/// <param name="IsQuarantined">Whether the partition is quarantined, or null if not reported.</param>
/// <param name="AttemptStatus">Whether an attempt was recorded.</param>
/// <param name="Attempt">A bounded description of the latest attempt, if any.</param>
/// <param name="DiagnosticsRedacted">Whether retained text was withheld by the credential screen.</param>
public sealed record ObserverFailureEvidence(
    Guid Id,
    string Partition,
    bool? IsQuarantined,
    string AttemptStatus,
    FailedPartitionAttemptExplanation? Attempt,
    bool DiagnosticsRedacted);

---
title: Diagnose an observer
description: Inspect one observer's reported running state, sequence positions, and unresolved failed partitions without starting recovery.
---

Use `diagnose_observer` to inspect one observer in the ReadOnly profile. Find the exact observer ID with `list_observers`, then select the store, namespace, and sequence you want to inspect. The tool never starts a replay, retries a partition, changes quarantine, or runs a recommendation.

| Input | Default |
| ----- | ------- |
| `observerId` | Required; exact, case-sensitive ID. |
| `eventStore` | Configured event store. |
| `namespace` | Configured namespace. |
| `eventSequenceId` | `event-log`; must match the observer's reported sequence exactly. |

Example arguments:

```json
{"observerId":"sample-observer","eventStore":"SampleStore","namespace":"SampleTenant"}
```

Example excerpt from the returned diagnosis:

```json
{
  "status": "observed",
  "freshness": "unchanged",
  "observer": {"runningState": "Active", "lastHandledEventSequenceNumber": 0, "nextEventSequenceNumber": 1, "handledEventCount": 42},
  "sequenceTail": 0,
  "positionRelativeToTail": "at_tail",
  "progress": "unknown",
  "progressReason": "sampled_progress_not_available"
}
```

## Interpret the evidence

- **Running state** comes from observer metadata. `Replaying` is a state; `isReplayable` is a separate reported capability. Observer quarantine follows the running state, not the presence of failures. A partition's own `isQuarantined` can be `null` when not reported.
- **Position** compares the last handled position with the independent, unfiltered `sequenceTail`. This is separate from the observer's reported `tailEventSequenceNumber`. The relation is `before_tail`, `at_tail`, `after_tail`, or `unknown` when a position is absent. Zero is a real position. An available tail read with reason `no_tail` means no actual tail was reported; an unavailable read is not evidence that the sequence is empty. `handledEventCount` comes from the observer, not position arithmetic.
- **Progress** is always `unknown`: these reads do not measure the rate of handling, whether matching work is pending, or the difference between a stall and healthy idleness. Active at the tail is consistent with idleness, not proof of health.
- **Associations** with jobs and recommendations are `unknown` (`no_typed_observer_target`). The tool does not read those lists or infer a relationship from text. Empty rows with limit zero mean no associations were verified, not that none exist.

For an unresolved failure, pass its exact GUID and the verified scope and sequence to [`explain_failed_partition`](failed-partitions.md) for deeper read-only investigation. Any hint is investigative text, not approval or instructions to execute recovery.

## Check status and freshness

`observed` means the supported sources were read and the observer facts matched at recheck. `not_found` means no exact match in the chosen scope; `ambiguous_observer` means multiple matches. `sequence_mismatch` shows the selected and reported sequence without switching sequences. These outcomes do not fetch failure or tail evidence. Blank observer ID or sequence input is rejected before any read.

`partial` identifies missing metadata or a failed source through a fixed reason, without echoing upstream exception text. A failed recheck leaves the earlier evidence marked `unverified`; it is not disappearance. `changed` and `disappeared` discard intervening failure and tail evidence. If a unique observer still exists, `changed` shows its final facts. No outcome is an atomic snapshot: failure rows were unresolved when read, and even an unchanged recheck cannot rule out intervening or later changes.

## Output limits and sensitive text

The tool returns at most five eligible unresolved failures in server order and flags additional eligible rows with `truncated`. Each row retains only the latest attempt (last in server order on tied times), or reports `no_attempt`. The attempt position is the recorded value, not a verified event lookup. At most ten messages of 2,048 UTF-16 units and a stack of 8,192 units are retained. Truncation flags reflect source sizes. Display IDs cap at 512 units with `identifiersTruncated`; the tool queries using full IDs and returns exact failure GUIDs.

Recognizable credential markers cause the entire retained message or stack to be replaced with `[withheld]` before clipping (`diagnosticsRedacted`). Other application text can still be sensitive. The `untrustedData.content` envelope labels store-controlled content as data; it does not scrub it. No event content is read.

A complete diagnosis uses two uncapped scoped observer list reads, one uncapped observer-filtered failure list read (including all attempts and messages), and one scalar tail read. It performs no job or recommendation reads. The caps apply after transfer: they do not cap response bytes, storage work, or transport memory.

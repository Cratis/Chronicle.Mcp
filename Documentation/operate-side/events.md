# Events

These tools read from event sequences — the append-only logs at the heart of Chronicle.

## Tools

| Tool | Description |
| ---- | ----------- |
| `get_events` | Retrieve events from an event sequence, with optional filtering by sequence range, event source id, and event type. Returns event headers and JSON content. |
| `get_tail_sequence_number` | Return the highest used sequence number (the tail) in an event sequence. |

## `get_events`

| Parameter | Description |
| --------- | ----------- |
| `eventSequenceId` | The sequence to read from. Defaults to `event-log`. |
| `from` / `to` | The sequence-number range. `from` defaults to `0`; `to` is optional. |
| `eventSourceId` | An optional event source id to filter by. |
| `eventType` | An optional comma-separated event type filter (for example `UserRegistered` or `UserRegistered+1`). |
| `eventStore` / `namespace` | Default to the configured values. |

Use it to read what actually happened — for a whole sequence, a range, or a single event source.

## `get_tail_sequence_number`

The tail is the highest used sequence number, not a total count of events — gaps can exist. Use it to gauge how far a sequence has progressed, for example when checking whether an observer has caught up.

## Narrating history

`get_events` returns raw events. To turn one event source's history into a "what happened and why" narrative — following correlation and causation — use the design-time [causal trace](../design-time/causal-trace.md) tool instead.

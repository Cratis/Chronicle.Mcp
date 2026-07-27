# Causal trace

`explain_causal_trace` reads the ordered event history of one event source together with each event's correlation id and causation chain, so an agent can narrate *what happened and why* instead of a support engineer reading raw JSON logs. It is the natural-language equivalent of `git blame` for state.

## When to use it

- To answer a question like *"Why does this order show as cancelled?"* for support or operations staff.
- To reconstruct the sequence of facts that produced an entity's current state.
- To relate an event to the wider flow that produced it, by following its correlation id.

## Parameters

| Parameter | Required | Description |
| --------- | -------- | ----------- |
| `eventSourceId` | yes | The event source id to trace (for example the order id). |
| `eventSequenceId` | no | The event sequence to read from. Defaults to `event-log`. |
| `eventType` | no | A comma-separated event type filter. |
| `eventStore` | no | The event store. Defaults to the configured event store. |
| `namespace` | no | The namespace. Defaults to the configured namespace. |

## What it returns

The event source's events in sequence order. Each event carries:

- Its sequence number, event type, generation, and when it occurred.
- Its **correlation id** — the key that groups it with the wider flow it belongs to.
- The identity that **caused** it, when known.
- Its **causation** chain — the recorded causes, each with a type and details.
- The event payload as JSON.

## Example

**Prompt:** *"Why does order 8a3f… show as cancelled?"*

The agent calls `explain_causal_trace` with the order's id and walks the returned timeline: `OrderPlaced`, then `PaymentFailed` sharing a correlation id with a payment retry flow, then `OrderCancelled` caused by that failure. From the ordered facts and their causation, the agent produces a plain-language narrative — "the order was cancelled because payment failed twice" — without anyone reading the raw log.

To widen the picture beyond a single entity, follow the correlation ids: events that share a correlation id belong to the same business flow, even across event sources.

## Related

- [Get events](../operate-side/events.md) — read raw events from a sequence with filtering.

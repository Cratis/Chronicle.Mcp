# Unconsumed-event audit

`audit_unconsumed_event_types` cross-references every registered event type against everything that consumes it — projections, reducers, and reactors — and reports two kinds of silent data debt:

- **Unconsumed event types** — data being written that no read model or reactor reads.
- **Dangling references** — the inverse: a consumer that references an event type id which no longer exists in the registry, typically a dead projection left behind after a refactor.

It needs no natural-language parsing and is cheap to run, which makes it a good periodic health check rather than only an ad-hoc query.

## When to use it

- As a recurring audit to catch events recorded "just in case" that never became a read model.
- After a refactor, to find projections or reducers still pointing at event types that have been removed.

## Parameters

| Parameter | Required | Description |
| --------- | -------- | ----------- |
| `eventStore` | no | The event store. Defaults to the configured event store. |
| `namespace` | no | The namespace to resolve observers in. Defaults to the configured namespace. |

## What it returns

- The number of event types considered.
- **Unconsumed** event types, each with its id, generation, owner, and whether it is a tombstone. The owner tells you whether it is a `Client` type a developer usually cares about or a framework-internal `Chronicle` type.
- **Dangling references**, each naming the missing event type id and the consumer (its type and id) that still points at it.

## How consumption is determined

The audit is grounded entirely in the store's registered definitions:

- **Projections** contribute the event types they read from — the keys of their `From`, `Join`, and removal definitions, walked into nested and child projections.
- **Observers** (reactors, reducers, and projections) contribute the event types each observes.

An event type is *unconsumed* when nothing in either set references it. A reference is *dangling* when it names an event type id that is not in the registry.

## Example

**Prompt:** *"What events are we writing that nothing reads?"*

The agent calls `audit_unconsumed_event_types` and reports back, for example, that `CartAbandoned` is written but read by no projection or reactor — a candidate for either a new read model or removal — and that a `LegacyOrderView` projection still references a `PriceQuoted` event type that no longer exists.

## Related

- [Event catalog](event-catalog.md) — the full picture of every event and its consumers, including the orphans this audit isolates.
- [Read-model scaffolding](read-model-scaffolding.md) — turn an unconsumed event into a read model.

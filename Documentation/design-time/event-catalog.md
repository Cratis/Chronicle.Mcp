# Event catalog

`generate_event_catalog` produces a living data dictionary from the event store: every event type, its fields, and every projection, reducer, and reactor that consumes it. Event names are the ubiquitous language of an event-sourced system, and this keeps that language documented and current straight from the store — useful onboarding material for new developers and grounding for any modeling question.

## When to use it

- To onboard someone to a domain without hand-maintained documentation.
- As grounding before a modeling conversation — the agent reads the catalog to know what already exists.
- To regenerate a data dictionary on demand instead of maintaining one by hand.

## Parameters

| Parameter | Required | Description |
| --------- | -------- | ----------- |
| `eventStore` | no | The event store. Defaults to the configured event store. |
| `namespace` | no | The namespace to resolve observers in. Defaults to the configured namespace. |
| `eventType` | no | A single event type id to scope the catalog to. Omit for the whole store. |

## What it returns

One entry per event type, each with:

- **Identity** — id, generation, owner, source, whether it is a tombstone, and the originating store for foreign types.
- **Fields** — every property with its JSON and suggested C# type, the same detail as [describe an event type](describe-event-type.md).
- **Consumers** — the projections, reducers, and reactors that read the event, each with its type, identifier, and the read model it builds (when it builds one).

An event type with an empty consumer list is an orphan — the same signal the [unconsumed-event audit](unconsumed-event-audit.md) isolates.

## Example

**Prompt:** *"Give me a catalog of our order events and who reads them."*

Scoped with `eventType` or filtered by the agent, the catalog shows, for each order event, its fields and its consumers — for example that `OrderPlaced` is read by the `OrderSummary` projection and the `ShippingNotifier` reactor, while `OrderCancelled` is read only by `OrderSummary`. From there the agent can explain the read side of the domain, or spot a gap.

## Related

- [Describe an event type](describe-event-type.md) — the same field detail for a single type.
- [Unconsumed-event audit](unconsumed-event-audit.md) — a focused view of the orphans in the catalog.

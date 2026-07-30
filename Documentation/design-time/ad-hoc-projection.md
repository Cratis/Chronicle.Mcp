# Ad-hoc projection

*"Show all employees with all details."* In an event-sourced system, answering that means folding events into current state — and if no projection happens to exist for exactly that question, the answer traditionally requires writing one, deploying it, and waiting for it to catch up. `run_ad_hoc_projection` removes that loop: it materializes the answer on demand, straight from the store, without registering or persisting anything.

Given the event types involved, the tool reads the matching events from the event sequence and folds them into per-event-source instances with the same semantics as an AutoMap projection: properties merge by name, later events overwrite earlier values, and removal events drop the instance. The result is the current state of every instance — computed in the moment, discarded after.

## The natural-language flow

The assistant resolves the question to event types; the tool does the folding:

1. **Understand the system** — [describe the system](describe-system.md) or the [event catalog](event-catalog.md) reveals the entities and which event types carry which facts.
2. **Pick the event types** — for *"all employees with all details"* that is every event contributing employee state: `EmployeeHired,EmployeeMoved,EmployeePromoted`, with `EmployeeTerminated` as a removal so former employees drop out.
3. **Run the projection** — the tool folds the events and returns the instances.
4. **Present the answer** — instances share a shape, so a table usually reads best.

The `query_system` prompt packages exactly this flow: give it the natural-language question and it walks the assistant through resolving the event types and materializing the answer.

## Parameters

| Parameter | Required | Description |
| --------- | -------- | ----------- |
| `eventTypes` | yes | Comma-separated event type ids to fold. |
| `removedWith` | no | Comma-separated event type ids that remove an instance. |
| `eventStore` | no | The event store. Defaults to the configured event store. |
| `namespace` | no | The namespace. Defaults to the configured namespace. |
| `eventSequenceId` | no | The event sequence to read from. Defaults to `event-log`. |
| `eventSourceId` | no | Scope to a single instance. |
| `limit` | no | Maximum instances returned. Defaults to 100. |

## What it returns

- **Instances** — one per event source, each with its folded properties, the sequence number of the last event applied, and when it occurred.
- **Counts** — how many events were folded and how many instances materialized (before the limit).
- **Guidance** — how to present and refine the result.

## From ad-hoc to permanent

An ad-hoc projection is a probe. When the same question keeps coming back, that is the signal it deserves a real read model — hand the same event types to [read-model scaffolding](read-model-scaffolding.md) and turn the probe into a reviewable projection in your codebase.

## Honest limits

- The folding is AutoMap-shaped: top-level properties merge by name, last write wins. Joins, children collections, and computed values are projection-engine territory — reach for a real projection when you need them.
- Every matching event is read from the sequence on each call. That is exactly right for design-time exploration and operational spot checks; it is not a serving layer for production queries.

## Related

- [Describe the system](describe-system.md) — find out which event types exist before folding them.
- [Event catalog](event-catalog.md) — the field-level detail per event type.
- [Read-model scaffolding](read-model-scaffolding.md) — make a recurring ad-hoc projection permanent.

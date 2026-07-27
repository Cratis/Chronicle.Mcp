# Observer health

Observers — projections, reducers, and reactors — consume event sequences to build read models and trigger side effects. These tools let an agent check their health, find failures, and surface maintenance the store recommends.

## Tools

| Tool | Description |
| ---- | ----------- |
| `list_observers` | List observers in a namespace, with health and replay status. Optionally filter by type (reactor, reducer, projection, or all). |
| `get_observer` | Show detailed information about a specific observer — its type, running state, owner, handled sequence numbers, and observed event types. |
| `list_failed_partitions` | List observer partitions that have failed and are paused. |
| `list_recommendations` | List active maintenance recommendations the store has raised. |

## A troubleshooting flow

When an observer is stuck or a read model looks stale:

1. `list_observers` — check the observer's running state and how far its sequence numbers have progressed.
2. `get_observer` — inspect the observer in detail, including which event types it observes.
3. `list_failed_partitions` — see whether a partition has failed and paused the observer.
4. `list_recommendations` — see whether the store already recommends a fix.

A **quarantined** observer does not auto-resume; an operator has to clear the quarantine. Use the [Cratis CLI](https://github.com/Cratis/cli) or the management API to replay, retry, or clear quarantine once you have diagnosed the cause here.

## Finding the consumers of an event type

`get_observer` shows the event types a single observer consumes. For the reverse — every consumer of a given event type across the store, plus its fields — use the design-time [event catalog](../design-time/event-catalog.md).

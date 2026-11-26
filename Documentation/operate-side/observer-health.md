---
title: Observer health
description: Use Chronicle.Mcp tools to check the health of projections, reducers and reactors, find failed partitions and list store recommendations.
---

# Observer health

A stale read model is a reason to inspect its observer before changing the projection. Observers — projections, reducers, and reactors — consume event sequences to build read models and trigger side effects. Chronicle MCP exposes their reported running state, sequence positions, and failures to help you investigate. The tools on this page only inspect; they do not replay an observer, retry a partition, or clear quarantine, even under the `Mutation` profile.

## A troubleshooting flow

First confirm the event store and namespace. If you omit them, the tools use your [configured defaults](../configuration.md); a healthy observer in another namespace does not answer the question about this one.

1. Use `list_observers` to find the observer and check its running state, replay status, and handled sequence numbers. Filter by observer type when you already know whether you are investigating a projection, reducer, or reactor.
2. Call [`diagnose_observer`](diagnose-observer.md) to compare reported positions and inspect unresolved failures in the same sequence and scope. Use `get_observer` if you also need to check the event types the observer consumes.
3. Use `list_failed_partitions` to look for failed, paused partitions. You can filter by observer id to keep the investigation focused. Pass the failure id to [`explain_failed_partition`](failed-partitions.md) to inspect the latest attempt, the exact failed event, and a bounded window of preceding events.
4. Read `list_recommendations` for maintenance the server has already identified, such as rerunning a projection or handling a schema migration. Listing a recommendation does not carry it out.

A **quarantined** observer does not auto-resume; an operator has to clear the quarantine. Once you have diagnosed the cause, use the [Cratis CLI](https://github.com/Cratis/cli) or the management API to replay, retry, or clear quarantine. MCP's job-control tools are not observer-recovery tools.

## Tools

| Tool | Description |
| ---- | ----------- |
| `list_observers` | List observers in a namespace, with health and replay status. Optionally filter by type (reactor, reducer, projection, or all). |
| `get_observer` | Show a specific observer's type, running state, owner, handled sequence numbers, and observed event types. |
| `diagnose_observer` | Inspect one observer's reported state, positions, and unresolved failures without attempting recovery. |
| `list_failed_partitions` | List observer partitions that have failed and are paused, including failure details. |
| `explain_failed_partition` | Inspect one failure, its exact event, and bounded preceding sequence history without taking recovery action. |
| `list_recommendations` | List active maintenance recommendations the store has raised. |

## Finding the consumers of an event type

`get_observer` answers which event types one observer consumes. If your investigation starts with an event type instead, use the design-time [event catalog](../design-time/event-catalog.md) to find every consumer across the store, together with the event type's fields.

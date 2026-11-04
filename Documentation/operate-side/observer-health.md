---
title: Observer health
description: Use Chronicle.Mcp tools to check the health of projections, reducers and reactors, find failed partitions and list store recommendations.
---

# Observer health

A stale read model is a reason to inspect its observer before changing the projection. Observers — projections, reducers, and reactors — consume event sequences to build read models and trigger side effects. Chronicle MCP exposes their running state, progress, and failures so you can find where processing stopped. The tools on this page only inspect; they do not replay an observer, retry a partition, or clear quarantine, even under the `Mutation` profile.

## A troubleshooting flow

First confirm the event store and namespace. If you omit them, the tools use your [configured defaults](../configuration.md); a healthy observer in another namespace does not answer the question about this one.

1. Use `list_observers` to find the observer and check its running state, replay status, and handled sequence numbers. Filter by observer type when you already know whether you are investigating a projection, reducer, or reactor. To compare its progress with the event sequence's tail, use the tools on [Events](events.md) against the same sequence and scope.
2. Call `get_observer` with the selected observer id. Check its owner and observed event types as well as its progress: this identifies whether it is the consumer you expected to handle the events in question.
3. Use `list_failed_partitions` to look for failed, paused partitions. You can filter by observer id to keep the investigation focused. The latest attempt's sequence number and messages, when present, help you locate the failing event and investigate the exception.
4. Read `list_recommendations` for maintenance the server has already identified, such as rerunning a projection or handling a schema migration. Listing a recommendation does not carry it out.

A **quarantined** observer does not auto-resume; an operator has to clear the quarantine. Once you have diagnosed the cause, use the [Cratis CLI](https://github.com/Cratis/cli) or the management API to replay, retry, or clear quarantine. MCP's job-control tools are not observer-recovery tools.

## Tools

| Tool | Description |
| ---- | ----------- |
| `list_observers` | List observers in a namespace, with health and replay status. Optionally filter by type (reactor, reducer, projection, or all). |
| `get_observer` | Show a specific observer's type, running state, owner, handled sequence numbers, and observed event types. |
| `list_failed_partitions` | List observer partitions that have failed and are paused, including failure details. |
| `list_recommendations` | List active maintenance recommendations the store has raised. |

## Finding the consumers of an event type

`get_observer` answers which event types one observer consumes. If your investigation starts with an event type instead, use the design-time [event catalog](../design-time/event-catalog.md) to find every consumer across the store, together with the event type's fields.

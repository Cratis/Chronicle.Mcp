---
title: Operating a store
description: The Chronicle.Mcp operate-side tools let an agent inspect and operate a live Chronicle system, from browsing events to watching observers and managing jobs.
---

# Operating a store

Chronicle MCP's operate-side tools let your agent inspect a running Cratis Chronicle system: browse its domain, read events, investigate observer failures, and follow background jobs. They answer the operational questions you would otherwise take to the [Cratis CLI](https://github.com/Cratis/cli). Choose a starting point below based on what you need to find out.

Inspection is available in the default `ReadOnly` profile. Only `stop_job`, `resume_job`, and `delete_job` change server-side state; they are unavailable in `ReadOnly` and require the `Mutation` profile with a matching authorization grant. Every other tool is read-only. See [Deployment profiles](../deployment-profiles.md) before enabling job control, or [Which tools change state](../concepts.md#which-tools-change-state) for the distinction.

## Capabilities

| What you need to find out | Start here |
| ------------------------ | ---------- |
| Which stores, schemas, projections, and read-model data are available | [Exploring the store](exploring-the-store.md) |
| What happened to an entity, or where an event sequence ends | [Events](events.md) |
| Why an observer or read model has stopped progressing | [Observer health](observer-health.md) |
| What background work is doing, or whether a job needs intervention | [Jobs](jobs.md) |

Each page lists its tools and shows how to use their results in the investigation.

## Things to ask

Tools that accept an event store or namespace use your [configured defaults](../configuration.md) when you omit them. Name the scope in your request when you want a different one:

- List all event stores.
- List all event types in the `orders` event store.
- Show me the events for order `8a3f…`.
- Are there any failed partitions?
- What observers use the event type `OrderPlaced`?
- List all jobs, and show me the steps of job `…`.

## Operate-side and design-time together

Exploring a store can reveal something you need to build. After inspecting the registered event types and their consumers, move to [read-model scaffolding](../design-time/read-model-scaffolding.md) to propose a model based on that schema. Design-time tools return advisory output; they do not apply changes to the store. Use operate-side tools to inspect the running system, and design-time tools to develop a proposal from what it contains. [How it works](../concepts.md) explains how the two modes share one connection.

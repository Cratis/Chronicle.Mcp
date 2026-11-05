---
title: Operating a store
description: The Chronicle.Mcp operate-side tools let an agent inspect and operate a live Chronicle system, from browsing events to watching observers and managing jobs.
---

# Operating a store

Chronicle MCP's operate-side tools let your agent inspect a running Cratis Chronicle system: browse its domain, read events, investigate observer failures, and follow background jobs. They answer the operational questions you would otherwise take to the [Cratis CLI](https://github.com/Cratis/cli). Choose a starting point below based on what you need to find out.

Inspection is available in the default `ReadOnly` profile. Only `stop_job`, `resume_job`, and `delete_job` change server-side state; they are unavailable in `ReadOnly` and require the `Mutation` profile with a matching authorization grant. Every other tool is read-only. See [Deployment profiles](../deployment-profiles.md) before enabling job control, or [Which tools change state](../concepts.md#which-tools-change-state) for the distinction.

## Capabilities

| Area | Tools | Page |
| ---- | ----- | ---- |
| Exploring the store | `list_event_stores`, `list_namespaces`, `list_event_types`, `list_projections`, `list_read_models`, `get_read_model_instances`, `get_patterns`, `get_server_version` | [Exploring the store](exploring-the-store.md) |
| Events | `get_events`, `get_tail_sequence_number` | [Events](events.md) |
| Observer health | `list_observers`, `get_observer`, `list_failed_partitions`, `list_recommendations` | [Observer health](observer-health.md) |
| Jobs | `list_jobs`, `get_job`, `get_job_steps`, `stop_job`, `resume_job`, `delete_job` | [Jobs](jobs.md) |

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

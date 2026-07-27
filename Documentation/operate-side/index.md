# Operating a store

The operate-side tools inspect and operate a live Chronicle system. They are the natural-language equivalent of running the [Cratis CLI](https://github.com/Cratis/cli) against a store: browse the domain, watch observers, read events, and manage jobs. Every tool defaults the event store and namespace to your [configured defaults](../configuration.md), so you only name a store or namespace when you want a specific one.

## Capabilities

| Area | Tools | Page |
| ---- | ----- | ---- |
| Exploring the store | `list_event_stores`, `list_namespaces`, `list_event_types`, `list_projections`, `list_read_models`, `get_read_model_instances`, `get_server_version` | [Exploring the store](exploring-the-store.md) |
| Events | `get_events`, `get_tail_sequence_number` | [Events](events.md) |
| Observer health | `list_observers`, `get_observer`, `list_failed_partitions`, `list_recommendations` | [Observer health](observer-health.md) |
| Jobs | `list_jobs`, `get_job`, `get_job_steps`, `stop_job`, `resume_job`, `delete_job` | [Jobs](jobs.md) |

## Things to ask

- List all event stores.
- List all event types in the `orders` event store.
- Show me the events for order `8a3f…`.
- Are there any failed partitions?
- What observers use the event type `OrderPlaced`?
- List all jobs, and show me the steps of job `…`.

## Operate-side and design-time together

Operate-side tools discover *what is there*; design-time tools turn that into *artifacts*. A common pattern is to explore event types here, then hand them to [read-model scaffolding](../design-time/read-model-scaffolding.md). See [How it works](../concepts.md) for the full split.

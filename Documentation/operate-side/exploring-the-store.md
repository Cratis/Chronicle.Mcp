# Exploring the store

These tools let an agent walk a Chronicle store's structure — its stores, namespaces, event types, projections, read models, and current instances — to build up a picture of the domain before doing anything else.

## Tools

| Tool | Description |
| ---- | ----------- |
| `get_server_version` | Get version info from the server. Doubles as a connectivity check. |
| `list_event_stores` | List all event stores on the server. |
| `list_namespaces` | List namespaces within an event store. |
| `list_event_types` | List registered event types, with id, generation, owner, and source. |
| `list_projections` | List projection definitions, including the read model each projects into and its activity state. |
| `list_read_models` | List read model definitions. |
| `get_read_model_instances` | List the current instances of a read model, paged. |

## Typical flow

Start broad and narrow down:

1. `get_server_version` — confirm the server is reachable.
2. `list_event_stores` then `list_namespaces` — find the store and tenant you care about.
3. `list_event_types` — see the domain's vocabulary.
4. `list_projections` and `list_read_models` — see what is already derived from those events.
5. `get_read_model_instances` — read the current state a projection has produced.

## Going deeper on an event type

`list_event_types` gives you the inventory. To see an event type's actual fields and types, use the design-time [describe an event type](../design-time/describe-event-type.md) tool, or [generate an event catalog](../design-time/event-catalog.md) for the whole store at once.

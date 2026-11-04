---
title: Exploring the store
description: Use Chronicle.Mcp tools to list a store's event stores, namespaces, event types, projections and read models, and read their current instances.
---

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
| `get_patterns` | Get learned behavior patterns matching a grouping key and context, in server order. |

## Typical flow

Start broad and narrow down:

1. `get_server_version` — confirm the server is reachable.
2. `list_event_stores` then `list_namespaces` — find the store and tenant you care about.
3. `list_event_types` — see the domain's vocabulary.
4. `list_projections` and `list_read_models` — see what is already derived from those events.
5. `get_read_model_instances` — read the current state a projection has produced.

## Querying learned behavior patterns

`get_patterns` is read-only. Supply the `groupingKey` used when learning the patterns and a `context` dictionary of facet names and values. Optional `eventStore` and `namespace` fall back to the configured defaults. Context is forwarded unchanged: canonical facet names are case-sensitive, and the server's configured vocabulary determines which facets participate, ignoring others. An action, when present, is an ordinary facet, not a separate response field.

`minimumConfidence` and `maximumResults` both default to `0`, leaving default resolution to the backend. In the pinned backend, confidence values at or below zero select the configured confidence (source default `0.5`), and result counts at or below zero select `10`. The documented confidence domain is `[0,1]`, but the tool adds no local range validation. Positive counts are forwarded unchanged without a local cap; choose a limit appropriate to your query. These source defaults do not establish the deployed server's configuration or version.

Results retain the server's authoritative order without local matching, filtering or reranking. Each result preserves `id`, `groupingKey`, `facets`, `confidence`, `support`, `occurrences` (64-bit integer), `weight`, `specificity`, `firstSeen` and `lastSeen`. Timestamps keep the contract wrapper shape, for example `"firstSeen": { "value": "2026-01-02T03:04:05.0000000+02:00" }`, rather than becoming bare timestamp strings. An empty successful result remains empty; unsuccessful queries are surfaced as failures, not empty matches.

## Going deeper on an event type

`list_event_types` gives you the inventory. To see an event type's actual fields and types, use the design-time [describe an event type](../design-time/describe-event-type.md) tool, or [generate an event catalog](../design-time/event-catalog.md) for the whole store at once.

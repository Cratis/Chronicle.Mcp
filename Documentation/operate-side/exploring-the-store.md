---
title: Exploring the store
description: Use Chronicle.Mcp tools to list a store's event stores, namespaces, event types, projections and read models, and read their current instances.
---

# Exploring the store

Start with discovery when you do not yet know which Chronicle event store holds the data you need. Chronicle MCP can list stores and namespaces, show the registered event types and their projections, then read current read-model instances. These are read-only operations.

Before exploring, check your [connection configuration](../configuration.md). Tools that take a store or namespace use your configured defaults unless you override them. Keep definitions and data separate as you explore: a registered projection identifies the read model it populates, while the model's current instances show what has actually been stored.

## Tools

| Tool | Description |
| ---- | ----------- |
| `get_server_version` | Get server version and commit information; a successful response also confirms connectivity. |
| `list_event_stores` | List all event stores on the server. |
| `list_namespaces` | List namespaces within an event store. |
| `list_event_types` | List registered event types, with id, generation, owner, and source. |
| `list_projections` | List projection definitions, including the read model each projects into and its activity state. |
| `list_read_models` | List read model definitions, including whether each model is queryable. |
| `get_read_model_instances` | Read a page of current instances from a read model. |
| `get_patterns` | Get learned behavior patterns matching a grouping key and context, in server order. |

## Typical flow

1. Ask for `get_server_version` to check that the MCP server can reach Chronicle. Resolve connection problems before investigating missing data.
2. Use `list_event_stores`, then `list_namespaces` for the chosen store. Carry the selected scope into subsequent calls.
3. Call `list_event_types` to see the domain's vocabulary. Use `list_projections` and `list_read_models` to find the existing projections and the models they populate.
4. Choose a queryable read model and pass its container name to `get_read_model_instances`. This returns a page of persisted data, not a projection definition. Ask for further pages when you need more than the first page of results.

If you already know the read model and are investigating stale data, go straight to [Observer health](observer-health.md). The observer's running state and failures are more useful than another schema inventory for that question.

## Querying learned behavior patterns

`get_patterns` is read-only. Supply the `groupingKey` used when learning the patterns and a `context` dictionary of facet names and values. Optional `eventStore` and `namespace` fall back to the configured defaults. Context is forwarded unchanged: canonical facet names are case-sensitive, and the server's configured vocabulary determines which facets participate, ignoring others. An action, when present, is an ordinary facet, not a separate response field.

`minimumConfidence` and `maximumResults` both default to `0`, leaving default resolution to the backend. In the pinned backend, confidence values at or below zero select the configured confidence (source default `0.5`), and result counts at or below zero select `10`. The documented confidence domain is `[0,1]`, but the tool adds no local range validation. Positive counts are forwarded unchanged without a local cap; choose a limit appropriate to your query. These source defaults do not establish the deployed server's configuration or version.

Results retain the server's authoritative order without local matching, filtering or reranking. Each result preserves `id`, `groupingKey`, `facets`, `confidence`, `support`, `occurrences` (64-bit integer), `weight`, `specificity`, `firstSeen` and `lastSeen`. Timestamps keep the contract wrapper shape, for example `"firstSeen": { "value": "2026-01-02T03:04:05.0000000+02:00" }`, rather than becoming bare timestamp strings. An empty successful result remains empty; unsuccessful queries are surfaced as failures, not empty matches.

## Going deeper on an event type

`list_event_types` gives you the inventory, including generation and ownership, but not an event type's fields. Use the design-time [describe an event type](../design-time/describe-event-type.md) tool for its actual fields and types, or [generate an event catalog](../design-time/event-catalog.md) for the whole store at once.

---
title: Design-time capabilities
description: The Chronicle.Mcp design-time tools turn plain-language requests into read models, projections, audits and catalogs grounded in the store's schema.
---

# Design-time capabilities

Use the design-time tools to understand an event store, review candidate event types, or answer a data question. Choose the task you have; you do not need to run every capability below. They are read-only against the store and generative against your codebase: nothing is written to the store, and generated code comes back as a proposal you review. See [How it works](../concepts.md) for the principle behind them.

If the server is not connected yet, start with [Getting started](../getting-started.md). Confirm the intended store and namespace before asking the agent to read data; read-only access is not a substitute for authorization or permission to share the results.

## Capabilities

| Capability | Tool | Use it to |
| ---------- | ---- | --------- |
| [Describe the system](describe-system.md) | `describe_system` | Deduce what the system is and is for — entities, lifecycles, read surfaces, automations — and tell its story. |
| [Suggest the next event types](suggest-next-event-types.md) | `suggest_next_event_types` | Find lifecycle gaps and get grounded suggestions for the event types to introduce next. |
| [Ad-hoc projection](ad-hoc-projection.md) | `run_ad_hoc_projection` | Answer "show all X with all details" by folding events into current state on demand — nothing registered. |
| [Describe an event type](describe-event-type.md) | `describe_event_type` | Read an event type's real fields and types — the grounding primitive the others build on. |
| [Read-model scaffolding](read-model-scaffolding.md) | `scaffold_read_model` | Turn a set of event types into a reviewable read model + projection, grounded in their schema. |
| [Unconsumed-event audit](unconsumed-event-audit.md) | `audit_unconsumed_event_types` | Find events nothing reads, and consumers pointing at event types that no longer exist. |
| [Event catalog](event-catalog.md) | `generate_event_catalog` | Produce a living data dictionary — every event, its fields, and its consumers. |
| [Causal trace](causal-trace.md) | `explain_causal_trace` | Turn an event source's raw log into a "what happened and why" narrative. |

## Prompts

Beyond tools, the server exposes MCP prompts that package whole workflows — clients that surface prompts (for example as slash commands) can offer them directly:

| Prompt | What it does |
| ------ | ------------ |
| `describe_system` | Describe what the system is and is for, and tell its story. |
| `suggest_next_event_types` | Propose the next event types, refined with domain knowledge. |
| `query_system` | Answer a natural-language question ("show all employees with all details") with an ad-hoc projection. |

## How an agent combines them

The capabilities are independent tasks, not a required sequence. When a question spans tasks, the following optional flow shows how they can fit together. To answer *"Show all employees with all details"* an agent might:

1. Understand the system with [describe the system](describe-system.md) — which entities exist and which event types carry which facts.
2. Fold the relevant event types into current state with an [ad-hoc projection](ad-hoc-projection.md) and present the result.
3. If the question keeps coming back, propose a permanent read model with [read-model scaffolding](read-model-scaffolding.md).

The tools return structured data; the agent turns it into prose or a proposal. Registered names and schemas ground that output, but they do not validate the inferred business meaning.

## Roadmap

The [capability hand-off](https://github.com/Cratis/Chronicle.Mcp) sketches ten design-time capabilities. The tools above cover the grounding-and-generation core plus system understanding, evolution suggestions, and ad-hoc querying. The remaining ideas — projection-vs-reducer advice, consistency-boundary (DCB) advice, schema-evolution assistance, spec scaffolding, and constraint suggestions — build on the same introspection pipeline and are candidates for future releases.

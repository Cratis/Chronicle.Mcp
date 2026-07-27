# Chronicle MCP Server

The Chronicle MCP server connects an AI agent to a running [Cratis Chronicle](https://github.com/Cratis/Chronicle) event store over the [Model Context Protocol](https://modelcontextprotocol.io). Point your agent at a Chronicle server and it can explore the domain, operate observers and jobs, and turn plain-language requests into Chronicle artifacts — always grounded in the store's real schema.

The server offers two complementary sets of capabilities:

| Mode | What it does | Start here |
| ---- | ------------ | ---------- |
| **Operate-side** | Inspect and operate a live system — browse events, watch observers, replay partitions, manage jobs. | [Operating a store](operate-side/index.md) |
| **Design-time** | Turn natural language into Chronicle artifacts — read models, projections, audits, catalogs — grounded in the actual event schema, never guessed. | [Design-time capabilities](design-time/index.md) |

## Get started

By the end of [Getting started](getting-started.md) you will have the server wired into your agent and answering questions about a running Chronicle store.

1. [Getting started](getting-started.md) — wire the server into your agent and ask your first question.
2. [Configuration](configuration.md) — connection string, CLI config sharing, and authentication.
3. [How it works](concepts.md) — the design-time vs. operate-side split and the grounding principle.

## Why this exists

Chronicle already exposes a rich gRPC management API, and the [Cratis CLI](https://github.com/Cratis/cli) puts it at your fingertips. This server puts the same store in reach of an AI agent — so instead of you translating a question into a sequence of API calls, the agent does it, and instead of hand-writing read-model boilerplate, the agent proposes a grounded starting point you review like any other change.

The design-time capabilities never invent field names or event shapes. Every suggestion is introspected from what the store actually has registered — see [How it works](concepts.md#grounding-over-fluency).

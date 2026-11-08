---
title: Getting started
description: Run the Chronicle.Mcp server in your MCP-capable agent, connect it to a live Chronicle event store, and ask questions about events and observers.
---

# Getting started

By the end of this page you will have the Chronicle MCP server running inside your agent and answering questions about a live Chronicle store.

## Prerequisites

- A running Chronicle server. If you do not have one, the [`docker-compose.yml` in the repository](https://github.com/Cratis/Chronicle.Mcp/blob/main/Source/docker-compose.yml) brings one up with `docker compose up -d`.
- An MCP-capable agent (for example VS Code with an agent extension, or any client that speaks the Model Context Protocol over stdio).
- Docker, to run the pre-built `cratis/chronicle-mcp` image.

Before connecting, choose the Chronicle server you intend to inspect and use credentials limited to the access needed. Keep credentials out of committed agent configuration. Reading and sharing returned data still require permission.

## Wire it into your agent

The server speaks MCP over stdio and ships as a container. This example passes a connection string but does not share host CLI configuration or token files. Configure your agent to launch it. In VS Code, add it to your user settings or to a `.vscode/mcp.json` in your project:

This configuration starts in the safe `ReadOnly` profile. You can inspect jobs, but `stop_job`, `resume_job`, and `delete_job` are not listed or callable. To enable authorized job control, configure a [Mutation deployment profile](deployment-profiles.md).

```json
{
    "servers": {
        "Chronicle": {
            "type": "stdio",
            "command": "docker",
            "args": [
                "run",
                "-i",
                "--rm",
                "-eCratis__Chronicle__Mcp__ConnectionString=chronicle://host.docker.internal:35000",
                "cratis/chronicle-mcp"
            ]
        }
    }
}
```

On macOS and Windows the host machine is reachable from the container at `host.docker.internal`. The connection string is the only value most setups need to change — everything else has a working local-development default. See [Configuration](configuration.md) for the full resolution order and authentication.

## Ask your first questions

Start by discovering which stores are available:

- "List all event stores."

Choose a store from that response, ask for its namespaces, and select the intended namespace from that result. Use the chosen store explicitly for discovery; include the namespace on tools that accept it. Then inspect that scope:

- "What event types are registered?"
- "Are there any failed partitions?"

Before continuing, check the scope actually supplied and that the inspection completed, not merely that the agent connected. Resolve connection or permission errors, and investigate an unexpected empty result, before interpreting the data.

Once the scope is confirmed, choose one of these optional questions for your task; they are alternatives, not a sequence:

- "Describe the system in this event store" → the agent infers an entity/lifecycle model with [describe the system](design-time/describe-system.md) for you to check against the domain.
- "Show all employees with all details" → an [ad-hoc projection](design-time/ad-hoc-projection.md) folds the events into current state on demand.
- "What event types should I introduce next?" → candidate gaps for domain review from [suggest the next event types](design-time/suggest-next-event-types.md).
- "Show me all the users registered" → the agent resolves the event types, then proposes a read model with [read-model scaffolding](design-time/read-model-scaffolding.md).
- "What events are we writing that nothing reads?" → an [unconsumed-event audit](design-time/unconsumed-event-audit.md).
- "Why does this order show as cancelled?" → a [causal trace](design-time/causal-trace.md) of the order's history.

## Where to go next

- [Configuration](configuration.md) — connect to a secured server, share the CLI's context, or run without Docker.
- [Deployment profiles](deployment-profiles.md) — keep read-only access or grant bounded job-control authority.
- [How it works](concepts.md) — the two modes and why every design-time suggestion is grounded in real schema.
- [Design-time capabilities](design-time/index.md) — the full set of generative and advisory tools.

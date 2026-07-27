# Getting started

By the end of this page you will have the Chronicle MCP server running inside your agent and answering questions about a live Chronicle store.

## Prerequisites

- A running Chronicle server. If you do not have one, the `docker-compose.yml` in the [`Source`](../Source/docker-compose.yml) folder brings one up with `docker compose up -d`.
- An MCP-capable agent (for example VS Code with an agent extension, or any client that speaks the Model Context Protocol over stdio).
- Docker, to run the pre-built `cratis/chronicle-mcp` image.

## Wire it into your agent

The server speaks MCP over stdio and ships as a container. Configure your agent to launch it. In VS Code, add it to your user settings or to a `.vscode/mcp.json` in your project:

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

Once the server is connected, your agent can reach the store. Start with the operate-side capabilities to confirm connectivity:

- "List all event stores."
- "What event types are registered?"
- "Are there any failed partitions?"

Then try the design-time capabilities, which turn a plain request into a grounded artifact:

- "Show me all the users registered" → the agent resolves the event types, then proposes a read model with [read-model scaffolding](design-time/read-model-scaffolding.md).
- "What events are we writing that nothing reads?" → an [unconsumed-event audit](design-time/unconsumed-event-audit.md).
- "Why does this order show as cancelled?" → a [causal trace](design-time/causal-trace.md) of the order's history.

## Where to go next

- [Configuration](configuration.md) — connect to a secured server, share the CLI's context, or run without Docker.
- [How it works](concepts.md) — the two modes and why every design-time suggestion is grounded in real schema.
- [Design-time capabilities](design-time/index.md) — the full set of generative and advisory tools.

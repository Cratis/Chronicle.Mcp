# Chronicle.MCP

[![Discord](https://img.shields.io/discord/1182595891576717413?label=Discord&logo=discord&color=7289da)](https://discord.gg/kt4AMpV8WV)
[![Docker](https://img.shields.io/docker/v/cratis/chronicle-mcp?label=Chronicle.Mcp&logo=docker&sort=semver)](https://hub.docker.com/r/cratis/chronicle-mcp)
[![Build](https://github.com/Cratis/Chronicle.Mcp/actions/workflows/build.yml/badge.svg)](https://github.com/Cratis/Chronicle.Mcp/actions/workflows/build.yml)
[![Publish](https://github.com/cratis/Chronicle.Mcp/actions/workflows/publish.yml/badge.svg)](https://github.com/Cratis/Chronicle.Mcp/actions/workflows/publish.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](./LICENSE)

The Chronicle MCP server connects an AI agent to a running [Cratis Chronicle](https://github.com/Cratis/Chronicle) event-sourcing database over the [Model Context Protocol](https://modelcontextprotocol.io). Point your agent at a Chronicle server and it can explore the domain, operate observers and jobs, and turn plain-language requests into Chronicle artifacts — always grounded in the store's real schema.

Because the server talks to Chronicle directly, it works no matter which Chronicle client language your application uses — .NET, TypeScript, Kotlin/Java, or Elixir. Any MCP-capable tool can use it. See the [Documentation](./Documentation/index.md) for the full guide.

## Using

The Chronicle MCP server leverages Stdio and is packaged as a container.
In your tool, configure it using that.

> Note: You must have a Chronicle server running.

### Example: VSCode

In VSCode you would do this by adding a tool to your agent.
This can done either by adding it to the global user settings or through an `mcp.json` file in
the `.vscode` folder of your project.

For the global user settings, you simply do the following:

```json
"mcp": {
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

> Note: To configure the connection string for Chronicle you pass it an environment variable; `Cratis__Chronicle__Mcp__ConnectionString`
> running locally - on MacOS and Windows the host machine is found at `host.docker.internal`.

For a local `mcp.json` file, its almost the same:

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

You can see this in action in the [mcp.json](./.vscode/mcp.json) in this repository.

> Note: The `cratis/chronicle-mcp` is a multi CPU architecture image supporting both x64 and arm64 automatically.

## Configuration

**Configuration is optional.** The MCP server works out of the box with sensible defaults suitable for local development:

- **Connection String:** `chronicle://host.docker.internal:35000`
- **Credentials:** Development client ID (`chronicle-dev-client`) and secret (`chronicle-dev-secret`)

If you need to customize any settings, the MCP server can be configured entirely on its own and is also compatible with the
[Cratis CLI](https://github.com/Cratis/cli). For any value you do not set explicitly, the server resolves it in this order:

1. Explicit MCP options (environment variables / `appsettings.json`).
2. The `CHRONICLE_CONNECTION_STRING` environment variable.
3. The active context in the CLI configuration at `~/.cratis/config.json`.
4. Built-in development defaults.

When client credentials are used, the server obtains and caches OAuth tokens in `~/.cratis/tokens`,
the same location used by the CLI, so tokens are shared between the two.

All options live under the `Cratis:Chronicle:Mcp` configuration section. As environment variables
they use the `Cratis__Chronicle__Mcp__` prefix:

| Option | Environment variable | Description |
| ------ | -------------------- | ----------- |
| `ConnectionString` | `Cratis__Chronicle__Mcp__ConnectionString` | The Chronicle connection string. Defaults to `chronicle://localhost:35000`. |
| `Context` | `Cratis__Chronicle__Mcp__Context` | The CLI context to read connection details from (defaults to the active context). |
| `UseCliConfiguration` | `Cratis__Chronicle__Mcp__UseCliConfiguration` | Set to `false` to ignore `~/.cratis/config.json` entirely. |
| `ClientId` / `ClientSecret` | `Cratis__Chronicle__Mcp__ClientId` / `...__ClientSecret` | Client credentials for authentication. Defaults to development credentials if not specified. |
| `ApiKey` | `Cratis__Chronicle__Mcp__ApiKey` | An API key to authenticate with, as an alternative to client credentials. |
| `EventStore` | `Cratis__Chronicle__Mcp__EventStore` | The default event store used by tools when none is specified. Defaults to `default`. |
| `Namespace` | `Cratis__Chronicle__Mcp__Namespace` | The default namespace used by tools when none is specified. Defaults to `Default`. |

## Prompts / Tools

The server exposes the following tools. Every tool defaults the event store and namespace to the
configured defaults when you do not specify them, so you can ask high-level questions and only
mention a store or namespace when you need a specific one.

The tools come in two complementary sets: **operate-side** tools for inspecting and operating a live
store, and **design-time** tools that turn natural language into Chronicle artifacts grounded in the
store's real schema. See the [Documentation](./Documentation/index.md) folder for a full guide, and
[How it works](./Documentation/concepts.md) for the split.

### Operate-side

| Tool | Description |
| ---- | ----------- |
| `list_event_stores` | List all event stores on the server. |
| `list_namespaces` | List namespaces within an event store. |
| `list_event_types` | List registered event types. |
| `list_observers` | List observers (reactors, reducers, projections), optionally filtered by type. |
| `get_observer` | Show detailed information about a specific observer. |
| `list_failed_partitions` | List observer partitions that have failed and are paused. |
| `list_projections` | List projection definitions. |
| `list_read_models` | List read model definitions. |
| `get_read_model_instances` | List the current instances of a read model (paged). |
| `get_events` | Read events from an event sequence, with optional filtering. |
| `get_tail_sequence_number` | Get the highest used sequence number (tail) in an event sequence. |
| `list_recommendations` | List active maintenance recommendations. |
| `get_server_version` | Get version info from the server (also a connectivity check). |
| `list_jobs` | List all jobs in a namespace, optionally filtered by job status. |
| `get_job` | Get a specific job by ID, including full details and status changes. |
| `get_job_steps` | Get the job steps for a specific job, optionally filtered by step status. |
| `stop_job` | Stop a specific job (transitions to Stopped status). |
| `resume_job` | Resume a specific stopped job. |
| `delete_job` | Delete a specific job (transitions to Removing status). |

### Design-time

| Tool | Description |
| ---- | ----------- |
| `describe_system` | Deduce what the system in an event store is and is for — entities, lifecycles, read surfaces, automations — with narrative guidance for telling its story. |
| `suggest_next_event_types` | Find lifecycle gaps and suggest the event types to introduce next, each grounded in the gap it closes. |
| `run_ad_hoc_projection` | Fold events into per-event-source instances on demand (AutoMap semantics) — answer "show all X with all details" without registering anything. |
| `describe_event_type` | Describe an event type's real schema — every property with its JSON and suggested C# type. The grounding primitive for the others. |
| `scaffold_read_model` | Generate a reviewable read model + model-bound projection from one or more event types, grounded in their schema. |
| `audit_unconsumed_event_types` | Report event types nothing reads, plus consumers that reference an event type id that no longer exists. |
| `generate_event_catalog` | Produce a living data dictionary — every event type, its fields, and its consumers. |
| `explain_causal_trace` | Read an event source's ordered history with correlation and causation, to narrate what happened and why. |

The server also exposes MCP **prompts** — `describe_system`, `suggest_next_event_types`, and `query_system` — that package these workflows for clients that surface prompts (for example as slash commands).

You can ask it things like:

- List all event stores
- List all event types in the [put name here] event store
- List all observers in the [put name here] event store and namespace [put namespace here]
- Show me the events in the [put name here] event store
- Are there any failed partitions?
- What observers in the [put event store name here] use event type [put event type name]
- List all jobs in the [put event store name here] event store
- Show me job [put job id here] in the [put namespace name here] namespace
- What steps does job [put job id here] have?
- Stop / Resume / Delete job [put job id here]

And design-time questions like:

- Describe the system in [put event store here] — and tell me its story
- What event types should I introduce next for the system in [put event store here]?
- Show all [put concept here] with all details (runs an ad-hoc projection)
- Show me all the [put concept here] registered (scaffolds a read model + projection)
- What fields does the [put event type here] event have?
- What events are we writing that nothing reads?
- Give me a catalog of our [put area here] events and who reads them
- Why does [put entity here] show as [put state here]?

## Local development

Using VSCode, the [mcp.json](./.vscode/mcp.json) in the `.vscode` folder of this repository is automatically supported.
Open it and click the **Start** button:

![](./images/start.png)

During development, compile and click the **Restart** button when having the `mcp.json` open:

![](./images/restart.png)

### Chronicle

To get Chronicle running, there is a `docker-compose.yml` file in the `Source` folder.
Simply do `docker compose up -d` and you'll have a Chronicle instance running.

If you want some data, we recommend using our [samples](https://github.com/cratis/samples) and
specifically the [console](https://github.com/Cratis/Samples/tree/main/Chronicle/Quickstart/Console) to initialize
it with some data.

## The Cratis ecosystem

This project is part of [Cratis](https://www.cratis.io) — free, MIT-licensed tools for building event-sourced and CQRS applications.

- **[Chronicle](https://github.com/Cratis/Chronicle)** — event-sourcing database and runtime. Orleans-based kernel, pluggable storage (MongoDB default; PostgreSQL, SQL Server, SQLite, in-memory), language-agnostic gRPC contracts. [Docs](https://www.cratis.io/chronicle/)
- **Chronicle clients** — first-class [.NET SDK](https://github.com/Cratis/Chronicle), plus [TypeScript](https://github.com/Cratis/Chronicle.TypeScript), [Kotlin/Java](https://github.com/Cratis/Chronicle.Kotlin), and [Elixir](https://github.com/Cratis/Chronicle.Elixir); [Python](https://github.com/Cratis/Chronicle.Python) coming soon (pre-alpha). AI agents connect through the Chronicle MCP server (this repository).
- **[Arc](https://github.com/Cratis/Arc)** — opinionated CQRS framework for ASP.NET Core with commands, queries, validation, authorization, and TypeScript proxy generation. Works without event sourcing. [Docs](https://www.cratis.io/arc/)
- **[Components](https://github.com/Cratis/Components)** — React components aligned with Arc patterns. [Docs](https://www.cratis.io/components/)
- **[CLI](https://github.com/Cratis/cli) + Workbench** — inspect and diagnose Chronicle from the terminal or the browser. [Docs](https://www.cratis.io/cli/)
- **Model-first layer (experimental)** — [Studio](https://github.com/Cratis/Studio), [Screenplay](https://github.com/Cratis/Screenplay), [Stage](https://github.com/Cratis/Stage), [Scene](https://github.com/Cratis/Scene), [Prologue](https://github.com/Cratis/Prologue)
- **Supporting** — [Fundamentals](https://github.com/Cratis/Fundamentals), [Specifications](https://github.com/Cratis/Specifications), [Synopsis](https://github.com/Cratis/Synopsis), [Lens](https://github.com/Cratis/Lens), [Narrator](https://github.com/Cratis/Narrator), and free [AI tooling](https://github.com/Cratis/AI) (preview); [Ensemble](https://github.com/Cratis/Ensemble) coming soon (pre-release)
- **[Samples](https://github.com/Cratis/Samples)** — runnable event sourcing and CQRS samples for the whole stack

Everything Cratis publishes today is MIT licensed and free to use.

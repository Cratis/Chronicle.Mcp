# Chronicle.MCP

[![Discord](https://img.shields.io/discord/1182595891576717413?label=Discord&logo=discord&color=7289da)](https://discord.gg/kt4AMpV8WV)
[![Docker](https://img.shields.io/docker/v/cratis/chronicle-mcp?label=Chronicle.Mcp&logo=docker&sort=semver)](https://hub.docker.com/r/cratis/chronicle-mcp)
[![Build](https://github.com/Cratis/Chronicle.Mcp/actions/workflows/build.yml/badge.svg)](https://github.com/Cratis/Chronicle.Mcp/actions/workflows/build.yml)
[![Publish](https://github.com/cratis/Chronicle.Mcp/actions/workflows/publish.yml/badge.svg)](https://github.com/Cratis/Chronicle.Mcp/actions/workflows/publish.yml)

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

The MCP server can be configured entirely on its own, and it is also compatible with the
[Cratis CLI](https://github.com/Cratis/cli). For any value you do not set explicitly, the server
resolves it in this order:

1. Explicit MCP options (environment variables / `appsettings.json`).
2. The `CHRONICLE_CONNECTION_STRING` / `CHRONICLE_MANAGEMENT_PORT` environment variables.
3. The active context in the CLI configuration at `~/.cratis/config.json`.
4. Built-in development defaults (`chronicle://localhost:35000/?disableTls=true`).

When client credentials are used, the server obtains and caches OAuth tokens in `~/.cratis/tokens`,
the same location used by the CLI, so tokens are shared between the two.

All options live under the `Cratis:Chronicle:Mcp` configuration section. As environment variables
they use the `Cratis__Chronicle__Mcp__` prefix:

| Option | Environment variable | Description |
| ------ | -------------------- | ----------- |
| `ConnectionString` | `Cratis__Chronicle__Mcp__ConnectionString` | The Chronicle connection string. |
| `ManagementPort` | `Cratis__Chronicle__Mcp__ManagementPort` | Management port for the HTTP API and token endpoint (default `8080`). |
| `Context` | `Cratis__Chronicle__Mcp__Context` | The CLI context to read connection details from (defaults to the active context). |
| `UseCliConfiguration` | `Cratis__Chronicle__Mcp__UseCliConfiguration` | Set to `false` to ignore `~/.cratis/config.json` entirely. |
| `ClientId` / `ClientSecret` | `Cratis__Chronicle__Mcp__ClientId` / `...__ClientSecret` | Client credentials for authentication. |
| `ApiKey` | `Cratis__Chronicle__Mcp__ApiKey` | An API key to authenticate with, as an alternative to client credentials. |
| `EventStore` | `Cratis__Chronicle__Mcp__EventStore` | The default event store used by tools when none is specified. |
| `Namespace` | `Cratis__Chronicle__Mcp__Namespace` | The default namespace used by tools when none is specified. |

> Note: If no credentials are supplied and none are found in the CLI configuration, the built-in
> development credentials are used, which work against a local development Chronicle server.

## Prompts / Tools

The server exposes the following tools. Every tool defaults the event store and namespace to the
configured defaults when you do not specify them, so you can ask high-level questions and only
mention a store or namespace when you need a specific one.

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

You can ask it things like:

- List all event stores
- List all event types in the [put name here] event store
- List all observers in the [put name here] event store and namespace [put namespace here]
- Show me the events in the [put name here] event store
- Are there any failed partitions?
- What observers in the [put event store name here] use event type [put event type name]

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

# Configuration

Configuration is optional. The server works out of the box with defaults suited to local development:

- **Connection string:** `chronicle://localhost:35000/?disableTls=true`
- **Credentials:** the development client id (`chronicle-dev-client`) and secret (`chronicle-dev-secret`)
- **Event store / namespace:** `default` / `Default`

You only set values when you need to point at a different server or authenticate against a secured one.

## Resolution order

For every value the server has not been given explicitly, it resolves in this order, first match wins:

1. Explicit MCP options — environment variables or `appsettings.json`.
2. The `CHRONICLE_CONNECTION_STRING` environment variable.
3. The active context in the Cratis CLI configuration at `~/.cratis/config.json`.
4. Built-in development defaults.

Because step 3 reads the same file the [Cratis CLI](https://github.com/Cratis/cli) writes, a store you have already set up with `cratis context create` is picked up automatically — no separate configuration.

## Options

All options live under the `Cratis:Chronicle:Mcp` configuration section. As environment variables they take the `Cratis__Chronicle__Mcp__` prefix:

| Option | Environment variable | Description |
| ------ | -------------------- | ----------- |
| `ConnectionString` | `Cratis__Chronicle__Mcp__ConnectionString` | The Chronicle connection string. Defaults to `chronicle://localhost:35000/?disableTls=true`. |
| `Context` | `Cratis__Chronicle__Mcp__Context` | The CLI context to read connection details from. Defaults to the active context. |
| `UseCliConfiguration` | `Cratis__Chronicle__Mcp__UseCliConfiguration` | Set to `false` to ignore `~/.cratis/config.json` entirely. |
| `ClientId` / `ClientSecret` | `Cratis__Chronicle__Mcp__ClientId` / `...__ClientSecret` | Client credentials for authentication. Defaults to development credentials. |
| `ApiKey` | `Cratis__Chronicle__Mcp__ApiKey` | An API key to authenticate with, as an alternative to client credentials. |
| `EventStore` | `Cratis__Chronicle__Mcp__EventStore` | The default event store used by tools when none is specified. Defaults to `default`. |
| `Namespace` | `Cratis__Chronicle__Mcp__Namespace` | The default namespace used by tools when none is specified. Defaults to `Default`. |

Every tool defaults the event store and namespace to these configured values, so you can ask high-level questions and only name a store or namespace when you need a specific one.

## Authentication

The server authenticates the same way the CLI does, and shares its token cache:

- **Client credentials (OAuth):** when the connection string carries a client id and secret (or you set `ClientId`/`ClientSecret`), the server obtains an OAuth token from the server's token endpoint, derived from the connection's own address. Tokens are cached on disk under `~/.cratis/tokens`, the same location the CLI uses, so the two share credentials.
- **API key:** set `ApiKey` (or embed `apiKey=` in the connection string) to authenticate with a static key instead.

> The token endpoint is taken from the connected server's address; there is no separate management-port setting. If you are upgrading from an older release that had a `ManagementPort` option, you can remove it — it no longer has any effect.

## Connect timeout

Set `CHRONICLE_CONNECT_TIMEOUT_SECONDS` to override the default 5-second connect timeout when reaching a slow or remote server.

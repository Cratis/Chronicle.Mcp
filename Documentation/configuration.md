# Configuration

Configuration is optional. The server works out of the box with defaults suited to local development:

- **Connection string:** `chronicle://localhost:35000`
- **Credentials:** the development client id (`chronicle-dev-client`) and secret (`chronicle-dev-secret`)
- **Event store / namespace:** `default` / `Default`
- **Deployment profile:** `ReadOnly` — job-control tools are not registered.

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
| `Profile` | `Cratis__Chronicle__Mcp__Profile` | `ReadOnly` (default) or `Mutation`. An unknown value prevents startup. |
| `Authorization.Principal` | `Cratis__Chronicle__Mcp__Authorization__Principal` | Identity this process acts for. Required and non-blank in `Mutation`. |
| `Authorization.ExpiresAt` | `Cratis__Chronicle__Mcp__Authorization__ExpiresAt` | Policy expiration as an ISO-8601 timestamp. Required and in the future at mutation startup. |
| `Authorization.RevokedPrincipals` | `Cratis__Chronicle__Mcp__Authorization__RevokedPrincipals__0` | Indexed list of revoked principals. Defaults to empty. |
| `Authorization.Grants` | `Cratis__Chronicle__Mcp__Authorization__Grants__0__Principal` (and other grant fields) | Indexed grants naming principal, store, namespace, operations, and targets. Defaults to empty; `Mutation` requires a grant for its principal. |
| `ConnectionString` | `Cratis__Chronicle__Mcp__ConnectionString` | The Chronicle connection string. Defaults to `chronicle://localhost:35000`. |
| `Context` | `Cratis__Chronicle__Mcp__Context` | The CLI context to read connection details from. Defaults to the active context. |
| `UseCliConfiguration` | `Cratis__Chronicle__Mcp__UseCliConfiguration` | Set to `false` to ignore `~/.cratis/config.json` entirely. |
| `ClientId` / `ClientSecret` | `Cratis__Chronicle__Mcp__ClientId` / `...__ClientSecret` | Client credentials for authentication. Defaults to development credentials. |
| `ApiKey` | `Cratis__Chronicle__Mcp__ApiKey` | An API key to authenticate with, as an alternative to client credentials. |
| `EventStore` | `Cratis__Chronicle__Mcp__EventStore` | The default event store used by tools when none is specified. Defaults to `default`. |
| `Namespace` | `Cratis__Chronicle__Mcp__Namespace` | The default namespace used by tools when none is specified. Defaults to `Default`. |

Every tool defaults the event store and namespace to these configured values, so you can ask high-level questions and only name a store or namespace when you need a specific one.

See [Deployment profiles](deployment-profiles.md) for the complete grant format, wildcard rules, revocation, and denial codes. Profile and authorization settings do not fall back to CLI contexts or development credentials.

## Authentication

Keep credentials in environment variables or a secret store, never in `server.json` or a committed `appsettings.json`. Chronicle authentication does not authorize MCP mutations.

The server authenticates the same way the CLI does, and shares its token cache:

- **Client credentials (OAuth):** when the connection string carries a client id and secret (or you set `ClientId`/`ClientSecret`), the server obtains an OAuth token from the server's token endpoint, derived from the connection's own address. Tokens are cached on disk under `~/.cratis/tokens`, the same location the CLI uses, so the two share credentials.
- **API key:** set `ApiKey` (or embed `apiKey=` in the connection string) to authenticate with a static key instead.

> The token endpoint is taken from the connected server's address; there is no separate management-port setting. If you are upgrading from an older release that had a `ManagementPort` option, you can remove it — it no longer has any effect.

## Connect timeout

Set `CHRONICLE_CONNECT_TIMEOUT_SECONDS` to override the default 5-second connect timeout when reaching a slow or remote server.

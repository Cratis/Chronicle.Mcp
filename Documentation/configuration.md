---
title: Configuration
description: Configure the Chronicle.Mcp server's connection string, credentials, default event store and namespace, and deployment profile.
---

# Configuration

Set the Chronicle destination and credentials explicitly. Missing connection or authentication settings prevent startup. The event store and namespace still default to `default` / `Default`, and the deployment profile defaults to `ReadOnly` (no job-control tools).

For local development only, set `Cratis__Chronicle__Mcp__UseDevelopmentCredentials=true`. This explicitly enables the development credentials and the `chronicle://localhost:35000` connection default. Confirm the server, event store and namespace you intend to use before connecting.

Keep credentials in environment variables or a secret store, never in `server.json` or a committed `appsettings.json`. Chronicle authentication does not authorize MCP mutations.

## Resolution order

The host reads environment variables and, optionally, one explicitly selected JSON file:

- Pass `--config /absolute/path/settings.json`, or set `CHRONICLE_MCP_CONFIG` to an absolute path. The argument takes precedence.
- Option environment variables override the selected file. A selected file must exist and contain valid JSON.
- No working-directory `appsettings.json`, environment-specific file, or user secrets are discovered. Relative configuration paths are rejected.
- Command-line configuration overrides are not accepted; only `--config` followed by an absolute file path is supported. Logging filters still use the selected file's `Logging` section or environment variables such as `Logging__LogLevel__Default`.

Connection resolution is, first match wins:

1. Explicit MCP options from the selected file or environment.
2. The `CHRONICLE_CONNECTION_STRING` environment variable.
3. The selected CLI context, only when `UseCliConfiguration=true` and `CliConfigurationPath` names an absolute file path.
4. The localhost development default, only when `UseDevelopmentCredentials=true`.

For example, set `Cratis__Chronicle__Mcp__UseCliConfiguration=true` and `Cratis__Chronicle__Mcp__CliConfigurationPath=/home/example/.cratis/config.json` to read a CLI configuration. A missing or malformed selected CLI file is an error. A container has its own filesystem; the Docker example does not mount the host's CLI files.

## Options

All options live under the `Cratis:Chronicle:Mcp` configuration section. As environment variables they take the `Cratis__Chronicle__Mcp__` prefix:

| Option | Environment variable | Description |
| ------ | -------------------- | ----------- |
| `Profile` | `Cratis__Chronicle__Mcp__Profile` | `ReadOnly` (default) or `Mutation`. An unknown value prevents startup. |
| `Authorization.Principal` | `Cratis__Chronicle__Mcp__Authorization__Principal` | Identity this process acts for. Required and non-blank in `Mutation`. |
| `Authorization.ExpiresAt` | `Cratis__Chronicle__Mcp__Authorization__ExpiresAt` | Policy expiration as an ISO-8601 timestamp. Required and in the future at mutation startup. |
| `Authorization.RevokedPrincipals` | `Cratis__Chronicle__Mcp__Authorization__RevokedPrincipals__0` | Indexed list of revoked principals. Defaults to empty. |
| `Authorization.Grants` | `Cratis__Chronicle__Mcp__Authorization__Grants__0__Principal` (and other grant fields) | Indexed grants naming principal, store, namespace, operations, and targets. Defaults to empty; `Mutation` requires a grant for its principal. |
| `ConnectionString` | `Cratis__Chronicle__Mcp__ConnectionString` | The Chronicle connection string. Required unless supplied by a selected CLI context or development opt-in. |
| `Context` | `Cratis__Chronicle__Mcp__Context` | The CLI context to read connection details from. Defaults to the active context. |
| `UseCliConfiguration` | `Cratis__Chronicle__Mcp__UseCliConfiguration` | Defaults to `false`; enables the explicitly selected CLI file. |
| `CliConfigurationPath` | `Cratis__Chronicle__Mcp__CliConfigurationPath` | Absolute CLI configuration path, required when CLI configuration is enabled. |
| `UseDevelopmentCredentials` | `Cratis__Chronicle__Mcp__UseDevelopmentCredentials` | Defaults to `false`; explicitly enables development credentials and the localhost connection default. |
| `ClientId` / `ClientSecret` | `Cratis__Chronicle__Mcp__ClientId` / `...__ClientSecret` | Explicit client credentials for authentication. No implicit development credentials. |
| `ApiKey` | `Cratis__Chronicle__Mcp__ApiKey` | An API key to authenticate with, as an alternative to client credentials. |
| `EventStore` | `Cratis__Chronicle__Mcp__EventStore` | The default event store used by tools when none is specified. Defaults to `default`. |
| `Namespace` | `Cratis__Chronicle__Mcp__Namespace` | The default namespace used by tools when none is specified. Defaults to `Default`. |

Check which scope arguments each tool accepts and what values the agent supplies. Do not assume a high-level question selected the intended store or namespace.

See [Deployment profiles](deployment-profiles.md) for the complete grant format, wildcard rules, revocation, and denial codes. Profile and authorization settings do not fall back to CLI contexts or development credentials.

## Authentication

Explicit embedded authentication takes precedence, followed by `ApiKey` or a complete `ClientId`/`ClientSecret` pair. Partial client credentials are rejected.

- **CLI login:** the token must be unexpired and its recorded `tokenServer` must match the configured host and port. TLS must be enabled and certificate validation must not be skipped (`disableTls=false` and `skipTlsValidation=false`). Because the Chronicle client's validation default is permissive, set `skipTlsValidation=false` explicitly when reusing a CLI login. Tokens without a destination or for another destination are never sent. Log in again with a CLI version that records `tokenServer` to replace an unbound login. CLI service-account credentials are used only for the context's own destination.
- **Client credentials (OAuth):** the token endpoint comes from the configured server address. Tokens are cached under `~/.cratis/mcp/tokens`, separately from the CLI's unscoped cache. The cache key includes destination, context and client ID; each cached token also records its destination, which must match before reuse. The MCP server never reads the CLI's shared OAuth token cache.
- **API key:** set `ApiKey` (or embed `apiKey=` in the connection string) to authenticate with a static key instead.

Explicit API keys, embedded credentials and explicit `ClientId`/`ClientSecret` options support SRV, multi-host and load-balancer connections. OAuth connections on those topologies bypass the disk token cache, rather than treating a topology-wide key as proof of the issuing server. Fixed destinations retain destination-bound disk caching. Implicit CLI and development credentials require one fixed destination.

## Migrating existing configuration

Replace implicit working-directory configuration and command-line overrides with `--config`, `CHRONICLE_MCP_CONFIG`, or option environment variables. Enable CLI configuration explicitly and supply `CliConfigurationPath` if you use it. CLI login-token reuse requires verified TLS; explicitly set `skipTlsValidation=false`. Supply credentials or opt into development credentials for local development. Existing unscoped OAuth cache files are ignored; MCP obtains its own token and caches it only for a fixed destination.

The development Compose services now publish ports only on `127.0.0.1`, including the Aspire dashboard. Remote machines cannot reach these published ports by default.

> The token endpoint is taken from the connected server's address; there is no separate management-port setting. If you are upgrading from an older release that had a `ManagementPort` option, you can remove it — it no longer has any effect.

## Connect timeout

Set `CHRONICLE_CONNECT_TIMEOUT_SECONDS` to override the default 5-second connect timeout when reaching a slow or remote server.

---
title: Deployment profiles
description: Choose read-only access or authorize specific Chronicle job mutations through a bounded deployment policy for the MCP server your agent connects to.
---

Use the default `ReadOnly` profile when your agent only needs to explore a store, diagnose observers, or generate design-time artifacts. Enable `Mutation` only when this deployment should control specific jobs.

## Choose a profile

| Profile | Exposed tools | Required policy |
| ------- | ------------- | --------------- |
| `ReadOnly` (default) | Only tools explicitly declared read-only. Job inspection and design-time tools remain available. | None. |
| `Mutation` | All tools, including `stop_job`, `resume_job`, and `delete_job`. Each mutation requires a matching local policy. | Connection credentials, a future expiration, and an explicit allowlist or valid legacy grants. |

An unknown profile prevents startup. In `ReadOnly`, mutating tools are absent from the tool list and cannot be called. This is the safe local-development default, even when Chronicle uses development credentials.

Changing the profile's exposed tool set requires a restart. The mutation policy is read on every call; changes to a reloadable `appsettings.json`, including revocations, take effect without restarting. Environment variable changes require restarting the process. A running mutation deployment also denies mutations if its current profile is changed to `ReadOnly`.

## Understand the trust boundary

Both profiles label Chronicle tool payloads as [untrusted data](concepts.md#untrusted-data-in-tool-results). Stored names, messages, definitions, and events never supply instructions or authorization for another call. Hosts should read `untrustedData.content` in structured results and require an explicit user request before any job mutation; the envelope does not replace grants or host policy.

Stdio has one caller per process. The effective Chronicle connection credentials establish that process's identity. Use dedicated, least-privilege client credentials (`ClientId` / `ClientSecret`) for mutation deployments. A client-credentials grant represents the client application, not a delegated human identity. There is no independently authenticated MCP caller, and HTTP OAuth is not implemented. Run separate processes with separate credentials and policies for different callers, and protect who can launch the process or edit its configuration.

`Authorization.Principal`, grant principals, and revoked principals are **deprecated local policy selectors, not authenticated identities**. They cannot establish or change the caller's Chronicle identity.

Locality and credentials alone never enable MCP mutations. The MCP policy is only a local safety bound, not an authorization service: **Chronicle remains authoritative**. A local allow does not prove that Chronicle enforces per-client job permissions; kernel job authorization is separate work tracked in [Chronicle #6205](https://github.com/Cratis/Chronicle/issues/6205). Chronicle authentication or permission denials are still propagated as sanitized MCP denials.

Mutation startup rejects the well-known development credentials by default, including when embedded explicitly. For isolated local development only, set `Authorization.AllowDevelopmentCredentials=true` in addition to `UseDevelopmentCredentials=true` when using the development fallback. This dangerous exception logs a warning on every start. `ReadOnly` does not require the extra opt-in. API keys and destination-bound CLI login tokens continue working for Mutation, but log a deprecation warning recommending dedicated client credentials; credential precedence and destination checks are unchanged.

Keep credentials in environment variables or a secret store. Never put API keys, client secrets, or credential-bearing connection strings in `server.json` or a committed `appsettings.json`. The policy examples below contain no secrets.

Express each authorization list in a single configuration source; a collection or object with both a scalar value (even an empty value) and children is ambiguous and rejected.

## Configure a mutation policy

All settings live under `Cratis:Chronicle:Mcp`. A mutation deployment refuses to start when:

- Credentials cannot be resolved, or are the development credentials without the mutation opt-in.
- `Authorization.ExpiresAt` is missing, malformed, offset-less, or is not in the future. Use an ISO-8601 timestamp with `Z` or an explicit offset. Expiry more than 90 days away produces a warning.
- The allowlist is empty, or any entry has a blank store or namespace, missing/empty operations, or an unknown, read-only, or wildcard operation.
- A legacy policy is present but has a blank principal, no grant for that principal, or a grant with blank principal/store/namespace, empty operations/targets, or an invalid operation.

Startup errors name setting paths, never their values. Operation wildcards are invalid, so adding a new mutating tool never grants it implicitly.

### Principal-free allowlist

For a new deployment, configure `Authorization` as follows, and supply dedicated client credentials separately as described in [Configuration](configuration.md). Replace the scope and expiry before starting:

```json
{
  "ExpiresAt": "2027-01-01T00:00:00Z",
  "Allowlist": [
    {
      "EventStore": "orders",
      "Namespace": "support",
      "Operations": ["stop_job", "resume_job"]
    }
  ]
}
```

The equivalent variables are `Cratis__Chronicle__Mcp__Authorization__ExpiresAt`, `...__Allowlist__0__EventStore`, `...__Allowlist__0__Namespace`, and `...__Allowlist__0__Operations__0` (and `__1`). Set `Cratis__Chronicle__Mcp__Profile=Mutation` too.

One entry must match the exact operation, event store, and namespace together. Matching is ordinal and case-sensitive; `*` is supported only for store and namespace, never operations. Both scopes set to `*` are unbounded across stores and namespaces and produce a warning; prefer specific bounds. Unknown, missing, empty, or malformed policy never grants unrestricted access. Omitted/blank call scopes use the same configured defaults as the tools.

**Allowlist entries permit all job IDs within their matching scope.** They have no principal, revocation, or target fields. The `jobId` must still be present and parse as a GUID. Use legacy grants while you need their narrower target restrictions.

### Existing principal-based policy (deprecated)

This legacy `appsettings.json` authorizes stopping and resuming one job in the `orders` store's `support` namespace. Replace the principal, scope, target, and expiration with the intended values before starting the server. Choose a short expiration suitable for the work.

```json
{
  "Cratis": {
    "Chronicle": {
      "Mcp": {
        "Profile": "Mutation",
        "ConnectionString": "chronicle://localhost:35000",
        "UseCliConfiguration": false,
        "EventStore": "orders",
        "Namespace": "support",
        "Authorization": {
          "Principal": "support-agent",
          "ExpiresAt": "2027-01-01T00:00:00Z",
          "RevokedPrincipals": [],
          "Grants": [
            {
              "Principal": "support-agent",
              "EventStore": "orders",
              "Namespace": "support",
              "Operations": ["stop_job", "resume_job"],
              "Targets": ["621d846d-56b7-49a4-8c1b-7703c6955c11"]
            }
          ]
        }
      }
    }
  }
}
```

The equivalent environment configuration uses indexed arrays. These variables configure the server process; when using Docker, pass them into the container with `-e VARIABLE_NAME`.

```bash
export Cratis__Chronicle__Mcp__Profile=Mutation
export Cratis__Chronicle__Mcp__ConnectionString=chronicle://localhost:35000
export Cratis__Chronicle__Mcp__UseCliConfiguration=false
export Cratis__Chronicle__Mcp__EventStore=orders
export Cratis__Chronicle__Mcp__Namespace=support
export Cratis__Chronicle__Mcp__Authorization__Principal=support-agent
export Cratis__Chronicle__Mcp__Authorization__ExpiresAt=2027-01-01T00:00:00Z
export Cratis__Chronicle__Mcp__Authorization__Grants__0__Principal=support-agent
export Cratis__Chronicle__Mcp__Authorization__Grants__0__EventStore=orders
export Cratis__Chronicle__Mcp__Authorization__Grants__0__Namespace=support
export Cratis__Chronicle__Mcp__Authorization__Grants__0__Operations__0=stop_job
export Cratis__Chronicle__Mcp__Authorization__Grants__0__Operations__1=resume_job
export Cratis__Chronicle__Mcp__Authorization__Grants__0__Targets__0=621d846d-56b7-49a4-8c1b-7703c6955c11
```

Start from an otherwise empty authorization configuration to make these examples equivalent. .NET configuration merges arrays across sources; an environment override does not remove grants already present in a JSON file.

Legacy matching remains ordinal, case-sensitive, and exact. `*` matches any store, namespace, or target, but never an operation or principal. Target strings must exactly match the supplied `jobId` string. All dimensions must match the same grant. If a caller omits or leaves the store or namespace blank, the server checks the same [configured defaults](configuration.md) the tool uses.

To revoke a legacy local policy selector, add it to `Authorization.RevokedPrincipals`. For an environment-based deployment, use `Cratis__Chronicle__Mcp__Authorization__RevokedPrincipals__0=support-agent` and restart. For a reloadable JSON policy, the next call observes the revocation.

### Migrate without widening access accidentally

Existing valid principal-based policies keep their restrictions and emit a value-free deprecation warning to stderr at startup and on reload. If any legacy key (`Principal`, `Grants`, or `RevokedPrincipals`) is present, including an empty value, the complete legacy policy must remain valid. When `Allowlist` and legacy keys coexist, **both policies must allow the request**; legacy target constraints and revocation are never bypassed.

Start from clean policy configuration and check all environment overlays; a leftover legacy key still activates legacy validation. Remove all legacy keys only as a deliberate migration. That removes their principal, job-target, and revocation restrictions; the allowlist alone then bounds stores, namespaces, and operations across all jobs. For principal-free policies, revoke access by removing or narrowing the allowlist, expiring the policy, switching to ReadOnly, or revoking the connection credentials upstream. Reloadable policy edits are checked on the next call. Operator warnings go to stderr, never protocol stdout, and contain no configured values.

## Handle denials

A denial returns `isError: true`, with identical JSON in structured content and a text block:

```json
{"error":{"code":"not_authorized","operation":"stop_job"}}
```

The response never includes store names, namespaces, targets, principals, credentials, or exception text. Denials are logged at Warning to stderr with only the code and operation.

| Code | Meaning |
| ---- | ------- |
| `bad_request` | The target argument is missing or does not parse as its required type (`Guid` for `jobId`). |
| `not_authenticated` | A required legacy local policy selector is blank or absent; this is not a Chronicle authentication result. |
| `policy_invalid` | The current policy is malformed, cannot be bound, or no longer permits the mutation profile. |
| `policy_expired` | The expiration is missing or has been reached. |
| `principal_revoked` | The legacy local policy selector is revoked. |
| `not_authorized` | No allowlist entry matches, or a legacy principal/store/namespace/operation/target restriction denies the request. |
| `chronicle_denied` | Chronicle returns gRPC `PermissionDenied` or `Unauthenticated`, including for a read tool. |

For mutating calls, checks run in table order from `bad_request` through `not_authorized`; the first failure wins and the tool is not invoked. `chronicle_denied` is returned only after a tool reaches Chronicle and Chronicle refuses the call.

## Confirm destructive operations

`delete_job` is destructive and irreversible: the removed job and its progress cannot be recovered. MCP clients should ask the user to confirm the specific job and scope before calling it. The server does not prompt; a matching grant does not replace that confirmation.

Use [job inspection](operate-side/jobs.md) to check a job's current state before stopping, resuming, or deleting it.

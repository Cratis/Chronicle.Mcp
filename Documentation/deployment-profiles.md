---
title: Deployment profiles
description: Choose read-only access or authorize specific Chronicle job mutations with a bounded deployment policy.
---

Use the default `ReadOnly` profile when your agent only needs to explore a store, diagnose observers, or generate design-time artifacts. Enable `Mutation` only when this deployment should control specific jobs.

## Choose a profile

| Profile | Exposed tools | Required policy |
| ------- | ------------- | --------------- |
| `ReadOnly` (default) | Only tools explicitly declared read-only. Job inspection and design-time tools remain available. | None. |
| `Mutation` | All tools, including `stop_job`, `resume_job`, and `delete_job`. Each mutation requires a matching grant. | A principal, a future expiration, and valid grants. |

An unknown profile prevents startup. In `ReadOnly`, mutating tools are absent from the tool list and cannot be called. This is the safe local-development default, even when Chronicle uses development credentials.

Changing the profile's exposed tool set requires a restart. The mutation policy is read on every call; changes to a reloadable `appsettings.json`, including revocations, take effect without restarting. Environment variable changes require restarting the process. A running mutation deployment also denies mutations if its current profile is changed to `ReadOnly`.

## Understand the trust boundary

Stdio has one caller per process. `Authorization.Principal` is the identity this deployment acts for, not a principal supplied by individual tool arguments. Run separate processes with separate policies for different callers, and protect who can launch the process or edit its configuration.

Locality, a connection string, and Chronicle credentials never grant MCP mutation authority. MCP grants only narrow access: **Chronicle remains the authority** and applies its own authentication and authorization to reads and mutations alike.

Keep credentials in environment variables or a secret store. Never put API keys, client secrets, or credential-bearing connection strings in `server.json` or a committed `appsettings.json`. The policy examples below contain no secrets.

Express each authorization list in a single configuration source; a collection or object with both a scalar value (even an empty value) and children is ambiguous and rejected.

## Configure a mutation policy

All settings live under `Cratis:Chronicle:Mcp`. A mutation deployment refuses to start when:

- `Authorization.Principal` is blank.
- `Authorization.ExpiresAt` is missing or is not in the future.
- No grant names the configured principal.
- Any grant has a blank principal, store, or namespace, empty operations or targets, or an operation that is not a known mutating tool.

Startup errors name setting paths, never their values. Operation wildcards are invalid, so adding a new mutating tool never grants it implicitly.

This complete `appsettings.json` authorizes stopping and resuming one job in the `orders` store's `support` namespace. Replace the principal, scope, target, and expiration with the intended values before starting the server. Choose a short expiration suitable for the work.

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

Matching is ordinal, case-sensitive, and exact. `*` matches any store, namespace, or target, but never an operation or principal. Target strings must exactly match the supplied `jobId` string. All dimensions must match the same grant. If a caller omits or leaves the store or namespace blank, the server checks the same [configured defaults](configuration.md) the tool uses.

To revoke a deployment principal, add it to `Authorization.RevokedPrincipals`. For an environment-based deployment, use `Cratis__Chronicle__Mcp__Authorization__RevokedPrincipals__0=support-agent` and restart. For a reloadable JSON policy, the next call observes the revocation.

## Handle denials

A denial returns `isError: true`, with identical JSON in structured content and a text block:

```json
{"error":{"code":"not_authorized","operation":"stop_job"}}
```

The response never includes store names, namespaces, targets, principals, credentials, or exception text. Denials are logged at Warning to stderr with only the code and operation.

| Code | Meaning |
| ---- | ------- |
| `bad_request` | The target argument is missing or does not parse as its required type (`Guid` for `jobId`). |
| `not_authenticated` | The configured deployment principal is blank or absent. |
| `policy_invalid` | The current policy is malformed, cannot be bound, or no longer permits the mutation profile. |
| `policy_expired` | The expiration is missing or has been reached. |
| `principal_revoked` | The deployment principal is revoked. |
| `not_authorized` | No grant matches the principal, store, namespace, operation, and target. |
| `chronicle_denied` | Chronicle returns gRPC `PermissionDenied` or `Unauthenticated`, including for a read tool. |

For mutating calls, checks run in table order from `bad_request` through `not_authorized`; the first failure wins and the tool is not invoked. `chronicle_denied` is returned only after a tool reaches Chronicle and Chronicle refuses the call.

## Confirm destructive operations

`delete_job` is destructive and irreversible: the removed job and its progress cannot be recovered. MCP clients should ask the user to confirm the specific job and scope before calling it. The server does not prompt; a matching grant does not replace that confirmation.

Use [job inspection](operate-side/jobs.md) to check a job's current state before stopping, resuming, or deleting it.

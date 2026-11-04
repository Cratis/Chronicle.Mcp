---
title: Jobs
description: Use Chronicle.Mcp tools to list Chronicle jobs, inspect their progress and steps, and stop, resume or delete them under the Mutation profile.
---

# Jobs

Chronicle runs long-lived work, including replays, projection rebuilds, and other maintenance, as jobs. Use Chronicle MCP to find a job, inspect its progress, and see which steps have completed or stopped. You can do that investigation in the default `ReadOnly` profile without enabling job control.

Only `stop_job`, `resume_job`, and `delete_job` change job state. They are unavailable in `ReadOnly` and require the `Mutation` profile plus a matching grant for the principal, event store, namespace, operation, and job ID. [Deployment profiles](../deployment-profiles.md) explains how that authority is bounded.

:::danger[Deleting a job is irreversible]
`delete_job` removes the job; once removed, it cannot be recovered. Stopping a job is different: it keeps its progress and can be resumed. Inspect the job before choosing either action.
:::

## Typical flow

1. Call `list_jobs` in the event store and namespace you want to inspect. Filter by job status when, for example, you only need failed jobs.
2. Select a job id and call `get_job`. Its details, progress, and status-change history show what the job is doing and how it reached its current state.
3. Use `get_job_steps` with that id to examine individual steps. An optional step-status filter helps narrow the result to the work you are investigating.

This inspection can be the whole task. If you need to intervene, continue with the control guidance below; inspection does not require a control call.

## A note on control operations

Before a control call, have the agent use `get_job` to confirm the job id and current state. Confirm the event store and namespace too; tools use your [configured defaults](../configuration.md) if you omit them. MCP clients should ask you to confirm the specific job and scope before calling a control tool. The server does not prompt.

The server marks these three tools as state-changing, so a client that asks before state-changing calls asks for them. All three are safe to repeat, but that does not make deletion reversible. `stop_job` and `resume_job` are not destructive; a stopped job keeps its progress and can be resumed. See [Which tools change state](../concepts.md#which-tools-change-state) for the MCP annotations and their meaning.

## Tools

| Tool | Description |
| ---- | ----------- |
| `list_jobs` | List all jobs in a namespace, optionally filtered by job status. |
| `get_job` | Get a specific job by id, including full details and status changes. |
| `get_job_steps` | Get the steps of a specific job, optionally filtered by step status. |
| `stop_job` | Mutation profile: stop a specific job. It transitions to a stopped status. |
| `resume_job` | Mutation profile: resume a specific stopped job. |
| `delete_job` | Mutation profile: delete a specific job. It transitions to a removing status. |

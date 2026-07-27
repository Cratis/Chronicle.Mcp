# Jobs

Chronicle runs long-lived work — replays, projections rebuilds, and other maintenance — as jobs. These tools let an agent list jobs, inspect their progress and steps, and control their lifecycle.

## Tools

| Tool | Description |
| ---- | ----------- |
| `list_jobs` | List all jobs in a namespace, optionally filtered by job status. |
| `get_job` | Get a specific job by id, including full details and status changes. |
| `get_job_steps` | Get the steps of a specific job, optionally filtered by step status. |
| `stop_job` | Stop a specific job. It transitions to a stopped status. |
| `resume_job` | Resume a specific stopped job. |
| `delete_job` | Delete a specific job. It transitions to a removing status. |

## Typical flow

1. `list_jobs` — see what is running or has run, filtered by status when you only care about, say, failed jobs.
2. `get_job` — inspect one job's details and the history of its status changes.
3. `get_job_steps` — drill into the individual steps to see where a job is or where it stopped.
4. `stop_job` / `resume_job` / `delete_job` — control the job once you understand its state.

## A note on control operations

`stop_job`, `resume_job`, and `delete_job` change a running system. Have the agent confirm the job id and its current state — with `get_job` — before invoking them, the same care you would take from the CLI.

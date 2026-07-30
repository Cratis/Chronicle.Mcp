# Describe the system

You inherit an event store — or return to one after six months — and the first question is always the same: *what is this system, and what is it for?* The answer is already in the store. Event names are past-tense facts about subjects, so the names themselves carry which entities exist and what can happen to them. `describe_system` reads that language back out.

Ask *"describe the system in Sales"* and the tool deduces a structural model from the registered metadata: it clusters event types into the entities they describe, places every event in its entity's lifecycle, and maps the read models and automations around them. The assistant then turns that model into prose — what the system is, and the story of how it behaves.

## How the deduction works

Everything is deduced from what the store actually contains — nothing is guessed from code or documentation:

- **Entities** come from event type names. `AuthorRegistered`, `AuthorNameChanged`, and `AuthorRemoved` all name the same subject, so they cluster as the entity **Author**. Property-scoped clusters fold into their parent: with both `CustomerRegistered` and `CustomerAddressUpdated` present, "CustomerAddress" folds into **Customer** so the whole lifecycle sits together.
- **Lifecycle stages** come from the verb: *Registered/Created/Hired* mark **Creation**, *Changed/Renamed/Assigned* mark **Mutation**, *Received/Shipped/LoggedIn* mark **Activity**, *Corrected/Redacted* mark **Correction**, and *Removed/Closed/Cancelled* mark **Termination**.
- **Properties** come from each event type's registered JSON schema — the same field detail as [describe an event type](describe-event-type.md).
- **Read surfaces** are the projections and reducers with the read models they build — what the business watches.
- **Automations** are the reactors and external observers — what the system does on its own.

## Parameters

| Parameter | Required | Description |
| --------- | -------- | ----------- |
| `eventStore` | no | The event store to describe. Defaults to the configured event store. |
| `namespace` | no | The namespace to resolve observers in. Defaults to the configured namespace. |

## What it returns

- **Entities** — each with its events ordered by lifecycle stage, and every event's properties.
- **Read surfaces** — each read model with how it is built (projection or reducer) and the event types it derives from.
- **Automations** — each observer with the event types that trigger it.
- **Unconsumed event types** — facts the system records but nothing acts on yet.
- **Statistics** — headline counts, and the namespaces present in the store.
- **Narrative guidance** — instructions the assistant follows to turn the model into a description and a story.

## Example

**Prompt:** *"Describe the system in HR — and tell me its story."*

From a store holding `EmployeeHired`, `EmployeeMoved`, `EmployeePromoted`, and `EmployeeTerminated`, plus an `AllEmployees` projection and an `OnboardingNotifier` reactor, the assistant deduces an HR system centered on the **Employee** entity and narrates it: an employee's story begins when they are hired with a name and title; along the way they can move (the events carry the address vocabulary) and be promoted; the business watches all employees through `AllEmployees`; and when someone is hired, the system acts on its own to start onboarding. Termination ends the story.

The `describe_system` prompt packages this workflow, so clients that surface MCP prompts can offer it directly.

## Related

- [Suggest the next event types](suggest-next-event-types.md) — the same deduced model, used to find the gaps.
- [Event catalog](event-catalog.md) — the flat data dictionary underneath the narrative.
- [Ad-hoc projection](ad-hoc-projection.md) — query the described system's actual data.

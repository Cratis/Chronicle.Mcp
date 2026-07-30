# Suggest the next event types

A growing event model develops blind spots: an entity that can be created but never removed, a value set once at registration that the business later needs to change, a lifecycle whose beginning is implicit. You usually discover these gaps the hard way — when a feature needs an event that does not exist. `suggest_next_event_types` finds them first.

Ask *"what event types should I introduce next for the system in Sales?"* and the tool deduces the system's entities and lifecycles from the registered event types (the same deduction as [describe the system](describe-system.md)), then runs deterministic gap analysis over them. Every suggestion names the concrete gap it closes — these are grounded findings, not brainstorming.

## The gaps it finds

- **No explicit creation** — an entity's first recorded fact is a mutation or termination, so its lifecycle starts implicitly. Suggests an explicit creation event that gives projections a reliable initialization point.
- **A life without an end** — the entity has a beginning but nothing ever removes, closes, or archives it. Suggests a termination event so read models can clear state (`[RemovedWith]`) and compliance and cleanup have a fact to act on.
- **Facts frozen at creation** — a property is set when the entity comes to life and no later event ever touches it. If the business allows it to change, that change needs its own event. Identifier properties are excluded — those genuinely never change.

## Parameters

| Parameter | Required | Description |
| --------- | -------- | ----------- |
| `eventStore` | no | The event store to analyze. Defaults to the configured event store. |
| `entity` | no | An entity name to scope the suggestions to (case-insensitive). Omit for the whole system. |

## What it returns

- **Suggestions** — each with a suggested name (past tense, one purpose), the entity and lifecycle stage it completes, the rationale naming the gap, and candidate properties drawn from the existing schema.
- **The entities analyzed** — so the assistant can reason about the suggestions in context.
- **Guidance** — instructions for refining the seeds with domain knowledge.

## The suggestions are seeds

The analysis is deliberately mechanical so it never invents facts — which means the names need the domain's own language. The guidance tells the assistant to refine them: a Book is *Withdrawn*, not *Removed*; an Order is *Cancelled*, not *Deleted*; and some facts really are immutable, in which case the right answer is to discard the suggestion and say why.

## Example

**Prompt:** *"What event types should I introduce next for the system in CRM?"*

From a store holding `CustomerRegistered` (with `Email`, `FullName`, `PhoneNumber`) and `CustomerAddressUpdated`, the analysis finds that nothing ever ends a customer's life, and that email, name, and phone number are frozen at registration. The assistant proposes `CustomerRemoved` (or, refined, `CustomerOffboarded`), plus `CustomerEmailChanged`, `CustomerFullNameChanged`, and `CustomerPhoneNumberChanged` — each carrying its property from the existing schema — and notes which ones the business may not need.

The `suggest_next_event_types` prompt packages this workflow for clients that surface MCP prompts.

## Related

- [Describe the system](describe-system.md) — the deduced model the analysis runs over.
- [Unconsumed-event audit](unconsumed-event-audit.md) — the complementary gap: events that exist but nothing reads.
- [Read-model scaffolding](read-model-scaffolding.md) — once the new events exist, project them.

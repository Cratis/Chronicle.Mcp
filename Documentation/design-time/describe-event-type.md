# Describe an event type

`describe_event_type` reads an event type's real schema from the store — every property with its JSON type and a suggested C# type. It is the grounding primitive the other design-time tools build on: any generated projection, read model, or spec starts from the fields the event actually carries, not a guess.

## When to use it

- Before generating anything from an event type, to confirm its shape.
- To answer "what fields does `UserRegistered` have?" without opening source code.
- As the first step when an agent maps a natural-language request onto concrete events.

## Parameters

| Parameter | Required | Description |
| --------- | -------- | ----------- |
| `eventType` | yes | The event type id/name to describe (for example `UserRegistered`). |
| `generation` | no | A specific generation to describe. The latest is used when omitted. |
| `eventStore` | no | The event store. Defaults to the configured event store. |

## What it returns

The event type's identity (id, generation, owner, source, whether it is a tombstone, and the originating store when it is a foreign type received through the inbox), plus a list of properties. Each property carries its name, JSON schema type, a suggested C# type, any schema `format`, whether it is required, and its description.

Returns `null` when no matching event type is registered — a signal that the name is wrong or the type has not been registered yet.

## Example

**Prompt:** *"What does the UserRegistered event look like?"*

The agent calls `describe_event_type` with `eventType: "UserRegistered"` and gets back the grounded shape:

| Property | JSON type | C# type | Required |
| -------- | --------- | ------- | -------- |
| `Email` | string | `string` | yes |
| `Name` | string | `string` | no |
| `RegisteredAt` | string (date-time) | `DateTimeOffset` | no |

The C# type suggestions map JSON schema types and formats to idiomatic C# — `guid`/`uuid` to `Guid`, `date-time` to `DateTimeOffset`, `integer` to `int`, arrays to `IEnumerable<T>`, and so on.

## Related

- [Read-model scaffolding](read-model-scaffolding.md) uses these fields to build a read model.
- [Event catalog](event-catalog.md) returns the same field detail for every event type at once.

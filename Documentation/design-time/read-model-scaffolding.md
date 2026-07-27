# Read-model scaffolding

`scaffold_read_model` is the flagship design-time capability. It turns a set of event types into a reviewable read model and its model-bound projection — with field names and types taken from the events' real schema, never invented. The output is always a proposal to review, never a silent write into your codebase.

## When to use it

- To answer a request like *"Show me all the users registered"* with a working starting point in seconds instead of hand-written boilerplate.
- When product or analytics folks describe a read model in plain English and an engineer needs something to approve in a pull request.

The agent resolves the natural-language request to a concrete list of event types first — with [list event types](../operate-side/exploring-the-store.md), [describe an event type](describe-event-type.md), or the [event catalog](event-catalog.md) — then passes them to this tool.

## Parameters

| Parameter | Required | Description |
| --------- | -------- | ----------- |
| `readModelName` | yes | The name of the read model to generate (for example `Users`). |
| `eventTypes` | yes | A comma-separated list of event type ids to project from (for example `UserRegistered,UserEmailVerified`). |
| `codeNamespace` | no | The C# namespace to generate into. Defaults to `ReadModels`. |
| `eventStore` | no | The event store. Defaults to the configured event store. |

## What it returns

- **Generated C# code** for a model-bound read model and projection.
- **Chosen fields**, each traced back to the event it came from, so you can see why every property is there.
- **Existing projections** that already read these events or target this read model — surfaced so you do not create a duplicate.
- **Resolved and unresolved event types** — the requested types that were found, and any that are not registered.
- **Notes** on caveats, such as a property that appears with two different types across events, or an identity that should become a strongly-typed concept.

If none of the requested event types are registered, it returns no code and says so, rather than fabricating a shape.

## Example

**Prompt:** *"Show me all the users registered."*

The agent resolves the request to the `UserRegistered` event, then calls `scaffold_read_model` with `readModelName: "Users"` and `eventTypes: "UserRegistered"`. The generated code is grounded in `UserRegistered`'s real fields:

```csharp
// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using MongoDB.Driver;

namespace ReadModels;

/// <summary>
/// Users read model, scaffolded from UserRegistered.
/// Review the field selection and replace the Guid identity with a strongly-typed EventSourceId concept before use.
/// </summary>
[ReadModel]
[FromEvent<UserRegistered>]
public record Users(
    Guid Id,
    string Email,
    string Name)
{
    /// <summary>
    /// Queries all Users instances.
    /// </summary>
    /// <param name="collection">The read model collection.</param>
    /// <returns>All Users instances.</returns>
    public static IQueryable<Users> AllUsers(IMongoCollection<Users> collection) =>
        collection.AsQueryable();
}
```

## Reviewing the proposal

The scaffold is a starting point, not a finished slice. When you review it:

- **Identity.** The key is generated as `Guid Id`. Replace it with a strongly-typed identity concept derived from `EventSourceId<Guid>` — the note on the result reminds you.
- **AutoMap.** Field names match the event's property names so Chronicle's AutoMap wires them; keep names aligned or add explicit mapping where they diverge.
- **`using` directives.** `[ReadModel]` and `[FromEvent<T>]` come from Chronicle; most Cratis projects surface them through global usings. Adjust the usings to match your project.
- **Existing projections.** If the result lists an existing projection that already answers the question, prefer surfacing that over adding a duplicate.

## Related

- [Describe an event type](describe-event-type.md) — inspect the fields before scaffolding.
- [Unconsumed-event audit](unconsumed-event-audit.md) — find events that still lack a read model.

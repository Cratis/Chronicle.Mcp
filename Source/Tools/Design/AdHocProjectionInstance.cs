// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;

namespace Cratis.Chronicle.Mcp.Tools.Design;

/// <summary>
/// One instance materialized by an ad-hoc projection — the current state of one event source,
/// folded from the selected events.
/// </summary>
/// <param name="EventSourceId">The event source the instance represents.</param>
/// <param name="Properties">The folded properties — later events overwrite earlier ones per property name.</param>
/// <param name="LastEventSequenceNumber">The sequence number of the last event folded into the instance.</param>
/// <param name="LastOccurred">When the last folded event occurred.</param>
public record AdHocProjectionInstance(
    string EventSourceId,
    IReadOnlyDictionary<string, JsonElement> Properties,
    ulong LastEventSequenceNumber,
    DateTimeOffset? LastOccurred);

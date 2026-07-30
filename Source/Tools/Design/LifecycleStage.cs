// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Tools.Design;

/// <summary>
/// The stage in an entity's lifecycle that an event represents, deduced from the event type's name.
/// </summary>
public enum LifecycleStage
{
    /// <summary>
    /// The event brings an entity into existence (e.g. Registered, Created, Opened).
    /// </summary>
    Creation = 0,

    /// <summary>
    /// The event changes state the entity already has (e.g. Changed, Renamed, Assigned).
    /// </summary>
    Mutation = 1,

    /// <summary>
    /// The event records something that happened around the entity without defining its lifecycle
    /// (e.g. Received, Shipped, LoggedIn).
    /// </summary>
    Activity = 2,

    /// <summary>
    /// The event corrects or reverses an earlier fact (e.g. Corrected, Adjusted, Redacted).
    /// </summary>
    Correction = 3,

    /// <summary>
    /// The event ends the entity's life (e.g. Removed, Closed, Cancelled).
    /// </summary>
    Termination = 4
}

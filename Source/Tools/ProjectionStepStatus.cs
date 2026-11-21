// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Tools;

/// <summary>
/// The status values a <see cref="ProjectionStepResult"/> can carry.
/// </summary>
public static class ProjectionStepStatus
{
    /// <summary>
    /// The history was stepped; every event is in the result.
    /// </summary>
    public const string Ok = "ok";

    /// <summary>
    /// The request itself was not valid, for instance a blank read model or key.
    /// </summary>
    public const string InvalidRequest = "invalid-request";

    /// <summary>
    /// No read model with the given name is registered in the event store.
    /// </summary>
    public const string UnknownReadModel = "unknown-read-model";

    /// <summary>
    /// The read model is not a server-owned projection, so it cannot be stepped.
    /// </summary>
    public const string UnsupportedReadModel = "unsupported-read-model";

    /// <summary>
    /// The projection has no events for the given key.
    /// </summary>
    public const string NoHistory = "no-history";

    /// <summary>
    /// The history is larger than the budget and was refused rather than truncated.
    /// </summary>
    public const string RefusedOverBudget = "refused-over-budget";
}

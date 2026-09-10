// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Tools.Jobs;

/// <summary>
/// A status change that occurred for a job step.
/// </summary>
/// <param name="Status">The status that was set.</param>
/// <param name="Occurred">When the status change occurred.</param>
public record JobStepStatusChangeDescriptor(
    string Status,
    DateTimeOffset Occurred);

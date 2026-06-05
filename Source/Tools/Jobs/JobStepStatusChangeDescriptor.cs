// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Tools.Jobs;

/// <summary>
/// A status change event for a job step.
/// </summary>
/// <param name="Status">The status at the time of the change.</param>
/// <param name="Occurred">When the status change occurred.</param>
/// <param name="ExceptionMessages">Any exception messages associated with the change.</param>
/// <param name="ExceptionStackTrace">Stack trace for exceptions, if any.</param>
public record JobStepStatusChangeDescriptor(
    string Status,
    DateTimeOffset Occurred,
    IEnumerable<string> ExceptionMessages,
    string ExceptionStackTrace);

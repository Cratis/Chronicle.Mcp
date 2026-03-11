// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Api;

/// <summary>
/// Represents a status change for a job in Chronicle.
/// </summary>
/// <param name="Status">The job status value (0=None, 1=Scheduled, 2=PreparingSteps, 3=PreparingStepsForRunning, 4=Started, 5=Running, 6=CompletedSuccessfully, 7=CompletedWithFailures, 8=Stopped, 9=Removing).</param>
/// <param name="Occurred">When the status change occurred.</param>
/// <param name="ExceptionMessages">Any exception messages associated with the status change.</param>
/// <param name="ExceptionStackTrace">The exception stack trace if applicable.</param>
public record JobStatusChanged(
    int Status,
    DateTimeOffset Occurred,
    IEnumerable<string> ExceptionMessages,
    string ExceptionStackTrace)
{
    static readonly string[] _statusNames =
    [
        "None", "Scheduled", "PreparingSteps", "PreparingStepsForRunning",
        "Started", "Running", "CompletedSuccessfully", "CompletedWithFailures",
        "Stopped", "Removing"
    ];

    /// <summary>
    /// Gets the human-readable name of the status.
    /// </summary>
    public string StatusName => Status >= 0 && Status < _statusNames.Length ? _statusNames[Status] : "Unknown";
}

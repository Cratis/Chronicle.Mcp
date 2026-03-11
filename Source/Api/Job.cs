// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Api;

/// <summary>
/// Represents a job in Chronicle.
/// </summary>
/// <param name="Id">The unique identifier of the job.</param>
/// <param name="Details">The details describing what the job does.</param>
/// <param name="Type">The type of the job.</param>
/// <param name="Status">The current status value (0=None, 1=Scheduled, 2=PreparingSteps, 3=PreparingStepsForRunning, 4=Started, 5=Running, 6=CompletedSuccessfully, 7=CompletedWithFailures, 8=Stopped, 9=Removing).</param>
/// <param name="Created">When the job was created.</param>
/// <param name="StatusChanges">The history of status changes for the job.</param>
/// <param name="Progress">The current progress of the job.</param>
public record Job(
    Guid Id,
    string Details,
    string Type,
    int Status,
    DateTimeOffset Created,
    IEnumerable<JobStatusChanged> StatusChanges,
    JobProgress Progress)
{
    static readonly string[] _statusNames =
    [
        "None", "Scheduled", "PreparingSteps", "PreparingStepsForRunning",
        "Started", "Running", "CompletedSuccessfully", "CompletedWithFailures",
        "Stopped", "Removing"
    ];

    /// <summary>
    /// Gets the human-readable name of the current status.
    /// </summary>
    public string StatusName => Status >= 0 && Status < _statusNames.Length ? _statusNames[Status] : "Unknown";
}

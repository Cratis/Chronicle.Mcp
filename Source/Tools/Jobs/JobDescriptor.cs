// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Tools.Jobs;

/// <summary>
/// A concise description of a job.
/// </summary>
/// <param name="Id">The unique identifier of the job.</param>
/// <param name="Details">The details for the job.</param>
/// <param name="Type">The type of the job.</param>
/// <param name="Status">The current status of the job (None, PreparingJob, PreparingSteps, StartingSteps, Running, CompletedSuccessfully, CompletedWithFailures, Stopped, Failed, Removing).</param>
/// <param name="Created">When the job was created.</param>
/// <param name="StatusChanges">Collection of status changes that occurred for the job.</param>
/// <param name="Progress">The current progress of the job.</param>
public record JobDescriptor(
    Guid Id,
    string Details,
    string Type,
    string Status,
    DateTimeOffset Created,
    IEnumerable<StatusChangeDescriptor> StatusChanges,
    JobProgressDescriptor Progress);

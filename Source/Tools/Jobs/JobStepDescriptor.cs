// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Tools.Jobs;

/// <summary>
/// A concise description of a job step.
/// </summary>
/// <param name="Id">The unique identifier of the job step.</param>
/// <param name="Type">The type of the job step.</param>
/// <param name="Name">The name of the job step.</param>
/// <param name="Status">The current status of the job step (Unknown, Scheduled, Running, CompletedSuccessfully, CompletedWithFailure, Stopped, Failed, Removing).</param>
/// <param name="StatusChanges">Collection of status changes that occurred for the job step.</param>
/// <param name="Progress">The current progress of the job step.</param>
public record JobStepDescriptor(
    Guid Id,
    string Type,
    string Name,
    string Status,
    IEnumerable<JobStepStatusChangeDescriptor> StatusChanges,
    JobStepProgressDescriptor? Progress);

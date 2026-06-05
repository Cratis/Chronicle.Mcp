// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Tools.Jobs;

/// <summary>
/// The progress information for a job.
/// </summary>
/// <param name="TotalSteps">Total number of steps in the job.</param>
/// <param name="SuccessfulSteps">Number of completed steps.</param>
/// <param name="FailedSteps">Number of failed steps.</param>
/// <param name="StoppedSteps">Number of stopped steps.</param>
/// <param name="IsCompleted">Whether the job is completed.</param>
/// <param name="IsStopped">Whether the job is stopped.</param>
/// <param name="Message">Current progress message.</param>
public record JobProgressDescriptor(
    int TotalSteps,
    int SuccessfulSteps,
    int FailedSteps,
    int StoppedSteps,
    bool IsCompleted,
    bool IsStopped,
    string Message);

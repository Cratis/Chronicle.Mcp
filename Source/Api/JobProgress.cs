// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Api;

/// <summary>
/// Represents the progress of a job in Chronicle.
/// </summary>
/// <param name="TotalSteps">The total number of steps in the job.</param>
/// <param name="SuccessfulSteps">The number of successfully completed steps.</param>
/// <param name="FailedSteps">The number of failed steps.</param>
/// <param name="StoppedSteps">The number of stopped steps.</param>
/// <param name="IsCompleted">Whether the job has completed.</param>
/// <param name="IsStopped">Whether the job has been stopped.</param>
/// <param name="Message">A message describing the current progress.</param>
public record JobProgress(
    int TotalSteps,
    int SuccessfulSteps,
    int FailedSteps,
    int StoppedSteps,
    bool IsCompleted,
    bool IsStopped,
    string Message);

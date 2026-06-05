// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Tools.Jobs;

/// <summary>
/// The progress information for a job step.
/// </summary>
/// <param name="Percentage">The percentage completion of the step.</param>
/// <param name="Message">Current progress message.</param>
public record JobStepProgressDescriptor(
    int Percentage,
    string Message);

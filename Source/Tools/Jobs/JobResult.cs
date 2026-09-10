// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Tools.Jobs;

/// <summary>
/// Result of a job operation, containing either a job or an error.
/// </summary>
/// <param name="Job">The job descriptor, if found.</param>
/// <param name="Error">The job error, if the job was not found or invalid.</param>
public record JobResult(
    JobDescriptor? Job,
    JobErrorDescriptor? Error)
{
    /// <summary>
    /// Gets whether the result contains an error.
    /// </summary>
    public bool HasError => Error is not null;
}

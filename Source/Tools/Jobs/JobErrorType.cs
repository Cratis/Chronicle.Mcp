// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Tools.Jobs;

/// <summary>
/// The types of job errors that can occur when resolving a job.
/// </summary>
public enum JobErrorType
{
    /// <summary>No error.</summary>
    None = 0,

    /// <summary>The job was not found.</summary>
    NotFound = 1
}

// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Contracts.Jobs;

namespace Cratis.Chronicle.Mcp.Tools.Jobs;

/// <summary>
/// Error information when a job is not found or invalid.
/// </summary>
/// <param name="Error">The type of job error (NotFound, TypeIsNotAJobStateType, TypeIsNotAssociatedWithAJobType).</param>
public record JobErrorDescriptor(JobError Error);

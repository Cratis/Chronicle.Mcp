// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Screenplay.Observations;

namespace Cratis.Chronicle.Mcp.Tools.Design.Conformance;

/// <summary>
/// The allowlisted facts about one observed append.
/// </summary>
/// <param name="Position">Position in the audited sequence.</param>
/// <param name="TypeId">The event type identifier.</param>
/// <param name="Occurred">The occurrence time used for filtering.</param>
/// <param name="Producer">The recognized producer, if any.</param>
internal sealed record ConformanceAppend(long Position, string TypeId, DateTimeOffset Occurred, ObservedProducer? Producer);

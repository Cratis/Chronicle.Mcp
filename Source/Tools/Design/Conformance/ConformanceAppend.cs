// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Screenplay.Observations;

namespace Cratis.Chronicle.Mcp.Tools.Design.Conformance;

internal sealed record ConformanceAppend(long Position, string TypeId, DateTimeOffset Occurred, ObservedProducer? Producer);

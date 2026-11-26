// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Tools;

/// <summary>
/// A bounded list of identities known to an event store namespace.
/// </summary>
/// <param name="Identities">The identities in server order, up to the requested limit.</param>
/// <param name="Truncated">Whether more identities were available than the limit permits.</param>
public record IdentitiesResult(IReadOnlyList<IdentityDescriptor> Identities, bool Truncated);

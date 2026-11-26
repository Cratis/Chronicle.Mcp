// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Tools;

/// <summary>
/// An identity known to Chronicle, including any delegated identity.
/// </summary>
/// <param name="Subject">The identity's subject.</param>
/// <param name="Name">The display name.</param>
/// <param name="UserName">The user name.</param>
/// <param name="OnBehalfOf">The identity on whose behalf this identity acted, if any.</param>
public record IdentityDescriptor(string Subject, string Name, string UserName, IdentityDescriptor? OnBehalfOf);

// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Tools.Design;

/// <summary>
/// The identity that caused an event.
/// </summary>
/// <param name="Subject">The subject identifier.</param>
/// <param name="Name">The display name.</param>
/// <param name="UserName">The user name.</param>
public record CausedByDescriptor(string Subject, string Name, string UserName);

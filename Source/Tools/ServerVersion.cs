// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Tools;

/// <summary>
/// Version information for the connected Chronicle server.
/// </summary>
/// <param name="Version">The server version.</param>
/// <param name="CommitSha">The commit SHA the server was built from.</param>
public record ServerVersion(string Version, string CommitSha);

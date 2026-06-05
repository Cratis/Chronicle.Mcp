// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Connections;

namespace Cratis.Chronicle.Mcp.Connection;

/// <summary>
/// A <see cref="ITokenProvider"/> that returns a pre-configured static token (e.g. an API key).
/// </summary>
/// <param name="token">The access token to return for all calls.</param>
public sealed class StaticTokenProvider(string token) : ITokenProvider
{
    /// <inheritdoc/>
    public Task<string?> GetAccessToken(CancellationToken cancellationToken = default) => Task.FromResult<string?>(token);

    /// <inheritdoc/>
    public Task<string?> Refresh(CancellationToken cancellationToken = default) => Task.FromResult<string?>(token);
}

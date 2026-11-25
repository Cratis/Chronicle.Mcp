// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using Cratis.Chronicle.Connections;

namespace Cratis.Chronicle.Mcp.Connection;

/// <summary>
/// Caches tokens only for their recorded destination, never reading CLI-owned token files.
/// </summary>
/// <param name="innerProvider">The provider authenticating against the configured destination.</param>
/// <param name="cachePath">The MCP-owned cache file path.</param>
/// <param name="destination">The normalized destination.</param>
public sealed class DestinationTokenProvider(ITokenProvider innerProvider, string cachePath, string destination) : ITokenProvider, IDisposable
{
    readonly SemaphoreSlim _lock = new(1, 1);

    /// <inheritdoc/>
    public Task<string?> GetAccessToken(CancellationToken cancellationToken = default) => GetToken(refresh: false, cancellationToken);

    /// <inheritdoc/>
    public Task<string?> Refresh(CancellationToken cancellationToken = default) => GetToken(refresh: true, cancellationToken);

    /// <inheritdoc/>
    public void Dispose()
    {
        _lock.Dispose();
        if (innerProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    async Task<string?> GetToken(bool refresh, CancellationToken cancellationToken)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (!refresh)
            {
                return ReadCache() ?? await AcquireToken(refresh: false, cancellationToken);
            }

            if (File.Exists(cachePath))
            {
                File.Delete(cachePath);
            }

            return await AcquireToken(refresh: true, cancellationToken);
        }
        finally
        {
            _lock.Release();
        }
    }

    string? ReadCache()
    {
        try
        {
            var cache = JsonSerializer.Deserialize<TokenCache>(File.ReadAllText(cachePath));
            return IsUsable(cache) ? cache!.AccessToken : null;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException)
        {
            return null;
        }
    }

    bool IsUsable(TokenCache? cache) =>
        cache is not null && cache.Destination == destination && cache.Expiry > DateTimeOffset.UtcNow;

    async Task<string?> AcquireToken(bool refresh, CancellationToken cancellationToken)
    {
        var token = refresh
            ? await innerProvider.Refresh(cancellationToken)
            : await innerProvider.GetAccessToken(cancellationToken);
        if (token is not null)
        {
            WriteCache(new TokenCache(token, DateTimeOffset.UtcNow.AddMinutes(55), destination));
        }

        return token;
    }

    void WriteCache(TokenCache cache)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(cachePath)!);
        var options = new FileStreamOptions
        {
            Mode = FileMode.Create,
            Access = FileAccess.Write
        };
        if (!OperatingSystem.IsWindows())
        {
            options.UnixCreateMode = UnixFileMode.UserRead | UnixFileMode.UserWrite;
        }

        using var stream = new FileStream(cachePath, options);
        JsonSerializer.Serialize(stream, cache);
    }

    sealed record TokenCache(string AccessToken, DateTimeOffset Expiry, string? Destination);
}

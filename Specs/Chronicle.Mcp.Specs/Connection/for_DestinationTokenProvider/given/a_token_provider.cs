// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using Cratis.Chronicle.Connections;
using NSubstitute;

namespace Cratis.Chronicle.Mcp.Connection.for_DestinationTokenProvider.given;

public class a_token_provider : Specs.given.a_configuration_directory
{
    protected const string Destination = "server.example:35000";
    protected ITokenProvider _inner;
    protected DestinationTokenProvider _provider;
    protected string _cachePath;
    protected string? _result;

    void Establish()
    {
        _inner = Substitute.For<ITokenProvider>();
        _inner.GetAccessToken().Returns("fresh-token");
        _inner.Refresh().Returns("refreshed-token");
        _cachePath = Path.Combine(_directory, "cache.json");
        _provider = new DestinationTokenProvider(_inner, _cachePath, Destination);
    }

    protected void Cache(string? destination, DateTimeOffset? expiry = null) =>
        File.WriteAllText(_cachePath, JsonSerializer.Serialize(new
        {
            AccessToken = "cached-token",
            Expiry = expiry ?? DateTimeOffset.UtcNow.AddHours(1),
            Destination = destination
        }));

    protected string? ReadDestination()
    {
        using var document = JsonDocument.Parse(File.ReadAllText(_cachePath));
        return document.RootElement.GetProperty("Destination").GetString();
    }

    void Destroy() => _provider.Dispose();
}

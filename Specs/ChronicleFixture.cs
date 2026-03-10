// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Cratis.Chronicle.Mcp.Api;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Networks;

namespace Cratis.Chronicle.Mcp.Specs;

/// <summary>
/// Shared fixture that starts a Chronicle Docker container for integration specs.
/// </summary>
#pragma warning disable CA1001 // Disposable fields are cleaned up in IAsyncLifetime.DisposeAsync
public class ChronicleFixture : IAsyncLifetime
#pragma warning restore CA1001
{
    INetwork _network = null!;
    IContainer _container = null!;
    HttpClient _httpClient = null!;

    /// <summary>
    /// Gets the <see cref="ChronicleApiClient"/> configured to talk to the running container.
    /// </summary>
    public ChronicleApiClient ApiClient { get; private set; } = null!;

    /// <summary>
    /// Gets the <see cref="ChronicleClient"/> configured to talk to the running container.
    /// </summary>
    public ChronicleClient Client { get; private set; } = null!;

    /// <inheritdoc/>
    public async Task InitializeAsync()
    {
        _network = new NetworkBuilder()
            .WithName($"chronicle-mcp-specs-{Guid.NewGuid():N}")
            .Build();
        await _network.CreateAsync();

        _container = new ContainerBuilder("cratis/chronicle:latest-development")
            .WithNetwork(_network)
            .WithHostname("chronicle")
            .WithPortBinding(0, 8080)
            .WithPortBinding(0, 35000)
            .WithWaitStrategy(
                Wait.ForUnixContainer()
                    .UntilHttpRequestIsSucceeded(r => r.ForPort(8080).ForPath("/health")))
            .Build();

        await _container.StartAsync();

        var httpPort = _container.GetMappedPublicPort(8080);
        var grpcPort = _container.GetMappedPublicPort(35000);

        var accessToken = await ObtainAccessToken(httpPort);
        _httpClient = new HttpClient { BaseAddress = new Uri($"http://localhost:{httpPort}") };
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        ApiClient = new ChronicleApiClient(_httpClient);

        var options = ChronicleOptions.FromConnectionString(
            $"chronicle://chronicle-dev-client:chronicle-dev-secret@localhost:{grpcPort}?disableTls=true");
        options.ManagementPort = httpPort;
        Client = new ChronicleClient(options);
    }

    /// <inheritdoc/>
    public async Task DisposeAsync()
    {
        _httpClient.Dispose();

        if (_container is not null)
        {
            await _container.DisposeAsync();
        }

        if (_network is not null)
        {
            await _network.DisposeAsync();
        }
    }

    /// <summary>
    /// Obtains an OAuth access token from the Chronicle token endpoint using client credentials.
    /// </summary>
    /// <param name="httpPort">The mapped HTTP port of the Chronicle container.</param>
    /// <returns>The access token string.</returns>
    static async Task<string> ObtainAccessToken(int httpPort)
    {
        using var tokenClient = new HttpClient();
        using var tokenRequest = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = "chronicle-dev-client",
            ["client_secret"] = "chronicle-dev-secret",
            ["scope"] = "openid"
        });

        var response = await tokenClient.PostAsync($"http://localhost:{httpPort}/connect/token", tokenRequest);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();

        return json.GetProperty("access_token").GetString()!;
    }
}

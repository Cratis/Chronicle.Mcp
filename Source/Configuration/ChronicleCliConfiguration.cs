// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cratis.Chronicle.Mcp.Configuration;

/// <summary>
/// Reads the Cratis CLI configuration stored in the user's home directory under <c>.cratis</c>.
/// </summary>
/// <remarks>
/// The format is compatible with the Cratis CLI: named contexts under a <c>contexts</c> dictionary,
/// with an <c>activeContext</c> pointer. The legacy flat format is also understood so that an MCP
/// server can read configurations written by older CLI versions.
/// </remarks>
public class ChronicleCliConfiguration
{
    /// <summary>
    /// The name of the default context.
    /// </summary>
    public const string DefaultContextName = "default";

    static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Gets or sets the name of the currently active context.
    /// </summary>
    public string? ActiveContext { get; set; }

    /// <summary>
    /// Gets or sets the named contexts.
    /// </summary>
    public IDictionary<string, ChronicleCliContext> Contexts { get; set; } = new Dictionary<string, ChronicleCliContext>();

    /// <summary>
    /// Gets the name of the active context, falling back to the default name.
    /// </summary>
    [JsonIgnore]
    public string ActiveContextName => ActiveContext ?? DefaultContextName;

    /// <summary>
    /// Gets the path to the configuration file.
    /// </summary>
    /// <returns>The full path to the config file.</returns>
    public static string GetConfigPath() =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".cratis", "config.json");

    /// <summary>
    /// Gets the path to the token cache file for the given cache key.
    /// </summary>
    /// <param name="key">The cache key to derive the file path from.</param>
    /// <returns>The full path to the token cache file.</returns>
    /// <remarks>
    /// The key should uniquely identify the context and credentials (e.g. <c>"{contextName}_{clientId}"</c>).
    /// This matches the CLI so tokens are shared between the CLI and the MCP server.
    /// </remarks>
    public static string GetTokenCachePath(string key)
    {
        var safeKey = string.Concat(key.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".cratis", "tokens", $"{safeKey}.token");
    }

    /// <summary>
    /// Loads the configuration from disk, returning defaults if the file does not exist.
    /// </summary>
    /// <returns>The loaded <see cref="ChronicleCliConfiguration"/>.</returns>
    public static ChronicleCliConfiguration Load()
    {
        var path = GetConfigPath();
        if (!File.Exists(path))
        {
            return new ChronicleCliConfiguration();
        }

        try
        {
            var json = File.ReadAllText(path);
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            // Detect legacy flat format: has "defaultServer" or "clientId" but no "contexts".
            if ((root.TryGetProperty("defaultServer", out _) || root.TryGetProperty("clientId", out _)) &&
                !root.TryGetProperty("contexts", out _))
            {
                return ReadLegacy(root);
            }

            return JsonSerializer.Deserialize<ChronicleCliConfiguration>(json, _jsonOptions) ?? new ChronicleCliConfiguration();
        }
        catch (JsonException)
        {
            // A malformed config file should not prevent the MCP server from falling back to
            // environment variables and built-in development defaults.
            return new ChronicleCliConfiguration();
        }
    }

    /// <summary>
    /// Gets the context with the given name, falling back to the active context, then the default.
    /// </summary>
    /// <param name="name">The optional context name to resolve. When <see langword="null"/>, the active context is used.</param>
    /// <returns>The matching <see cref="ChronicleCliContext"/>, or an empty context when none is found.</returns>
    public ChronicleCliContext GetContext(string? name = null)
    {
        var contextName = name ?? ActiveContextName;
        return Contexts.TryGetValue(contextName, out var ctx) ? ctx : new ChronicleCliContext();
    }

    static ChronicleCliConfiguration ReadLegacy(JsonElement root)
    {
        var ctx = new ChronicleCliContext
        {
            Server = GetStringProperty(root, "defaultServer"),
            EventStore = GetStringProperty(root, "defaultEventStore"),
            Namespace = GetStringProperty(root, "defaultNamespace"),
            ClientId = GetStringProperty(root, "clientId"),
            ClientSecret = GetStringProperty(root, "clientSecret"),
            AccessToken = GetStringProperty(root, "accessToken"),
            TokenExpiry = GetStringProperty(root, "tokenExpiry"),
            LoggedInUser = GetStringProperty(root, "loggedInUser")
        };

        return new ChronicleCliConfiguration
        {
            ActiveContext = DefaultContextName,
            Contexts = new Dictionary<string, ChronicleCliContext> { [DefaultContextName] = ctx }
        };
    }

    static string? GetStringProperty(JsonElement root, string name) =>
        root.TryGetProperty(name, out var prop) && prop.ValueKind == JsonValueKind.String
            ? prop.GetString()
            : null;
}

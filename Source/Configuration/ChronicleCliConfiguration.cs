// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cratis.Chronicle.Mcp.Configuration;

/// <summary>
/// Reads the Cratis CLI configuration stored in the user's home directory under <c language="csharp">.cratis</c>.
/// </summary>
/// <remarks>
/// The format is compatible with the Cratis CLI: named contexts under a <c language="csharp">contexts</c> dictionary,
/// with an <c language="csharp">activeContext</c> pointer. The legacy flat format is also understood so that an MCP
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
    /// The key should uniquely identify the context and credentials (e.g. <c language="csharp">"{contextName}_{clientId}"</c>).
    /// This identifies the CLI-owned cache. The MCP connection never reads these unscoped files.
    /// </remarks>
    public static string GetTokenCachePath(string key)
    {
        var safeKey = string.Concat(key.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".cratis", "tokens", $"{safeKey}.token");
    }

    /// <summary>
    /// Loads the configuration from an explicitly selected file.
    /// </summary>
    /// <param name="path">The absolute CLI configuration file path.</param>
    /// <returns>The loaded <see cref="ChronicleCliConfiguration"/>.</returns>
    public static ChronicleCliConfiguration Load(string path)
    {
        if (!Path.IsPathFullyQualified(path))
        {
            throw new ArgumentException("The CLI configuration file path must be absolute.", nameof(path));
        }

        var json = File.ReadAllText(path);
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        return IsLegacy(root)
            ? ReadLegacy(root)
            : JsonSerializer.Deserialize<ChronicleCliConfiguration>(json, _jsonOptions) ?? new ChronicleCliConfiguration();
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

    static bool IsLegacy(JsonElement root) =>
        !root.TryGetProperty("contexts", out _) && HasLegacySettings(root);

    static bool HasLegacySettings(JsonElement root) =>
        root.TryGetProperty("defaultServer", out _) || root.TryGetProperty("clientId", out _);

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
            TokenServer = GetStringProperty(root, "tokenServer"),
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

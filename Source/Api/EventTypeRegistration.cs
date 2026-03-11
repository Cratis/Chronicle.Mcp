// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Api;

/// <summary>
/// Represents the payload of an event type registration including its schema.
/// </summary>
/// <param name="Type">The event type information.</param>
/// <param name="Owner">The owner of the event type (0=None, 1=Client, 2=Server).</param>
/// <param name="Source">The source of the event type (0=Unknown, 1=Code, 2=User).</param>
/// <param name="Schema">The JSON schema of the event type.</param>
public record EventTypeRegistration(
    EventType Type,
    int Owner,
    int Source,
    string Schema)
{
    static readonly string[] _owners = ["None", "Client", "Server"];
    static readonly string[] _sources = ["Unknown", "Code", "User"];

    /// <summary>
    /// Gets the human-readable name of the owner.
    /// </summary>
    public string OwnerName => Owner >= 0 && Owner < _owners.Length ? _owners[Owner] : "Unknown";

    /// <summary>
    /// Gets the human-readable name of the source.
    /// </summary>
    public string SourceName => Source >= 0 && Source < _sources.Length ? _sources[Source] : "Unknown";
}

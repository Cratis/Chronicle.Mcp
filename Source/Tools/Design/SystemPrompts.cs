// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using ModelContextProtocol.Server;

namespace Cratis.Chronicle.Mcp.Tools.Design;

/// <summary>
/// Prompts that package the system-analysis workflows so MCP clients can offer them directly
/// (for example as slash commands), pairing the analysis tools with the narrative work the
/// client's model performs.
/// </summary>
[McpServerPromptType]
public static class SystemPrompts
{
    /// <summary>
    /// Prompt for describing the system in an event store and telling its story.
    /// </summary>
    /// <param name="eventStore">The event store to describe; empty uses the configured default.</param>
    /// <returns>The prompt text.</returns>
    [McpServerPrompt(Name = "describe_system")]
    [Description("Describe what the system in an event store is and is for, and tell its story — deduced from the event types, their names and properties, and the read models and automations around them.")]
    public static string DescribeSystem(
        [Description("The event store to describe. Leave empty to use the configured default.")] string? eventStore = null) =>
        $"Call the describe_system tool{ForEventStore(eventStore)}. From its result, produce two things:\n" +
        "1. A description of what this system is and what it is for, written in the domain's own language — " +
        "deduce the domain from the entity names, event vocabulary, and property names.\n" +
        "2. The story of the system: walk through each entity's life from the events — how it comes into being, " +
        "what happens to it, and how it ends — and weave in what the business watches (read surfaces) and what " +
        "the system does on its own (automations). Follow the narrative guidance carried in the result.";

    /// <summary>
    /// Prompt for suggesting the next event types to introduce for a system.
    /// </summary>
    /// <param name="eventStore">The event store to analyze; empty uses the configured default.</param>
    /// <returns>The prompt text.</returns>
    [McpServerPrompt(Name = "suggest_next_event_types")]
    [Description("Suggest which event types to introduce next for the system in an event store, grounded in gaps found in the registered event types and refined with domain knowledge.")]
    public static string SuggestNextEventTypes(
        [Description("The event store to analyze. Leave empty to use the configured default.")] string? eventStore = null) =>
        $"Call the suggest_next_event_types tool{ForEventStore(eventStore)}. Present each suggestion with the gap " +
        "it closes, refine the names to the domain's ubiquitous language, and discard any suggestion the business " +
        "genuinely would not need — explain why in each case. Follow the guidance carried in the result, and keep " +
        "every proposed event past tense, single purpose, and free of nullable properties.";

    /// <summary>
    /// Prompt for answering a natural-language query with an ad-hoc projection.
    /// </summary>
    /// <param name="query">The natural-language question to answer (e.g. "show all employees with all details").</param>
    /// <param name="eventStore">The event store to query; empty uses the configured default.</param>
    /// <returns>The prompt text.</returns>
    [McpServerPrompt(Name = "query_system")]
    [Description("Answer a natural-language question about the data in an event store — resolves which event types are involved and materializes the answer with an ad-hoc projection, without registering anything.")]
    public static string QuerySystem(
        [Description("The natural-language question to answer, e.g. 'show all employees with all details'.")] string query,
        [Description("The event store to query. Leave empty to use the configured default.")] string? eventStore = null) =>
        $"Answer this question about the system{ForEventStore(eventStore)}: \"{query}\".\n" +
        "1. Call describe_system (or generate_event_catalog) to learn the entities and their event types.\n" +
        "2. Decide which event types carry the facts the question asks about — include every event type that " +
        "contributes details, and identify any termination events that should remove instances from the answer.\n" +
        "3. Call run_ad_hoc_projection with those event types (and removedWith for the termination events).\n" +
        "4. Present the instances in the domain's language — a table when they share a shape — and follow the " +
        "guidance carried in the result. If the question recurs, suggest making the projection permanent with scaffold_read_model.";

    static string ForEventStore(string? eventStore) =>
        string.IsNullOrWhiteSpace(eventStore) ? string.Empty : $" for the '{eventStore}' event store";
}

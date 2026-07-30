// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Tools.Design;

/// <summary>
/// Deduces structure from event type names. Event names are the ubiquitous language of an
/// event-sourced system — a well-named event is a past-tense fact about a subject, so the name
/// itself carries which entity it belongs to and where in that entity's lifecycle it sits.
/// </summary>
public static class EventNameMorphology
{
    static readonly HashSet<string> _particles = new(StringComparer.OrdinalIgnoreCase)
    {
        "In", "Out", "Up", "Down", "On", "Off", "Over", "Back", "To", "From"
    };

    static readonly HashSet<string> _irregularVerbs = new(StringComparer.OrdinalIgnoreCase)
    {
        "Sent", "Built", "Made", "Won", "Lost", "Sold", "Bought", "Paid", "Held", "Left", "Kept",
        "Met", "Set", "Put", "Reset", "Split", "Shut", "Begun", "Drawn", "Withdrawn", "Given",
        "Taken", "Chosen", "Frozen", "Broken", "Hidden", "Written", "Overridden", "Done", "Undone",
        "Gone", "Born", "Torn", "Worn", "Sworn", "Shown", "Known", "Grown", "Flown", "Thrown",
        "Sung", "Rung", "Hung", "Struck", "Stuck", "Spent", "Lent", "Bent", "Burnt", "Learnt",
        "Meant", "Felt", "Dealt", "Found", "Bound", "Wound", "Caught", "Taught", "Brought", "Sought"
    };

    static readonly Dictionary<string, LifecycleStage> _verbStages = new(StringComparer.OrdinalIgnoreCase)
    {
        // Creation — the entity comes into existence.
        ["Registered"] = LifecycleStage.Creation,
        ["Created"] = LifecycleStage.Creation,
        ["Added"] = LifecycleStage.Creation,
        ["Opened"] = LifecycleStage.Creation,
        ["Started"] = LifecycleStage.Creation,
        ["Placed"] = LifecycleStage.Creation,
        ["Submitted"] = LifecycleStage.Creation,
        ["Imported"] = LifecycleStage.Creation,
        ["Onboarded"] = LifecycleStage.Creation,
        ["Initialized"] = LifecycleStage.Creation,
        ["Initiated"] = LifecycleStage.Creation,
        ["Established"] = LifecycleStage.Creation,
        ["Enrolled"] = LifecycleStage.Creation,
        ["Joined"] = LifecycleStage.Creation,
        ["Founded"] = LifecycleStage.Creation,
        ["Launched"] = LifecycleStage.Creation,
        ["Provisioned"] = LifecycleStage.Creation,
        ["Issued"] = LifecycleStage.Creation,
        ["SignedUp"] = LifecycleStage.Creation,
        ["Hired"] = LifecycleStage.Creation,
        ["Appointed"] = LifecycleStage.Creation,
        ["Admitted"] = LifecycleStage.Creation,

        // Termination — the entity's life ends.
        ["Removed"] = LifecycleStage.Termination,
        ["Deleted"] = LifecycleStage.Termination,
        ["Closed"] = LifecycleStage.Termination,
        ["Cancelled"] = LifecycleStage.Termination,
        ["Canceled"] = LifecycleStage.Termination,
        ["Archived"] = LifecycleStage.Termination,
        ["Terminated"] = LifecycleStage.Termination,
        ["Ended"] = LifecycleStage.Termination,
        ["Completed"] = LifecycleStage.Termination,
        ["Finished"] = LifecycleStage.Termination,
        ["Expired"] = LifecycleStage.Termination,
        ["Revoked"] = LifecycleStage.Termination,
        ["Unregistered"] = LifecycleStage.Termination,
        ["Erased"] = LifecycleStage.Termination,
        ["Withdrawn"] = LifecycleStage.Termination,
        ["Retired"] = LifecycleStage.Termination,
        ["Discarded"] = LifecycleStage.Termination,
        ["Abandoned"] = LifecycleStage.Termination,
        ["Dissolved"] = LifecycleStage.Termination,
        ["Fired"] = LifecycleStage.Termination,
        ["Dismissed"] = LifecycleStage.Termination,
        ["Resigned"] = LifecycleStage.Termination,

        // Correction — an earlier fact is corrected or reversed.
        ["Corrected"] = LifecycleStage.Correction,
        ["Adjusted"] = LifecycleStage.Correction,
        ["Amended"] = LifecycleStage.Correction,
        ["Reverted"] = LifecycleStage.Correction,
        ["Undone"] = LifecycleStage.Correction,
        ["Redacted"] = LifecycleStage.Correction,
        ["Rectified"] = LifecycleStage.Correction,

        // Mutation — existing state changes.
        ["Changed"] = LifecycleStage.Mutation,
        ["Updated"] = LifecycleStage.Mutation,
        ["Renamed"] = LifecycleStage.Mutation,
        ["Modified"] = LifecycleStage.Mutation,
        ["Set"] = LifecycleStage.Mutation,
        ["Reset"] = LifecycleStage.Mutation,
        ["Assigned"] = LifecycleStage.Mutation,
        ["Reassigned"] = LifecycleStage.Mutation,
        ["Unassigned"] = LifecycleStage.Mutation,
        ["Moved"] = LifecycleStage.Mutation,
        ["Transferred"] = LifecycleStage.Mutation,
        ["Increased"] = LifecycleStage.Mutation,
        ["Decreased"] = LifecycleStage.Mutation,
        ["Enabled"] = LifecycleStage.Mutation,
        ["Disabled"] = LifecycleStage.Mutation,
        ["Activated"] = LifecycleStage.Mutation,
        ["Reactivated"] = LifecycleStage.Mutation,
        ["Deactivated"] = LifecycleStage.Mutation,
        ["Suspended"] = LifecycleStage.Mutation,
        ["Resumed"] = LifecycleStage.Mutation,
        ["Paused"] = LifecycleStage.Mutation,
        ["Promoted"] = LifecycleStage.Mutation,
        ["Demoted"] = LifecycleStage.Mutation,
        ["Upgraded"] = LifecycleStage.Mutation,
        ["Downgraded"] = LifecycleStage.Mutation,
        ["Approved"] = LifecycleStage.Mutation,
        ["Rejected"] = LifecycleStage.Mutation,
        ["Granted"] = LifecycleStage.Mutation,
        ["Denied"] = LifecycleStage.Mutation,
        ["Locked"] = LifecycleStage.Mutation,
        ["Unlocked"] = LifecycleStage.Mutation,
        ["Restored"] = LifecycleStage.Mutation
    };

    /// <summary>
    /// Splits an event type name into its words, handling PascalCase, camelCase, acronyms, and
    /// kebab/snake/dot separators.
    /// </summary>
    /// <param name="name">The event type name to split.</param>
    /// <returns>The words making up the name, in order.</returns>
    public static IReadOnlyList<string> SplitWords(string name)
    {
        var words = new List<string>();
        var current = new System.Text.StringBuilder();

        for (var index = 0; index < name.Length; index++)
        {
            var character = name[index];
            if (character is '-' or '_' or '.' or ' ')
            {
                Flush(words, current);
                continue;
            }

            if (char.IsUpper(character) && current.Length > 0)
            {
                var previous = name[index - 1];
                var startsNewWord = char.IsLower(previous) || char.IsDigit(previous) ||
                    (char.IsUpper(previous) && index + 1 < name.Length && char.IsLower(name[index + 1]));

                if (startsNewWord)
                {
                    Flush(words, current);
                }
            }

            current.Append(character);
        }

        Flush(words, current);
        return words;
    }

    /// <summary>
    /// Infers the entity (the subject noun phrase) an event type belongs to from its name.
    /// </summary>
    /// <param name="eventTypeName">The event type name.</param>
    /// <returns>The inferred entity name, or the full name when no verb can be identified.</returns>
    public static string InferEntity(string eventTypeName)
    {
        var words = SplitWords(eventTypeName);
        var verbIndex = FindVerbIndex(words);

        if (verbIndex <= 0)
        {
            return Capitalize(string.Concat(words));
        }

        return Capitalize(string.Concat(words.Take(verbIndex)));
    }

    /// <summary>
    /// Classifies where in its entity's lifecycle an event sits, based on the verb in its name.
    /// </summary>
    /// <param name="eventTypeName">The event type name.</param>
    /// <returns>The deduced <see cref="LifecycleStage"/>.</returns>
    public static LifecycleStage Classify(string eventTypeName)
    {
        var words = SplitWords(eventTypeName);
        var verbIndex = FindVerbIndex(words);

        if (verbIndex < 0)
        {
            return LifecycleStage.Activity;
        }

        var phrase = string.Concat(words.Skip(verbIndex));
        if (_verbStages.TryGetValue(phrase, out var phraseStage))
        {
            return phraseStage;
        }

        return _verbStages.TryGetValue(words[verbIndex], out var verbStage) ? verbStage : LifecycleStage.Activity;
    }

    static int FindVerbIndex(IReadOnlyList<string> words)
    {
        if (words.Count == 0)
        {
            return -1;
        }

        var lastIndex = words.Count - 1;
        if (_particles.Contains(words[lastIndex]) && words.Count > 1)
        {
            lastIndex--;
        }

        return IsVerb(words[lastIndex]) ? lastIndex : -1;
    }

    static bool IsVerb(string word) =>
        (word.Length > 2 && word.EndsWith("ed", StringComparison.OrdinalIgnoreCase)) ||
        _irregularVerbs.Contains(word);

    static string Capitalize(string value) =>
        value.Length == 0 ? value : string.Concat(char.ToUpperInvariant(value[0]).ToString(), value[1..]);

    static void Flush(List<string> words, System.Text.StringBuilder current)
    {
        if (current.Length > 0)
        {
            words.Add(current.ToString());
            current.Clear();
        }
    }
}

// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Tools.Design;

/// <summary>
/// A proposed read model + model-bound projection, scaffolded from the real schema of the chosen event
/// types. Always a proposal to review and apply — never a silent write into a codebase.
/// </summary>
/// <param name="ReadModelName">The name of the proposed read model.</param>
/// <param name="Namespace">The C# namespace the scaffold was generated into.</param>
/// <param name="Code">The generated C# code for the read model and its model-bound projection, or null when it could not be grounded.</param>
/// <param name="Fields">The fields chosen for the read model, each traced back to the event it came from.</param>
/// <param name="ResolvedEventTypes">The requested event types that were found in the store and used.</param>
/// <param name="UnresolvedEventTypes">The requested event types that are not registered — grounding failed for these.</param>
/// <param name="ExistingProjections">Existing projections that already read these events or target this read model, so a duplicate is not created blindly.</param>
/// <param name="Notes">Human-readable notes and caveats about the scaffold.</param>
public record ReadModelScaffold(
    string ReadModelName,
    string Namespace,
    string? Code,
    IReadOnlyList<ScaffoldedField> Fields,
    IReadOnlyList<string> ResolvedEventTypes,
    IReadOnlyList<string> UnresolvedEventTypes,
    IReadOnlyList<ExistingProjectionMatch> ExistingProjections,
    IReadOnlyList<string> Notes);

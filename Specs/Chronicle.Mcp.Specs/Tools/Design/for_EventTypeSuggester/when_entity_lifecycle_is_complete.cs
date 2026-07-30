// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Mcp.Tools.Design;

namespace Cratis.Chronicle.Mcp.Specs.Tools.Design.for_EventTypeSuggester;

public class when_entity_lifecycle_is_complete : Specification
{
    static readonly EventSchemaProperty _name = new("Name", "string", "string", null, true, null);

    IReadOnlyList<EventTypeSuggestion> _result;

    void Because() => _result = EventTypeSuggester.Suggest(
    [
        new SystemEntity("Author",
        [
            new SystemEntityEvent("AuthorRegistered", LifecycleStage.Creation, [_name]),
            new SystemEntityEvent("AuthorNameChanged", LifecycleStage.Mutation, [_name]),
            new SystemEntityEvent("AuthorRemoved", LifecycleStage.Termination, [])
        ])
    ]);

    [Fact] void should_have_nothing_to_suggest() => _result.ShouldBeEmpty();
}

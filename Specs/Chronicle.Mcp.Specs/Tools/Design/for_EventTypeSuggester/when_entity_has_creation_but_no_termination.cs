// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Mcp.Tools.Design;

namespace Cratis.Chronicle.Mcp.Specs.Tools.Design.for_EventTypeSuggester;

public class when_entity_has_creation_but_no_termination : Specification
{
    IReadOnlyList<EventTypeSuggestion> _result;

    void Because() => _result = EventTypeSuggester.Suggest(
    [
        new SystemEntity("Author",
        [
            new SystemEntityEvent("AuthorRegistered", LifecycleStage.Creation, [])
        ])
    ]);

    [Fact] void should_suggest_a_termination_event() => _result.Single().SuggestedName.ShouldEqual("AuthorRemoved");
    [Fact] void should_target_the_entity() => _result.Single().Entity.ShouldEqual("Author");
    [Fact] void should_fill_the_termination_stage() => _result.Single().Stage.ShouldEqual(LifecycleStage.Termination);
    [Fact] void should_explain_the_gap() => _result.Single().Rationale.ShouldContain("no end");
}

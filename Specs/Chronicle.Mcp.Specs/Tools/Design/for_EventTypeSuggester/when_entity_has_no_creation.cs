// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Mcp.Tools.Design;

namespace Cratis.Chronicle.Mcp.Specs.Tools.Design.for_EventTypeSuggester;

public class when_entity_has_no_creation : Specification
{
    IReadOnlyList<EventTypeSuggestion> _result;

    void Because() => _result = EventTypeSuggester.Suggest(
    [
        new SystemEntity("Order",
        [
            new SystemEntityEvent("OrderCancelled", LifecycleStage.Termination, [])
        ])
    ]);

    [Fact] void should_suggest_a_creation_event() => _result.Single().SuggestedName.ShouldEqual("OrderCreated");
    [Fact] void should_fill_the_creation_stage() => _result.Single().Stage.ShouldEqual(LifecycleStage.Creation);
    [Fact] void should_name_the_first_recorded_fact_in_the_rationale() => _result.Single().Rationale.ShouldContain("OrderCancelled");
}

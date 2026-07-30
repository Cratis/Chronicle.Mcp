// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Mcp.Tools.Design;

namespace Cratis.Chronicle.Mcp.Specs.Tools.Design.for_EventTypeSuggester;

public class when_creation_property_is_never_mutated : Specification
{
    static readonly EventSchemaProperty _customerId = new("CustomerId", "string", "Guid", "guid", true, null);
    static readonly EventSchemaProperty _email = new("Email", "string", "string", null, true, null);
    static readonly EventSchemaProperty _name = new("Name", "string", "string", null, true, null);

    IReadOnlyList<EventTypeSuggestion> _result;

    void Because() => _result = EventTypeSuggester.Suggest(
    [
        new SystemEntity("Customer",
        [
            new SystemEntityEvent("CustomerRegistered", LifecycleStage.Creation, [_customerId, _email, _name]),
            new SystemEntityEvent("CustomerNameChanged", LifecycleStage.Mutation, [_name]),
            new SystemEntityEvent("CustomerRemoved", LifecycleStage.Termination, [])
        ])
    ]);

    [Fact] void should_suggest_a_change_event_for_the_untouched_property() => _result.Single().SuggestedName.ShouldEqual("CustomerEmailChanged");
    [Fact] void should_not_suggest_for_identifier_properties() => _result.ShouldNotContain(suggestion => suggestion.SuggestedName.Contains("CustomerId"));
    [Fact] void should_not_suggest_for_properties_already_mutated() => _result.ShouldNotContain(suggestion => suggestion.SuggestedName.Contains("Name"));
    [Fact] void should_carry_the_property_on_the_suggestion() => _result.Single().SuggestedProperties.Single().ShouldEqual(_email);
    [Fact] void should_name_the_creation_event_in_the_rationale() => _result.Single().Rationale.ShouldContain("CustomerRegistered");
}

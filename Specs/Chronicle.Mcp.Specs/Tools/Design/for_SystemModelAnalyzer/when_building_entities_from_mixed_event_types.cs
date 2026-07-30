// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Mcp.Tools.Design;

namespace Cratis.Chronicle.Mcp.Specs.Tools.Design.for_SystemModelAnalyzer;

public class when_building_entities_from_mixed_event_types : Specification
{
    static readonly EventSchemaProperty _name = new("Name", "string", "string", null, true, null);

    IReadOnlyList<SystemEntity> _result;

    void Because() => _result = SystemModelAnalyzer.BuildEntities(new Dictionary<string, IReadOnlyList<EventSchemaProperty>>
    {
        ["AuthorRemoved"] = [],
        ["AuthorRegistered"] = [_name],
        ["AuthorNameChanged"] = [_name],
        ["BookAdded"] = []
    });

    [Fact] void should_deduce_two_entities() => _result.Count.ShouldEqual(2);
    [Fact] void should_order_entities_alphabetically() => _result.Select(entity => entity.Name).ShouldEqual("Author", "Book");
    [Fact] void should_cluster_all_author_events() => _result[0].Events.Count.ShouldEqual(3);
    [Fact] void should_order_events_by_lifecycle_stage() => _result[0].Events.Select(entityEvent => entityEvent.EventType).ShouldEqual("AuthorRegistered", "AuthorNameChanged", "AuthorRemoved");
    [Fact] void should_carry_the_schema_properties() => _result[0].Events[0].Properties.Single().ShouldEqual(_name);
    [Fact] void should_classify_each_event() => _result[0].Events[0].Stage.ShouldEqual(LifecycleStage.Creation);
}

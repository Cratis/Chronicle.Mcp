// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Tools.for_ProjectionStepTools;

public class when_stepping_a_projection : given.a_projection_step
{
    void Establish() => _response.Data = [Snapshot(1, """{"name":"Ada"}""", """{"name":"Ada"}"""), Snapshot(2, """{"name":"Ada","balance":10}""")];

    async Task Because() => _result = await Step();

    [Fact] void should_ask_for_event_grouping() => _request.Grouping.ShouldEqual("Event");
    [Fact] void should_ask_for_the_read_model_identifier() => _request.ReadModel.ShouldEqual(ReadModelIdentifier);
    [Fact] void should_ask_for_the_key() => _request.ReadModelKey.ShouldEqual(Key);
    [Fact] void should_default_to_the_event_log() => _request.EventSequenceId.ShouldEqual("event-log");
    [Fact] void should_resolve_the_configured_event_store() => _request.EventStore.ShouldEqual("ConfiguredStore");
    [Fact] void should_resolve_the_configured_namespace() => _request.Namespace.ShouldEqual("ConfiguredTenant");
    [Fact] void should_be_ok() => _result.Status.ShouldEqual(ProjectionStepStatus.Ok);
    [Fact] void should_count_the_events() => _result.TotalEvents.ShouldEqual(2);
    [Fact] void should_number_the_steps_in_server_order() => _result.Steps.Select(step => step.Step).ShouldContainOnly(1, 2);
    [Fact] void should_keep_the_events_in_server_order() => _result.Steps.Select(step => step.Events.Single().SequenceNumber).ShouldContainOnly(1UL, 2UL);
    [Fact] void should_carry_the_event_type() => _result.Steps[0].Events[0].EventType.ShouldEqual("event-1");
    [Fact] void should_carry_the_event_content_as_json() => _result.Steps[0].Events[0].Content!.Value.GetProperty("name").GetString().ShouldEqual("Ada");
    [Fact] void should_carry_each_instance_as_json() => _result.Steps[1].Instance!.Value.GetProperty("balance").GetInt32().ShouldEqual(10);
    [Fact] void should_derive_the_properties_created_by_the_first_event() => _result.Steps[0].ChangedProperties.ShouldContainOnly("name");
    [Fact] void should_derive_the_properties_changed_by_the_second_event() => _result.Steps[1].ChangedProperties.ShouldContainOnly("balance");
    [Fact] void should_not_mark_a_changing_step_unchanged() => _result.Steps[1].Unchanged.ShouldBeFalse();
    [Fact] void should_say_differences_are_derived() => _result.Note.ShouldContain("derived");
}

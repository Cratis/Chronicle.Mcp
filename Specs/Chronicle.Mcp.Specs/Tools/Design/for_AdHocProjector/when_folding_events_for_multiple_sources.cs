// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using Cratis.Chronicle.Mcp.Tools.Design;

namespace Cratis.Chronicle.Mcp.Specs.Tools.Design.for_AdHocProjector;

public class when_folding_events_for_multiple_sources : Specification
{
    static readonly DateTimeOffset _first = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
    static readonly DateTimeOffset _second = new(2026, 2, 1, 0, 0, 0, TimeSpan.Zero);

    IReadOnlyList<AdHocProjectionInstance> _result;

    static JsonElement Json(string json) => JsonSerializer.Deserialize<JsonElement>(json);

    void Because() => _result = AdHocProjector.Fold(
    [
        new AdHocEvent("employee-2", "EmployeeHired", 3, _second, Json("""{"Name":"Kari","Title":"Engineer"}""")),
        new AdHocEvent("employee-1", "EmployeeHired", 0, _first, Json("""{"Name":"Ola","Title":"Engineer"}""")),
        new AdHocEvent("employee-1", "EmployeePromoted", 5, _second, Json("""{"Title":"Architect"}"""))
    ]);

    [Fact] void should_materialize_one_instance_per_event_source() => _result.Count.ShouldEqual(2);
    [Fact] void should_order_instances_by_event_source_id() => _result.Select(instance => instance.EventSourceId).ShouldEqual("employee-1", "employee-2");
    [Fact] void should_let_later_events_overwrite_property_values() => _result[0].Properties["Title"].GetString().ShouldEqual("Architect");
    [Fact] void should_keep_properties_untouched_by_later_events() => _result[0].Properties["Name"].GetString().ShouldEqual("Ola");
    [Fact] void should_track_the_last_folded_sequence_number() => _result[0].LastEventSequenceNumber.ShouldEqual(5UL);
    [Fact] void should_track_when_the_last_event_occurred() => _result[0].LastOccurred.ShouldEqual(_second);
}

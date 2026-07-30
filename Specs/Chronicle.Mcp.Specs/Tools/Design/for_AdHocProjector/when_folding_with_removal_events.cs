// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using Cratis.Chronicle.Mcp.Tools.Design;

namespace Cratis.Chronicle.Mcp.Specs.Tools.Design.for_AdHocProjector;

public class when_folding_with_removal_events : Specification
{
    IReadOnlyList<AdHocProjectionInstance> _result;

    static JsonElement Json(string json) => JsonSerializer.Deserialize<JsonElement>(json);

    void Because() => _result = AdHocProjector.Fold(
    [
        new AdHocEvent("employee-1", "EmployeeHired", 0, null, Json("""{"Name":"Ola"}""")),
        new AdHocEvent("employee-2", "EmployeeHired", 1, null, Json("""{"Name":"Kari"}""")),
        new AdHocEvent("employee-1", "EmployeeTerminated", 2, null, null),
        new AdHocEvent("employee-3", "EmployeeHired", 3, null, Json("""{"Name":"Per"}""")),
        new AdHocEvent("employee-3", "EmployeeTerminated", 4, null, null),
        new AdHocEvent("employee-3", "EmployeeHired", 5, null, Json("""{"Name":"Per Again"}"""))
    ],
    ["EmployeeTerminated"]);

    [Fact] void should_drop_removed_instances() => _result.ShouldNotContain(instance => instance.EventSourceId == "employee-1");
    [Fact] void should_keep_instances_that_were_not_removed() => _result.ShouldContain(instance => instance.EventSourceId == "employee-2");
    [Fact] void should_let_later_events_recreate_a_removed_instance() => _result.Single(instance => instance.EventSourceId == "employee-3").Properties["Name"].GetString().ShouldEqual("Per Again");
}

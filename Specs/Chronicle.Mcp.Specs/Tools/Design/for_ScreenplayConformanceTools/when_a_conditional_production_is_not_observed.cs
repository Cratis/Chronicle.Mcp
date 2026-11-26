// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Tools.Design.for_ScreenplayConformanceTools;

public class when_a_conditional_production_is_not_observed : a_namespace_audit
{
    void Establish()
    {
        Argument("modelSource", Source.Replace(
            "produces Registered\n          for id\n          name = name",
            "produces when name == \"first\"\n          Registered\n            for id\n            name = name",
            StringComparison.Ordinal));
        _metadata.Clear();
        _metadata.AddRange([Append(0, "Registered"), Append(1, "Registered"), Append(2, "Registered")]);
    }

    Task Because() => Audit();

    [Fact]
    void should_not_treat_conditional_logic_as_observed_execution() =>
        Body.GetProperty("findings").EnumerateArray().Any(item => item.GetProperty("outcome").GetString() == "Unknown" &&
            item.TryGetProperty("reason", out var reason) && reason.GetString() == "ConditionalProduction").ShouldBeTrue();
}

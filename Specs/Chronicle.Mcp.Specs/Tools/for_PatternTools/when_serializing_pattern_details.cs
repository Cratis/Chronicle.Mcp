// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using Cratis.Chronicle.Contracts.Patterns;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using NSubstitute;

namespace Cratis.Chronicle.Mcp.Tools.for_PatternTools;

public class when_serializing_pattern_details : given.a_pattern_query
{
    ServiceProvider _provider;
    JsonElement _pattern;

    void Establish()
    {
        _response.Data =
        [
            new BehaviorPatternDetailsResponse
            {
                Id = "PatternId",
                GroupingKey = "Order/Placed",
                Facets = new Dictionary<string, string> { ["Action"] = "OrderPlaced", ["CustomFacet"] = "Value" },
                Confidence = 0.85,
                Support = 0.4,
                Occurrences = 9007199254740993L,
                Weight = 0.7,
                Specificity = 2,
                FirstSeen = new() { Value = "2026-01-02T03:04:05.0000000+02:00" },
                LastSeen = new() { Value = "2026-02-03T04:05:06.0000000-03:00" }
            }
        ];
        _provider = new ServiceCollection().AddSingleton(_services).AddSingleton(_configuration).BuildServiceProvider();
    }

    async Task Because()
    {
        var tool = McpServerTool.Create(typeof(PatternTools).GetMethod(nameof(PatternTools.GetPatterns))!, target: null, new() { Services = _provider });
        var result = await tool.InvokeAsync(new RequestContext<CallToolRequestParams>(
            Substitute.For<McpServer>(),
            new() { Method = "tools/call" },
            new()
            {
                Name = "get_patterns",
                Arguments = new Dictionary<string, JsonElement>
                {
                    ["groupingKey"] = JsonSerializer.SerializeToElement("Order/Placed"),
                    ["context"] = JsonSerializer.SerializeToElement(_context)
                }
            })
        {
            Services = _provider
        });
        _pattern = JsonSerializer.Deserialize<JsonElement>(result.Content.OfType<TextContentBlock>().Single().Text)[0];
    }

    [Fact] void should_preserve_all_ten_fields() => _pattern.EnumerateObject().Count().ShouldEqual(10);
    [Fact] void should_preserve_id() => _pattern.GetProperty("id").GetString().ShouldEqual("PatternId");
    [Fact] void should_preserve_grouping_key() => _pattern.GetProperty("groupingKey").GetString().ShouldEqual("Order/Placed");
    [Fact] void should_preserve_action_as_a_facet() => _pattern.GetProperty("facets").GetProperty("Action").GetString().ShouldEqual("OrderPlaced");
    [Fact] void should_preserve_arbitrary_facets() => _pattern.GetProperty("facets").GetProperty("CustomFacet").GetString().ShouldEqual("Value");
    [Fact] void should_preserve_confidence() => _pattern.GetProperty("confidence").GetDouble().ShouldEqual(0.85);
    [Fact] void should_preserve_support() => _pattern.GetProperty("support").GetDouble().ShouldEqual(0.4);
    [Fact] void should_preserve_int64_occurrences() => _pattern.GetProperty("occurrences").GetInt64().ShouldEqual(9007199254740993L);
    [Fact] void should_preserve_weight() => _pattern.GetProperty("weight").GetDouble().ShouldEqual(0.7);
    [Fact] void should_preserve_specificity() => _pattern.GetProperty("specificity").GetInt32().ShouldEqual(2);
    [Fact] void should_preserve_first_seen_with_its_offset() => _pattern.GetProperty("firstSeen").GetProperty("value").GetString().ShouldEqual("2026-01-02T03:04:05.0000000+02:00");
    [Fact] void should_preserve_last_seen_with_its_offset() => _pattern.GetProperty("lastSeen").GetProperty("value").GetString().ShouldEqual("2026-02-03T04:05:06.0000000-03:00");

    void Destroy() => _provider.Dispose();
}

// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using Cratis.Chronicle.Mcp.Tools;

namespace Cratis.Chronicle.Mcp.Specs.Tools.for_FailedPartitionTools.when_explaining_a_failed_partition;

public class with_compliance_filtered_content : given.a_failed_partition_query
{
    const string Sanitized = "{\"name\":\"[REDACTED]\"}";

    string _serialized;

    void Establish()
    {
        var current = EventAt(1000);
        current.Content = Sanitized;
        current.OriginalContent = "synthetic-marker-original";
        current.Revisions = [new() { Content = "synthetic-marker-revision" }];
        current.GenerationalContent = [new(1, "synthetic-marker-generation")];
        var erased = EventAt(900);
        erased.Content = "synthetic-marker-erased";
        erased.ErasedProperties = ["name"];
        var withheld = EventAt(4);
        withheld.Content = string.Empty;
        withheld.OriginalContent = "synthetic-marker-withheld";
        _page.Data.Events = [current, erased, withheld];
    }

    async Task Because()
    {
        _result = await FailedPartitionTools.ExplainFailedPartition(_services, _configuration, FailureId);
        _serialized = JsonSerializer.Serialize(_result);
    }

    [Fact] void should_preserve_sanitized_current_content() => _result.FailedEvent!.Content.ShouldEqual(Sanitized);
    [Fact] void should_omit_content_when_erased_paths_are_reported() => (_result.Context.Events[1].ContentStatus, _result.Context.Events[1].Content).ShouldEqual(("redacted", null));
    [Fact] void should_leave_withheld_content_unavailable() => (_result.Context.Events[0].ContentStatus, _result.Context.Events[0].Content).ShouldEqual(("unavailable", null));
    [Fact] void should_never_project_alternate_or_erased_content() => Assert.DoesNotContain("synthetic-marker", _serialized, StringComparison.Ordinal);
}

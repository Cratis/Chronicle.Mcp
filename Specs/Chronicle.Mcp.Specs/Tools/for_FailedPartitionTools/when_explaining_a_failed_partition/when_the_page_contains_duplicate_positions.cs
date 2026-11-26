// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Mcp.Tools;

namespace Cratis.Chronicle.Mcp.Specs.Tools.for_FailedPartitionTools.when_explaining_a_failed_partition;

public class when_the_page_contains_duplicate_positions : given.a_failed_partition_query
{
    Exception _error;

    void Establish() => _page.Data.Events = [EventAt(1000), EventAt(1000)];

    async Task Because() => _error = await Catch.Exception(() => FailedPartitionTools.ExplainFailedPartition(_services, _configuration, FailureId));

    [Fact] void should_not_trust_the_malformed_page() => Assert.IsType<InvalidOperationException>(_error);
}

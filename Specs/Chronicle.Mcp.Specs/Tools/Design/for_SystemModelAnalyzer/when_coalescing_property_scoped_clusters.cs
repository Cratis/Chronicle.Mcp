// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Mcp.Tools.Design;

namespace Cratis.Chronicle.Mcp.Specs.Tools.Design.for_SystemModelAnalyzer;

public class when_coalescing_property_scoped_clusters : Specification
{
    IReadOnlyList<SystemEntity> _withParent;
    IReadOnlyList<SystemEntity> _withoutParent;

    void Because()
    {
        _withParent = SystemModelAnalyzer.BuildEntities(new Dictionary<string, IReadOnlyList<EventSchemaProperty>>
        {
            ["CustomerRegistered"] = [],
            ["CustomerAddressUpdated"] = []
        });

        _withoutParent = SystemModelAnalyzer.BuildEntities(new Dictionary<string, IReadOnlyList<EventSchemaProperty>>
        {
            ["CustomerAddressUpdated"] = []
        });
    }

    [Fact] void should_fold_the_property_scoped_cluster_into_its_parent() => _withParent.Single().Name.ShouldEqual("Customer");
    [Fact] void should_keep_both_events_on_the_parent() => _withParent.Single().Events.Count.ShouldEqual(2);
    [Fact] void should_keep_the_cluster_standalone_without_a_parent() => _withoutParent.Single().Name.ShouldEqual("CustomerAddress");
}

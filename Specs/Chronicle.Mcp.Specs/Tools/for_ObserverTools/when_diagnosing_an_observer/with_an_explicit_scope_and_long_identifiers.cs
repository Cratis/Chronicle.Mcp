// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Specs.Tools.for_ObserverTools.when_diagnosing_an_observer;

public class with_an_explicit_scope_and_long_identifiers : given.an_observer_query
{
    const string ExplicitTenant = "OrderTenant";
    static readonly string _explicitStore = new('s', 512);
    static readonly string _longId = new('o', 520);
    static readonly string _explicitSequence = new('e', 520);

    void Establish()
    {
        _observer.Id = _longId;
        _observer.EventSequenceId = _explicitSequence;
        var failure = Failure("sample-partition");
        failure.ObserverId = _longId;
        WithFailures(failure);
    }

    async Task Because()
    {
        ClearCalls();
        _result = await Diagnose((_explicitStore, ExplicitTenant, _longId, _explicitSequence));
    }

    [Fact] void should_use_unshortened_inputs_for_all_reads() => ShouldReadScope((_explicitStore, ExplicitTenant, _longId, _explicitSequence));
    [Fact] void should_keep_a_store_at_the_limit() => _result.EventStore.ShouldEqual(_explicitStore);
    [Fact] void should_clip_the_displayed_observer_id() => _result.ObserverId.ShouldEqual(_longId[..512]);
    [Fact] void should_clip_the_displayed_sequence_id() => _result.Observer!.EventSequenceId.ShouldEqual(_explicitSequence[..512]);
    [Fact] void should_flag_shortened_identifiers() => _result.IdentifiersTruncated.ShouldBeTrue();
    [Fact] void should_match_failures_by_the_full_identifier() => Assert.Single(_result.Failures.Rows);
}

// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using Cratis.Chronicle.Contracts.Jobs;
using Cratis.Chronicle.Contracts.Queries;
using Cratis.Chronicle.Contracts.Recommendations;
using NSubstitute;
using ProtoBuf.Grpc;

namespace Cratis.Chronicle.Mcp.Specs.Tools.for_ObserverTools.when_diagnosing_an_observer;

public class when_associations_exist_only_in_prose : given.an_observer_query
{
    const string UnverifiedText = "Synthetic replay suggestion for " + ObserverId + " on " + Sequence;

    void Establish()
    {
        WithFailures(Failure("sample-partition"));
        _jobs.AllJobs(Arg.Any<AllJobsRequest>(), Arg.Any<CallContext>()).Returns(
            new QueryResult<IEnumerable<JobSummaryResponse>> { Data = [new() { Details = UnverifiedText }] });
        _recommendations.GetRecommendations(Arg.Any<GetRecommendationsRequest>(), Arg.Any<CallContext>()).Returns(
            new QueryResult<IEnumerable<RecommendationDetailsResponse>> { Data = [new() { Name = UnverifiedText }] });
    }

    async Task Because()
    {
        ClearCalls();
        _result = await Diagnose();
    }

    [Fact] void should_report_unknown_jobs() => _result.Jobs.Status.ShouldEqual("unknown");
    [Fact] void should_report_unknown_recommendations() => _result.Recommendations.Status.ShouldEqual("unknown");
    [Fact] void should_explain_why_the_associations_are_unknown() => Assert.All(new[] { _result.Jobs, _result.Recommendations }, section => section.Reason.ShouldEqual("no_typed_observer_target"));
    [Fact] void should_not_return_any_verified_rows() => Assert.All(new[] { _result.Jobs, _result.Recommendations }, section => Assert.Empty(section.Rows));
    [Fact] void should_not_report_a_window_of_unverified_rows() => Assert.All(new[] { _result.Jobs, _result.Recommendations }, section => Assert.True(section.Limit == 0 && !section.Truncated));
    [Fact] void should_not_echo_unverified_text_or_offer_actions() => Assert.DoesNotMatch("Synthetic replay suggestion|\"Actions\"", JsonSerializer.Serialize(_result));
    [Fact] void should_only_use_the_allowed_reads() => ShouldOnlyRead(2, 1, 1);
}

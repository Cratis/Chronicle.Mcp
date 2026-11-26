// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Cratis.Chronicle.Mcp.Configuration;
using Cratis.Screenplay.Comparison;
using Cratis.Screenplay.Observations;
using Cratis.Screenplay.Semantics;
using Cratis.Screenplay.Workspaces;
using ModelContextProtocol;
using NSubstitute;

namespace Cratis.Chronicle.Mcp.Tools.Design.for_ScreenplayConformanceTools;

public class when_comparing_a_namespace : a_namespace_audit
{
    Task Because() => Audit();

    [Fact]
    void should_match_the_cli_report_for_the_same_evidence()
    {
        var identity = ApplicationIdentity.Create(Store);
        var document = WorkspaceDocument.Create($"file-{Convert.ToHexStringLower(SHA256.HashData("application.play"u8))}", PortablePlayPath.Parse("application.play"), Encoding.UTF8.GetBytes(Source));
        var workspace = ScreenplayWorkspace.Create(identity, Store, [document], SemanticIdentityCatalog.Empty(identity));
        var compilation = ComparedModel.WithoutIdentities(workspace).Workspace.Compilation.Value!;
        var registered = new ObservedEventType("Registered", 2);
        var legacy = new ObservedEventType("Legacy", 1);
        var snapshot = ObservationSnapshot.Create(
            Store,
            Namespace,
            new(Sequence, true, 0, 2, 2, []),
            [new("Registered", 1), registered, new("RegisteredOnly", 1)],
            [new(registered, 2), new(legacy, 1)],
            [new(new(ObservedProducerKind.Command, "Register"), registered, 1)],
            [new(registered, 1), new(legacy, 1)],
            [new("Elsewhere", ObservedConsumerKind.Reaction, ObservedConsumerOwner.Application, "other-log", [registered], 0, 0)],
            Now);
        var expected = JsonNode.Parse(ObservationReportJson.Serialize(ObservationComparison.Compare(compilation, snapshot)));
        var actual = JsonNode.Parse(Body.GetRawText())!.AsObject();
        foreach (var key in new[] { "scanCapped", "reportTruncated", "omittedFindings", "omittedSlices", "conventions" }) actual.Remove(key);
        JsonNode.DeepEquals(actual, expected).ShouldBeTrue();
    }

    [Fact]
    void should_fetch_only_explicit_metadata_positions_in_the_target()
    {
        var page = _pages.Single();
        (page.EventStore, page.Namespace, page.EventSequenceId).ShouldEqual((Store, Namespace, Sequence));
        page.SequenceNumbers.ShouldEqual(0ul, 1ul, 2ul);
        _reads.ReceivedCalls().Select(call => call.GetMethodInfo().Name).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal)
            .ShouldEqual("get_EventStores", "get_EventTypes", "get_Namespaces", "get_Observers", "get_Sequences");
        _sequences.ReceivedCalls().Select(call => call.GetMethodInfo().Name).ShouldEqual("AllEventSequences", "TailSequenceNumber", "MetadataAt");
    }

    [Fact]
    void should_not_disclose_non_allowlisted_metadata()
    {
        var wire = JsonSerializer.Serialize(_result, McpJsonUtilities.ProtocolOptions);
        wire.Contains("private-", StringComparison.Ordinal).ShouldBeFalse();
        wire.Contains(_correlationId.ToString(), StringComparison.Ordinal).ShouldBeFalse();
        wire.Contains("initiatorType", StringComparison.Ordinal).ShouldBeFalse();
    }

    [Theory]
    [InlineData(DeploymentProfile.ReadOnly)]
    [InlineData(DeploymentProfile.Mutation)]
    public void should_be_exposed_as_a_read_only_tool(DeploymentProfile profile) =>
        _catalog.ForProfile(profile).Single(tool => tool.Name == Operation).IsReadOnly.ShouldBeTrue();
}

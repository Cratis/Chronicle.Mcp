// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using System.Text.Json.Nodes;
using Cratis.Chronicle.Mcp.Authorization;
using Cratis.Screenplay.Diagnostics;
using Cratis.Screenplay.Observations;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;

namespace Cratis.Chronicle.Mcp.Tools.Design.Conformance;

internal static class ConformanceReportPage
{
    const int MaxResultBytes = 64 * 1024;
    const int TransportOverheadReserve = 256;
    const int MaxSliceItems = 512;

    internal static CallToolResult Error(string code) => DenialEnvelope.Create(code, ScreenplayConformanceTools.Name);

    internal static CallToolResult InvalidModel(IEnumerable<Diagnostic> diagnostics) => Result(JsonSerializer.SerializeToElement(new
    {
        error = new { code = "conformance_invalid_model", operation = ScreenplayConformanceTools.Name },
        diagnostics = diagnostics.Take(32).Select(item => new { code = item.Code, path = "application.play", line = item.Location.Line, column = item.Location.Column })
    }), isError: true);

    internal static CallToolResult Create(ObservationReport report, ConformanceSnapshot snapshot)
    {
        var body = Canonical(report with { Findings = [], Slices = [] });
        body["scanCapped"] = snapshot.NextFrom.HasValue;
        if (snapshot.NextFrom.HasValue) body["continuation"] = JsonSerializer.SerializeToNode(new { fromSequence = snapshot.NextFrom, toSequence = snapshot.NextTo });
        body["conventions"] = JsonSerializer.SerializeToNode(new
        {
            generation = "Metadata uses the latest registered generation, or 1 when unregistered; it does not report stored generation.",
            evidence = "Only event types, bounded positions, counts, producer and subscription identities are disclosed.",
            omittedDetail = "Narrow the sequence range or model if report detail is omitted. A continuation is an independent comparison."
        });
        if (!Fits(body, report, 0, 0)) return Error("conformance_result_limit");

        var findings = AddPrefix(body, report, "findings", report.Findings.Length, 0,
            index => Canonical(report with { Findings = [report.Findings[index]], Slices = [] })["findings"]![0]);
        var slices = AddPrefix(body, report, "slices", report.Slices.Length, findings, index => Slice(report, index));
        SetCounts(body, report, findings, slices);
        return Result(JsonSerializer.SerializeToElement(body));
    }

    static JsonNode? Slice(ObservationReport report, int index)
    {
        var slice = report.Slices[index];
        // A single coherent slice may be too large to send. Refuse it before canonical serialization
        // if its nested records alone cannot reasonably fit the result budget.
        var items = slice.Events.Sum(item => 1L + item.GenerationCounts.Length) +
            slice.Producers.Sum(item => 1L + item.Edges.Length) + slice.Consumers.Sum(item => 1L + item.References.Length);
        return items > MaxSliceItems ? null : Canonical(report with { Findings = [], Slices = [slice] })["slices"]![0];
    }

    static int AddPrefix(JsonObject body, ObservationReport report, string field, int length, int findings, Func<int, JsonNode?> get)
    {
        var target = body[field]!.AsArray();
        for (var index = 0; index < length; index++)
        {
            if (get(index) is not { } item) break;
            target.Add(item.DeepClone());
            if (Fits(body, report, field == "findings" ? index + 1 : findings, field == "slices" ? index + 1 : 0)) continue;
            target.RemoveAt(index);
            break;
        }

        return target.Count;
    }

    static bool Fits(JsonObject body, ObservationReport report, int findings, int slices)
    {
        SetCounts(body, report, findings, slices);
        var preview = UntrustedDataEnvelope.Wrap(Result(JsonSerializer.SerializeToElement(body)), ScreenplayConformanceTools.Name);
        return JsonSerializer.SerializeToUtf8Bytes(preview, McpJsonUtilities.ProtocolOptions).Length <= MaxResultBytes - TransportOverheadReserve;
    }

    static void SetCounts(JsonObject body, ObservationReport report, int findings, int slices)
    {
        body["omittedFindings"] = report.Findings.Length - findings;
        body["omittedSlices"] = report.Slices.Length - slices;
        body["reportTruncated"] = findings < report.Findings.Length || slices < report.Slices.Length;
    }

    static JsonObject Canonical(ObservationReport report) => JsonNode.Parse(ObservationReportJson.Serialize(report))!.AsObject();

    static CallToolResult Result(JsonElement body, bool isError = false) => new()
    {
        IsError = isError,
        StructuredContent = body,
        Content = [new TextContentBlock { Text = body.GetRawText() }]
    };
}

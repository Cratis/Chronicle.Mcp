// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Text;
using Cratis.Chronicle.Mcp.Configuration;
using Cratis.Chronicle.Mcp.Tools.Design.Conformance;
using Cratis.Screenplay.Observations;
using Cratis.Screenplay.Semantics;
using Cratis.Screenplay.Workspaces;
using Grpc.Core;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using ProtoBuf.Grpc;

namespace Cratis.Chronicle.Mcp.Tools.Design;

/// <summary>
/// Compares bounded Chronicle metadata with an inline Screenplay model.
/// </summary>
/// <param name="services">The service provider for the lazy audit connection.</param>
/// <param name="connection">The shared audit admission slot.</param>
/// <param name="configuration">The deployment's configured defaults.</param>
/// <param name="clock">The clock for evidence capture.</param>
[McpServerToolType]
public sealed class ScreenplayConformanceTools(IServiceProvider services, ConformanceConnection connection, ChronicleConnectionConfiguration configuration, TimeProvider clock)
{
    /// <summary>
    /// The registered MCP operation name.
    /// </summary>
    internal const string Name = "audit_screenplay_conformance";
    static readonly TimeSpan _scanTimeout = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Compares declared relationships with registry, subscription, and bounded append metadata.
    /// </summary>
    /// <param name="eventStore">Exact Chronicle event store.</param>
    /// <param name="namespace">Exact Chronicle namespace.</param>
    /// <param name="sequence">Exact Chronicle sequence.</param>
    /// <param name="fromSequence">Inclusive nonnegative start position.</param>
    /// <param name="toSequence">Inclusive end position, clamped to the captured tail.</param>
    /// <param name="modelSource">A single self-contained inline Screenplay document.</param>
    /// <param name="applicationName">Optional semantic application name.</param>
    /// <param name="occurredFrom">Optional inclusive UTC time filter lower bound.</param>
    /// <param name="occurredTo">Optional exclusive UTC time filter upper bound.</param>
    /// <param name="cancellationToken">The caller's cancellation token.</param>
    /// <returns>A conformance report or a sanitized error.</returns>
    [McpServerTool(Name = Name, ReadOnly = true, OpenWorld = false)]
    [Description("Compare a self-contained inline Screenplay model with metadata from one explicit Chronicle sequence. Reports bounded evidence, not runtime correctness.")]
    public async Task<CallToolResult> AuditScreenplayConformance(
        string eventStore,
        string @namespace,
        string sequence,
        long fromSequence,
        long toSequence,
        string modelSource,
        string? applicationName = null,
        DateTimeOffset? occurredFrom = null,
        DateTimeOffset? occurredTo = null,
        CancellationToken cancellationToken = default)
    {
        var scope = new ConformanceScope(eventStore, @namespace, sequence, new(fromSequence, toSequence), (occurredFrom, occurredTo));
        if (!scope.IsValid || (applicationName is not null && !ConformanceScope.Identifier(applicationName))) return ConformanceReportPage.Error("conformance_invalid_scope");
        if (!connection.TryEnter()) return ConformanceReportPage.Error("conformance_busy");
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!ConformanceModelSources.Admit(modelSource)) return ConformanceReportPage.Error("conformance_model_limit");
            var (compilation, diagnostics) = ConformanceModelSources.Compile(modelSource, applicationName, configuration.ResolveEventStore());
            if (compilation is null) return ConformanceReportPage.InvalidModel(diagnostics);
            cancellationToken.ThrowIfCancellationRequested();
            return await Compare(scope, compilation, cancellationToken);
        }
        catch (Exception exception) when (exception is EncoderFallbackException or InvalidScreenplayWorkspace or InvalidWorkspaceDocument or InvalidSemanticContract)
        {
            return ConformanceReportPage.InvalidModel([]);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return ConformanceReportPage.Error("conformance_cancelled");
        }
        finally
        {
            connection.Exit();
        }
    }

    async Task<CallToolResult> Compare(ConformanceScope scope, SemanticCompilation compilation, CancellationToken cancellationToken)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(_scanTimeout);
        var token = timeout.Token;
        var context = new CallContext(new CallOptions(deadline: DateTime.UtcNow + _scanTimeout, cancellationToken: token));
        try
        {
            var reads = new ConformanceObservations(ConformanceConnection.Resolve(services), scope, context);
            var snapshot = await ConformanceSnapshot.Capture(reads, scope, clock, token);
            token.ThrowIfCancellationRequested();
            var identities = ConformanceIdentities.Map(compilation.Model, snapshot.Evidence.Consumers);
            var report = ObservationComparison.Compare(compilation, snapshot.Evidence, identities);
            var result = ConformanceReportPage.Create(report, snapshot);
            token.ThrowIfCancellationRequested();
            return result;
        }
        catch (ConformanceReadException exception)
        {
            return ConformanceReportPage.Error(exception.Message);
        }
        catch (InvalidObservationSnapshot)
        {
            return ConformanceReportPage.Error(ConformanceReadException.Failed);
        }
        catch (OperationCanceledException) when (timeout.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            return ConformanceReportPage.Error("conformance_timeout");
        }
        catch (RpcException exception) when (exception.StatusCode is not (StatusCode.PermissionDenied or StatusCode.Unauthenticated))
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ConformanceReportPage.Error(exception.StatusCode switch
            {
                StatusCode.DeadlineExceeded or StatusCode.Cancelled => "conformance_timeout",
                StatusCode.ResourceExhausted => ConformanceReadException.Limit,
                _ => ConformanceReadException.Failed
            });
        }
    }
}

// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Mcp.Configuration;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

using DeploymentOptions = Cratis.Chronicle.Mcp.Configuration.McpServerOptions;

namespace Cratis.Chronicle.Mcp.Authorization;

/// <summary>
/// Applies the deployment policy before mutation and sanitizes Chronicle denials.
/// </summary>
/// <param name="catalog">The shared tool catalog.</param>
/// <param name="authorizer">The policy decision logic.</param>
/// <param name="options">The live deployment policy.</param>
/// <param name="logger">The denial logger.</param>
public class AuthorizedToolCalls(ToolCatalog catalog, MutationAuthorizer authorizer, IOptionsMonitor<DeploymentOptions> options, ILogger<AuthorizedToolCalls> logger)
{
    /// <summary>
    /// Wraps the MCP tool-call pipeline.
    /// </summary>
    /// <param name="next">The next handler.</param>
    /// <returns>The filtered handler.</returns>
    public McpRequestHandler<CallToolRequestParams, CallToolResult> Filter(McpRequestHandler<CallToolRequestParams, CallToolResult> next) => async (request, cancellationToken) =>
    {
        var tool = request.MatchedPrimitive is McpServerTool registeredTool
            ? catalog.Find(registeredTool.ProtocolTool.Name)
            : null;
        if (tool is { IsReadOnly: false } && AuthorizeMutation(tool, request.Params?.Arguments) is { } denial)
        {
            return denial;
        }

        try
        {
            return await InvokeAndWrap(next, request, cancellationToken);
        }
        catch (RpcException exception) when (exception.StatusCode is StatusCode.PermissionDenied or StatusCode.Unauthenticated)
        {
            return Deny(DenialCodes.ChronicleDenied, tool?.Name ?? string.Empty);
        }
    };

    static async ValueTask<CallToolResult> InvokeAndWrap(McpRequestHandler<CallToolRequestParams, CallToolResult> next, RequestContext<CallToolRequestParams> request, CancellationToken cancellationToken)
    {
        // Let the SDK hide Chronicle exception text; tools must not throw McpException
        // with store-controlled text, since the SDK would show that text verbatim.
        var result = await next(request, cancellationToken);
        return UntrustedDataEnvelope.Wrap(result, request.Params?.Name ?? string.Empty);
    }

    CallToolResult? AuthorizeMutation(CatalogTool tool, IDictionary<string, System.Text.Json.JsonElement>? arguments)
    {
        if (!authorizer.HasValidTarget(tool, arguments)) return Deny(DenialCodes.BadRequest, tool.Name);

        AuthorizationDecision decision;
        try
        {
            decision = authorizer.Authorize(tool, arguments, options.CurrentValue);
        }
        catch (InvalidMcpConfiguration)
        {
            decision = new(DenialCodes.PolicyInvalid);
        }

        return decision.IsAllowed ? null : Deny(decision.Code!, tool.Name);
    }

    CallToolResult Deny(string code, string operation)
    {
        logger.ToolDenied(code, operation);

        return DenialEnvelope.Create(code, operation);
    }
}

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
        if (tool is { IsReadOnly: false })
        {
            if (!authorizer.HasValidTarget(tool, request.Params?.Arguments)) return Deny(DenialCodes.BadRequest, tool.Name);

            AuthorizationDecision decision;
            try
            {
                decision = authorizer.Authorize(tool, request.Params?.Arguments, options.CurrentValue);
            }
            catch (InvalidMcpConfiguration)
            {
                decision = new(DenialCodes.PolicyInvalid);
            }

            if (!decision.IsAllowed) return Deny(decision.Code!, tool.Name);
        }

        try
        {
            return await next(request, cancellationToken);
        }
        catch (RpcException exception) when (exception.StatusCode is StatusCode.PermissionDenied or StatusCode.Unauthenticated)
        {
            return Deny(DenialCodes.ChronicleDenied, tool?.Name ?? string.Empty);
        }
    };

    CallToolResult Deny(string code, string operation)
    {
        logger.ToolDenied(code, operation);

        return DenialEnvelope.Create(code, operation);
    }
}

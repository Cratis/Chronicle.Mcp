// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using Cratis.Chronicle.Mcp.Configuration;
using Cratis.Chronicle.Mcp.Tools;

namespace Cratis.Chronicle.Mcp.Authorization;

/// <summary>
/// Evaluates each mutating request without invoking a tool or throwing on a denial.
/// </summary>
/// <param name="policy">The shared grant validator.</param>
/// <param name="configuration">The same scope defaults used by tools.</param>
/// <param name="timeProvider">The policy clock.</param>
public class MutationAuthorizer(MutationPolicy policy, ChronicleConnectionConfiguration configuration, TimeProvider timeProvider)
{
    /// <summary>
    /// Authorizes a mutating request, returning the first denial in contract order.
    /// </summary>
    /// <param name="tool">The operation and target parameter metadata.</param>
    /// <param name="arguments">The caller's arguments.</param>
    /// <param name="settings">The policy reread for this call.</param>
    /// <returns>The authorization decision.</returns>
    public AuthorizationDecision Authorize(CatalogTool tool, IDictionary<string, JsonElement>? arguments, McpServerOptions settings)
    {
        var target = ReadTarget(tool, arguments);
        if (target is null) return new(DenialCodes.BadRequest);

        var authorization = settings.Authorization;
        if (string.IsNullOrWhiteSpace(authorization.Principal)) return new(DenialCodes.NotAuthenticated);
        if (settings.Profile != DeploymentProfile.Mutation || policy.InvalidSettings(authorization).Any()) return new(DenialCodes.PolicyInvalid);
        if (authorization.ExpiresAt is null || authorization.ExpiresAt <= timeProvider.GetUtcNow()) return new(DenialCodes.PolicyExpired);
        if (authorization.RevokedPrincipals.Contains(authorization.Principal, StringComparer.Ordinal)) return new(DenialCodes.PrincipalRevoked);

        var eventStore = configuration.ResolveEventStore(ReadString(arguments, "eventStore"));
        var @namespace = configuration.ResolveNamespace(ReadString(arguments, "namespace"));
        var matches = authorization.Grants.Any(grant =>
            string.Equals(grant.Principal, authorization.Principal, StringComparison.Ordinal) &&
            Matches(grant.EventStore, eventStore) && Matches(grant.Namespace, @namespace) &&
            grant.Operations.Contains(tool.Name, StringComparer.Ordinal) && grant.Targets.Any(candidate => Matches(candidate, target)));

        return new(matches ? null : DenialCodes.NotAuthorized);
    }

    /// <summary>
    /// Checks the target before reading a policy that might fail to bind.
    /// </summary>
    /// <param name="tool">The target parameter metadata.</param>
    /// <param name="arguments">The caller's arguments.</param>
    /// <returns>Whether the target parses as its parameter type.</returns>
    public bool HasValidTarget(CatalogTool tool, IDictionary<string, JsonElement>? arguments) => ReadTarget(tool, arguments) is not null;

    static bool Matches(string granted, string requested) => granted == "*" || string.Equals(granted, requested, StringComparison.Ordinal);

    static string? ReadString(IDictionary<string, JsonElement>? arguments, string name) =>
        arguments is not null && arguments.TryGetValue(name, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;

    static string? ReadTarget(CatalogTool tool, IDictionary<string, JsonElement>? arguments)
    {
        if (tool.Target is not { } parameter || arguments is null || !arguments.TryGetValue(parameter.Name!, out var value) || value.ValueKind != JsonValueKind.String)
        {
            return null;
        }

        try
        {
            return value.Deserialize(parameter.ParameterType) is not null ? value.GetString() : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}

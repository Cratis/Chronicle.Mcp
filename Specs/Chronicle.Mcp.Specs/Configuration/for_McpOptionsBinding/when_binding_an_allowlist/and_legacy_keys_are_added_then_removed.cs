// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using System.Text.Json.Nodes;
using NSubstitute;

namespace Cratis.Chronicle.Mcp.Configuration.for_McpOptionsBinding.when_binding_an_allowlist;

public class and_legacy_keys_are_added_then_removed : given.an_allowlist
{
    bool _warnedWhenAdded;
    bool _warnedWhenRemoved;

    async Task Because()
    {
        await Start();
        _logger.ClearReceivedCalls();
        _authorization["Principal"] = Principal;
        _authorization["Grants"] = new JsonArray(JsonSerializer.SerializeToNode(_grant));
        ReplacePolicy();
        _warnedWhenAdded = HasLegacyWarning();
        _logger.ClearReceivedCalls();
        _authorization.Remove("Principal");
        _authorization.Remove("Grants");
        ReplacePolicy();
        _warnedWhenRemoved = HasLegacyWarning();
    }

    [Fact] void should_warn_when_legacy_settings_appear() => _warnedWhenAdded.ShouldBeTrue();
    [Fact] void should_not_keep_a_sticky_legacy_marker() => _warnedWhenRemoved.ShouldBeFalse();
    bool HasLegacyWarning() => _logger.ReceivedCalls().Any(call => call.GetMethodInfo().Name == "Log" && call.GetArguments()[2]!.ToString()!.StartsWith("Legacy", StringComparison.Ordinal));
}

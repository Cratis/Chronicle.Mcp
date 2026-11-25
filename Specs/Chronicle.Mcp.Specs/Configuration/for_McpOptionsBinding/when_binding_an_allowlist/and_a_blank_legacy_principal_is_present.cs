// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Options;

namespace Cratis.Chronicle.Mcp.Configuration.for_McpOptionsBinding.when_binding_an_allowlist;

public class and_a_blank_legacy_principal_is_present : given.an_allowlist
{
    void Establish()
    {
        _authorization["Principal"] = string.Empty;
        ReplacePolicy();
    }

    async Task Because() => _error = await Catch.Exception(Start);
    [Fact] void should_not_bypass_the_malformed_legacy_policy() => _error.ShouldBeOfExactType<OptionsValidationException>();
    [Fact] void should_name_the_legacy_principal() => ((OptionsValidationException)_error).Failures.ShouldContain($"{McpOptionsBinding.SectionPath}:Authorization:Principal");
}

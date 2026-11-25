// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Options;

namespace Cratis.Chronicle.Mcp.Configuration.for_McpOptionsBinding.when_binding_an_allowlist;

public class and_allowlist_is_null : given.an_allowlist
{
    void Establish()
    {
        _authorization["Allowlist"] = null;
        ReplacePolicy();
    }

    async Task Because() => _error = await Catch.Exception(Start);
    [Fact] void should_fail_closed() => _error.ShouldBeOfExactType<OptionsValidationException>();
}

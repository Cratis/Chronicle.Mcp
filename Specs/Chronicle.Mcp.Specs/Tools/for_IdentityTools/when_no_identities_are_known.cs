// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Mcp.Tools;

namespace Cratis.Chronicle.Mcp.Specs.Tools.for_IdentityTools;

public class when_no_identities_are_known : given.an_identity_query
{
    async Task Because() => _result = await IdentityTools.ListIdentities(_services, _configuration);

    [Fact] void should_return_no_identities() => _result.Identities.ShouldBeEmpty();
    [Fact] void should_not_report_truncation() => _result.Truncated.ShouldBeFalse();
}

// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Mcp.Tools;

namespace Cratis.Chronicle.Mcp.Specs.Tools.for_IdentityTools;

public class when_the_result_exactly_fits_the_limit : given.an_identity_query
{
    void Establish() => WithIdentities(2);

    async Task Because() => _result = await IdentityTools.ListIdentities(_services, _configuration, limit: 2);

    [Fact] void should_return_all_identities() => _result.Identities.Count.ShouldEqual(2);
    [Fact] void should_not_report_truncation() => _result.Truncated.ShouldBeFalse();
}

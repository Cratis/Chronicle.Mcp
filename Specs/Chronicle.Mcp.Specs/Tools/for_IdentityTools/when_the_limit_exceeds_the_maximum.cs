// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Mcp.Tools;

namespace Cratis.Chronicle.Mcp.Specs.Tools.for_IdentityTools;

public class when_the_limit_exceeds_the_maximum : given.an_identity_query
{
    void Establish() => WithIdentities(501);

    async Task Because() => _result = await IdentityTools.ListIdentities(_services, _configuration, limit: int.MaxValue);

    [Fact] void should_cap_the_number_of_identities() => _result.Identities.Count.ShouldEqual(500);
    [Fact] void should_report_truncation() => _result.Truncated.ShouldBeTrue();
}

// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Mcp.Tools;

namespace Cratis.Chronicle.Mcp.Specs.Tools.for_IdentityTools;

public class when_the_limit_is_not_positive : given.an_identity_query
{
    void Establish() => WithIdentities(2);

    async Task Because() => _result = await IdentityTools.ListIdentities(_services, _configuration, limit: int.MinValue);

    [Fact] void should_clamp_the_limit_to_one() => _result.Identities.Count.ShouldEqual(1);
    [Fact] void should_report_truncation() => _result.Truncated.ShouldBeTrue();
}

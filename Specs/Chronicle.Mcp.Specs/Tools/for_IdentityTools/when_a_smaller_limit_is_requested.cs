// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Mcp.Tools;

namespace Cratis.Chronicle.Mcp.Specs.Tools.for_IdentityTools;

public class when_a_smaller_limit_is_requested : given.an_identity_query
{
    void Establish() => WithIdentities(3);

    async Task Because() => _result = await IdentityTools.ListIdentities(_services, _configuration, limit: 2);

    [Fact] void should_return_only_the_requested_identities() => _result.Identities.Select(identity => identity.Subject).ShouldEqual(["subject-0", "subject-1"]);
    [Fact] void should_report_truncation() => _result.Truncated.ShouldBeTrue();
}

// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Mcp.Tools;

namespace Cratis.Chronicle.Mcp.Specs.Tools.for_IdentityTools;

public class when_the_default_limit_is_exceeded : given.an_identity_query
{
    void Establish() => WithIdentities(101);

    async Task Because() => _result = await IdentityTools.ListIdentities(_services, _configuration);

    [Fact] void should_return_the_default_number_of_identities() => _result.Identities.Count.ShouldEqual(100);
    [Fact] void should_report_truncation() => _result.Truncated.ShouldBeTrue();
    [Fact] void should_preserve_the_first_identity() => _result.Identities[0].Subject.ShouldEqual("subject-0");
    [Fact] void should_preserve_the_last_identity_within_the_limit() => _result.Identities[^1].Subject.ShouldEqual("subject-99");
}

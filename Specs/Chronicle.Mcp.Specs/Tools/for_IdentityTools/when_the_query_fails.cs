// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Mcp.Tools;

namespace Cratis.Chronicle.Mcp.Specs.Tools.for_IdentityTools;

public class when_the_query_fails : given.an_identity_query
{
    Exception _error;

    void Establish() => _response.ExceptionMessages = ["Identity query refused"];

    async Task Because() => _error = await Catch.Exception(() => IdentityTools.ListIdentities(_services, _configuration));

    [Fact] void should_not_return_an_empty_success() => _error.ShouldBeOfExactType<InvalidOperationException>();
    [Fact] void should_preserve_the_query_failure_message() => _error.Message.ShouldEqual("Query failed: Identity query refused");
}

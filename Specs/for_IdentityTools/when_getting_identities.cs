// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Mcp.Api;

namespace Cratis.Chronicle.Mcp.Specs.for_IdentityTools;

[Collection(ChronicleCollection.Name)]
public class when_getting_identities(ChronicleFixture fixture) : given.a_chronicle_fixture(fixture)
{
    IEnumerable<Identity> _result = null!;

    async Task Because() =>
        _result = await IdentityTools.GetIdentities(ApiClient, ChronicleFixture.SharedEventStoreName);

    [Fact] void should_return_identities() => _result.ShouldNotBeNull();
    [Fact] void should_handle_server_gracefully() => _result.Count().ShouldEqual(0);
}

// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Contracts.Identities;
using Cratis.Chronicle.Mcp.Tools;

namespace Cratis.Chronicle.Mcp.Specs.Tools.for_IdentityTools;

public class when_identity_fields_are_missing : given.an_identity_query
{
    void Establish() => _response.Data = [new IdentityDetailsResponse { OnBehalfOf = new Identity() }];

    async Task Because() => _result = await IdentityTools.ListIdentities(_services, _configuration);

    [Fact] void should_return_an_empty_subject() => _result.Identities[0].Subject.ShouldEqual(string.Empty);
    [Fact] void should_return_an_empty_name() => _result.Identities[0].Name.ShouldEqual(string.Empty);
    [Fact] void should_return_an_empty_user_name() => _result.Identities[0].UserName.ShouldEqual(string.Empty);
    [Fact] void should_return_an_empty_delegated_subject() => _result.Identities[0].OnBehalfOf!.Subject.ShouldEqual(string.Empty);
    [Fact] void should_return_an_empty_delegated_name() => _result.Identities[0].OnBehalfOf!.Name.ShouldEqual(string.Empty);
    [Fact] void should_return_an_empty_delegated_user_name() => _result.Identities[0].OnBehalfOf!.UserName.ShouldEqual(string.Empty);
}

// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Contracts.Identities;
using Cratis.Chronicle.Mcp.Tools;
using NSubstitute;
using ProtoBuf.Grpc;

namespace Cratis.Chronicle.Mcp.Specs.Tools.for_IdentityTools;

public class when_listing_identities : given.an_identity_query
{
    void Establish() => _response.Data =
    [
        new IdentityDetailsResponse
        {
            Subject = "agent",
            Name = "Example Agent",
            UserName = "example-agent",
            OnBehalfOf = new Identity
            {
                Subject = "delegate",
                Name = "Example Delegate",
                UserName = "example-delegate",
                OnBehalfOf = new Identity { Subject = "person", Name = "Example Person", UserName = "example-person" }
            }
        },
        new IdentityDetailsResponse { Subject = "system", Name = "System", UserName = "system" }
    ];

    async Task Because() => _result = await IdentityTools.ListIdentities(_services, _configuration);

    [Fact] void should_use_the_configured_event_store() => _request.EventStore.ShouldEqual("ConfiguredStore");
    [Fact] void should_use_the_configured_namespace() => _request.Namespace.ShouldEqual("ConfiguredTenant");
    [Fact] void should_query_the_configured_service_once() => _identities.Received(1).GetIdentities(Arg.Any<GetIdentitiesRequest>(), Arg.Any<CallContext>());
    [Fact] void should_preserve_server_order() => _result.Identities.Select(identity => identity.Subject).ShouldEqual(["agent", "system"]);
    [Fact] void should_map_the_name() => _result.Identities[0].Name.ShouldEqual("Example Agent");
    [Fact] void should_map_the_user_name() => _result.Identities[0].UserName.ShouldEqual("example-agent");
    [Fact] void should_map_the_on_behalf_of_subject() => _result.Identities[0].OnBehalfOf!.Subject.ShouldEqual("delegate");
    [Fact] void should_map_the_on_behalf_of_name() => _result.Identities[0].OnBehalfOf!.Name.ShouldEqual("Example Delegate");
    [Fact] void should_map_the_on_behalf_of_user_name() => _result.Identities[0].OnBehalfOf!.UserName.ShouldEqual("example-delegate");
    [Fact] void should_preserve_the_delegation_chain() => _result.Identities[0].OnBehalfOf!.OnBehalfOf!.Subject.ShouldEqual("person");
    [Fact] void should_preserve_the_end_of_the_delegation_chain() => _result.Identities[0].OnBehalfOf!.OnBehalfOf!.OnBehalfOf.ShouldBeNull();
    [Fact] void should_preserve_an_absent_on_behalf_of() => _result.Identities[1].OnBehalfOf.ShouldBeNull();
    [Fact] void should_not_report_truncation() => _result.Truncated.ShouldBeFalse();
}

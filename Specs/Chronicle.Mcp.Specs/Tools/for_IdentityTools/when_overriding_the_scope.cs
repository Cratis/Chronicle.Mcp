// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Mcp.Tools;

namespace Cratis.Chronicle.Mcp.Specs.Tools.for_IdentityTools;

public class when_overriding_the_scope : given.an_identity_query
{
    async Task Because() => _result = await IdentityTools.ListIdentities(_services, _configuration, "OtherStore", "OtherTenant");

    [Fact] void should_use_the_requested_event_store() => _request.EventStore.ShouldEqual("OtherStore");
    [Fact] void should_use_the_requested_namespace() => _request.Namespace.ShouldEqual("OtherTenant");
}

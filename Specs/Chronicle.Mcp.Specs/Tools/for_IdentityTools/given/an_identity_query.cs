// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Contracts;
using Cratis.Chronicle.Contracts.Identities;
using Cratis.Chronicle.Contracts.Queries;
using Cratis.Chronicle.Mcp.Configuration;
using Cratis.Chronicle.Mcp.Tools;
using Microsoft.Extensions.Options;
using NSubstitute;
using ProtoBuf.Grpc;

namespace Cratis.Chronicle.Mcp.Specs.Tools.for_IdentityTools.given;

public class an_identity_query : Specification
{
    protected IServices _services;
    protected IIdentities _identities;
    protected ChronicleConnectionConfiguration _configuration;
    protected GetIdentitiesRequest _request;
    protected QueryResult<IEnumerable<IdentityDetailsResponse>> _response;
    protected IdentitiesResult _result;

    void Establish()
    {
        _services = Substitute.For<IServices>();
        _identities = Substitute.For<IIdentities>();
        _services.Identities.Returns(_identities);
        _configuration = new(Options.Create(new McpServerOptions
        {
            UseCliConfiguration = false,
            EventStore = "ConfiguredStore",
            Namespace = "ConfiguredTenant"
        }));
        _response = new() { Data = [] };
        _identities.GetIdentities(Arg.Any<GetIdentitiesRequest>(), Arg.Any<CallContext>()).Returns(call =>
        {
            _request = call.Arg<GetIdentitiesRequest>();
            return _response;
        });
    }

    protected void WithIdentities(int count) => _response.Data = Enumerable.Range(0, count)
        .Select(index => new IdentityDetailsResponse { Subject = $"subject-{index}" });
}

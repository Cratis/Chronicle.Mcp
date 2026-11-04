// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Contracts;
using Cratis.Chronicle.Contracts.Patterns;
using Cratis.Chronicle.Contracts.Queries;
using Cratis.Chronicle.Mcp.Configuration;
using Microsoft.Extensions.Options;
using NSubstitute;
using ProtoBuf.Grpc;

namespace Cratis.Chronicle.Mcp.Tools.for_PatternTools.given;

public class a_pattern_query : Specification
{
    protected IServices _services;
    protected IPatterns _patterns;
    protected ChronicleConnectionConfiguration _configuration;
    protected MatchingPatternsRequest _request;
    protected CallContext _callContext;
    protected QueryResult<IEnumerable<BehaviorPatternDetailsResponse>> _response;
    protected IEnumerable<BehaviorPatternDetailsResponse> _result;
    protected IDictionary<string, string> _context;

    void Establish()
    {
        _services = Substitute.For<IServices>();
        _patterns = Substitute.For<IPatterns>();
        _services.Patterns.Returns(_patterns);
        _configuration = new(Options.Create(new McpServerOptions
        {
            UseCliConfiguration = false,
            EventStore = "ConfiguredStore",
            Namespace = "ConfiguredTenant"
        }));
        _context = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["Action"] = "OrderPlaced",
            ["action"] = "Unchanged",
            ["CustomFacet"] = "CustomValue"
        };
        _response = new() { Data = [] };
        _patterns.MatchingPatterns(Arg.Any<MatchingPatternsRequest>(), Arg.Any<CallContext>()).Returns(call =>
        {
            _request = call.Arg<MatchingPatternsRequest>();
            _callContext = call.Arg<CallContext>();
            return _response;
        });
    }
}

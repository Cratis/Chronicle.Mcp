// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Tools.for_PatternTools;

public class when_the_query_has_validation_errors : given.a_pattern_query
{
    Exception _error;

    void Establish() => _response.ValidationResults = [new()];
    async Task Because() => _error = await Catch.Exception(() => PatternTools.GetPatterns(_services, _configuration, "Group", _context));

    [Fact] void should_not_return_an_empty_success() => _error.ShouldBeOfExactType<InvalidOperationException>();
    [Fact] void should_use_the_existing_query_failure_message() => _error.Message.ShouldEqual("Query failed: ");
}

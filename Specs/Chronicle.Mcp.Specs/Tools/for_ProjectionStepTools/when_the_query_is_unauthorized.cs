// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Tools.for_ProjectionStepTools;

public class when_the_query_is_unauthorized : given.a_projection_step
{
    Exception _error;

    void Establish() => _response.IsAuthorized = false;

    async Task Because() => _error = await Catch.Exception(() => Step());

    [Fact] void should_not_return_an_empty_success() => _error.ShouldBeOfExactType<InvalidOperationException>();
    [Fact] void should_use_the_existing_query_failure_message() => _error.Message.ShouldEqual("Query failed: ");
}

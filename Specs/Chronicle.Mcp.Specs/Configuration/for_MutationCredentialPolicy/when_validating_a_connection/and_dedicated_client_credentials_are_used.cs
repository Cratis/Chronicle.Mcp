// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Configuration.for_MutationCredentialPolicy.when_validating_a_connection;

public class and_dedicated_client_credentials_are_used : given.resolved_development_credentials
{
    void Establish() => _connection = _connection.WithCredentials("spec-client", "spec-secret");
    void Because() => _error = Catch.Exception(() => MutationCredentialPolicy.ValidateConnection(_connection, _settings));
    [Fact] void should_not_require_the_development_opt_in() => _error.ShouldBeNull();
}

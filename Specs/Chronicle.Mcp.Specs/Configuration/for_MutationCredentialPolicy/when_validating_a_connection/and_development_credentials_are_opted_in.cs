// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Configuration.for_MutationCredentialPolicy.when_validating_a_connection;

public class and_development_credentials_are_opted_in : given.resolved_development_credentials
{
    void Establish() => _settings.Authorization.AllowDevelopmentCredentials = true;
    void Because() => _error = Catch.Exception(() => MutationCredentialPolicy.ValidateConnection(_connection, _settings));
    [Fact] void should_preserve_the_explicit_local_exception() => _error.ShouldBeNull();
}

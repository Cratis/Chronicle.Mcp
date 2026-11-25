// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Configuration.for_MutationCredentialPolicy.when_validating_a_connection;

public class and_the_profile_is_read_only : given.resolved_development_credentials
{
    void Establish() => _settings.Profile = DeploymentProfile.ReadOnly;
    void Because() => _error = Catch.Exception(() => MutationCredentialPolicy.ValidateConnection(_connection, _settings));
    [Fact] void should_preserve_read_only_credential_behavior() => _error.ShouldBeNull();
}

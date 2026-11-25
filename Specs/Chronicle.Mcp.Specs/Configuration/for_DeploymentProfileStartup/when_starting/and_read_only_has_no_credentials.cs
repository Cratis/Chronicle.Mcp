// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Configuration.for_DeploymentProfileStartup.when_starting;

public class and_read_only_has_no_credentials : given.a_startup
{
    void Establish()
    {
        _settings.Profile = DeploymentProfile.ReadOnly;
        _settings.ClientId = null;
        _settings.ClientSecret = null;
        _settings.ConnectionString = null;
        _settings.Authorization = new();
    }

    async Task Because() => _error = await Catch.Exception(() => _startup.StartAsync(CancellationToken.None));
    [Fact] void should_leave_connection_validation_to_the_existing_connection_path() => _error.ShouldBeNull();
    [Fact] void should_not_emit_mutation_warnings() => Warnings.ShouldBeEmpty();
}

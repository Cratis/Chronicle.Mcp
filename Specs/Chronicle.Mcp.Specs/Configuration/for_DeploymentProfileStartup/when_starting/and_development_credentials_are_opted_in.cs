// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Configuration.for_DeploymentProfileStartup.when_starting;

public class and_development_credentials_are_opted_in : given.a_startup
{
    void Establish()
    {
        _settings.ClientId = null;
        _settings.ClientSecret = null;
        _settings.UseDevelopmentCredentials = true;
        _settings.Authorization.AllowDevelopmentCredentials = true;
    }

    async Task Because() => _error = await Catch.Exception(() => _startup.StartAsync(CancellationToken.None));
    [Fact] void should_start() => _error.ShouldBeNull();
    [Fact] void should_warn_prominently() => Warnings.Any(message => message.StartsWith("DANGER:", StringComparison.Ordinal)).ShouldBeTrue();
    [Fact] void should_not_log_credentials() => Warnings.Any(message => message.Contains("spec-secret", StringComparison.Ordinal)).ShouldBeFalse();
}

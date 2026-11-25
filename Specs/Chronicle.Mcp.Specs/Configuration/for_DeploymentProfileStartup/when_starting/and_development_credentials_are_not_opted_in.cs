// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Options;

namespace Cratis.Chronicle.Mcp.Configuration.for_DeploymentProfileStartup.when_starting;

public class and_development_credentials_are_not_opted_in : given.a_startup
{
    void Establish()
    {
        _settings.ClientId = null;
        _settings.ClientSecret = null;
        _settings.UseDevelopmentCredentials = true;
    }

    async Task Because() => _error = await Catch.Exception(() => _startup.StartAsync(CancellationToken.None));
    [Fact] void should_reject_mutation() => _error.ShouldBeOfExactType<OptionsValidationException>();
    [Fact] void should_name_only_the_opt_in_setting() => ((OptionsValidationException)_error).Failures.ShouldEqual(new[] { $"{McpOptionsBinding.SectionPath}:Authorization:AllowDevelopmentCredentials" });
}

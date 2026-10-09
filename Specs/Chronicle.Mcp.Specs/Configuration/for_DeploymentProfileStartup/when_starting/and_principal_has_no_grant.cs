// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Options;

namespace Cratis.Chronicle.Mcp.Configuration.for_DeploymentProfileStartup.when_starting;

public class and_principal_has_no_grant : given.a_startup
{
    void Establish() { _settings.Authorization.Grants = []; }
    async Task Because() => _error = await Catch.Exception(() => _startup.StartAsync(CancellationToken.None));
    [Fact] void should_refuse_startup() => _error.ShouldBeOfExactType<OptionsValidationException>();
    [Fact] void should_name_only_the_setting_path() => ((OptionsValidationException)_error).Failures.ShouldContain($"{McpOptionsBinding.SectionPath}:Authorization:Grants");
    [Fact] void should_not_expose_configuration_values() => _error.Message.Contains("private", StringComparison.Ordinal).ShouldBeFalse();
}

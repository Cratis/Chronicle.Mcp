// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Options;

namespace Cratis.Chronicle.Mcp.Configuration.for_DeploymentProfileStartup.when_starting;

public class and_mutation_has_no_credentials : given.a_startup
{
    void Establish()
    {
        _settings.ClientId = null;
        _settings.ClientSecret = null;
    }

    async Task Because() => _error = await Catch.Exception(() => _startup.StartAsync(CancellationToken.None));
    [Fact] void should_fail_before_serving_calls() => _error.ShouldBeOfExactType<OptionsValidationException>();
    [Fact] void should_report_only_a_setting_path() => ((OptionsValidationException)_error).Failures.ShouldEqual(new[] { $"{McpOptionsBinding.SectionPath}:ConnectionString" });
}

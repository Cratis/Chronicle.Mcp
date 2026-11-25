// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;

namespace Cratis.Chronicle.Mcp.Configuration.for_DeploymentProfileStartup.when_starting;

public class and_read_only_has_a_missing_cli_configuration_file : given.a_read_only_startup
{
    void Establish() => _configuration["CliConfigurationPath"] = Path.Combine(AppContext.BaseDirectory, $"missing-{Guid.NewGuid()}.json");
    async Task Because() => _error = await Catch.Exception(Start);
    [Fact] void should_preserve_lazy_read_only_startup() => _error.ShouldBeNull();
    [Fact] void should_still_reject_a_missing_cli_file_when_resolved() => Catch.Exception(() => _provider.GetRequiredService<ChronicleConnectionConfiguration>()).ShouldBeOfExactType<FileNotFoundException>();
}

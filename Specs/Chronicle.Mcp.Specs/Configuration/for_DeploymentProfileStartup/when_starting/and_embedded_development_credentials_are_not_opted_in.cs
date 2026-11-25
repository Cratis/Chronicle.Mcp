// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Connections;
using Microsoft.Extensions.Options;

namespace Cratis.Chronicle.Mcp.Configuration.for_DeploymentProfileStartup.when_starting;

public class and_embedded_development_credentials_are_not_opted_in : given.a_startup
{
    void Establish() => _settings.ConnectionString = new ChronicleConnectionString("chronicle://localhost:35000").WithCredentials(ChronicleConnectionString.DevelopmentClient, ChronicleConnectionString.DevelopmentClientSecret).ToString();
    async Task Because() => _error = await Catch.Exception(() => _startup.StartAsync(CancellationToken.None));
    [Fact] void should_not_bypass_the_opt_in_with_embedded_credentials() => _error.ShouldBeOfExactType<OptionsValidationException>();
}

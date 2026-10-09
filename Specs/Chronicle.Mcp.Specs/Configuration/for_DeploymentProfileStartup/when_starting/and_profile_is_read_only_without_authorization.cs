// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using NSubstitute;

namespace Cratis.Chronicle.Mcp.Configuration.for_DeploymentProfileStartup.when_starting;

public class and_profile_is_read_only_without_authorization : given.a_startup
{
    void Establish() { _settings = new();
        _monitor.CurrentValue.Returns(_settings); }
    async Task Because() => _error = await Catch.Exception(() => _startup.StartAsync(CancellationToken.None));
    [Fact] void should_start() => _error.ShouldBeNull();
}

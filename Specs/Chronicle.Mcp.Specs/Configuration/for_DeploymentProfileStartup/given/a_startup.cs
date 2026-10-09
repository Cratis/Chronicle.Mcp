// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Configuration.for_DeploymentProfileStartup.given;

public class a_startup : Authorization.Specs.given.a_policy
{
    protected DeploymentProfileStartup _startup;
    protected Exception? _error;
    void Establish() => _startup = new(_monitor, _policy, _clock);
}

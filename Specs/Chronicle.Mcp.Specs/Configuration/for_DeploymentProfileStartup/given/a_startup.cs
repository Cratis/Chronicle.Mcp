// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Mcp.Authorization;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Cratis.Chronicle.Mcp.Configuration.for_DeploymentProfileStartup.given;

public class a_startup : Authorization.Specs.given.a_policy
{
    protected DeploymentProfileStartup _startup;
    protected Exception? _error;
    protected ILogger<MutationPolicyWarnings> _logger;

    void Establish()
    {
        _settings.ConnectionString = "chronicle://localhost:35000";
        _settings.ClientId = "spec-client";
        _settings.ClientSecret = "spec-secret";
        _logger = Substitute.For<ILogger<MutationPolicyWarnings>>();
        _logger.IsEnabled(LogLevel.Warning).Returns(true);
        var credentials = new MutationCredentialPolicy(() => _configuration);
        _startup = new(_monitor, _policy, _clock, credentials, new(credentials, _logger, _clock));
    }

    protected IEnumerable<string> Warnings => _logger.ReceivedCalls().Where(call => call.GetMethodInfo().Name == "Log").Select(call => call.GetArguments()[2]!.ToString()!);

    void Destroy() => _startup.Dispose();
}

// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Authorization.for_DeploymentRegistration;

public class when_configuring_tools : given.a_deployment
{
    void Because() => Register();

    [Fact] void should_warn_about_untrusted_data() => _protocol.ServerInstructions.ShouldContain("untrusted");
    [Fact] void should_identify_stop_job() => _protocol.ServerInstructions.ShouldContain("stop_job");
    [Fact] void should_identify_resume_job() => _protocol.ServerInstructions.ShouldContain("resume_job");
    [Fact] void should_identify_delete_job() => _protocol.ServerInstructions.ShouldContain("delete_job");
}

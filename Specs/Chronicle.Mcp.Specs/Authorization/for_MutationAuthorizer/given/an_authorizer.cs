// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Authorization.for_MutationAuthorizer.given;

public class an_authorizer : Specs.given.a_policy
{
    protected MutationAuthorizer _authorizer;
    protected AuthorizationDecision _decision;

    void Establish() => _authorizer = new(_policy, _configuration, _clock);
}

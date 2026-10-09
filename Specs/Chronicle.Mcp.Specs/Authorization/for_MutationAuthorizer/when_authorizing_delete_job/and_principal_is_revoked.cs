// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Authorization.for_MutationAuthorizer.when_authorizing_delete_job;

public class and_principal_is_revoked : given.an_authorizer
{
    void Establish() { _settings.Authorization.RevokedPrincipals = [Principal]; }
    void Because() => _decision = _authorizer.Authorize(_catalog.Find("delete_job"), _arguments, _settings);
    [Fact] void should_deny_delete_job() => _decision.Code.ShouldEqual(DenialCodes.PrincipalRevoked);
}

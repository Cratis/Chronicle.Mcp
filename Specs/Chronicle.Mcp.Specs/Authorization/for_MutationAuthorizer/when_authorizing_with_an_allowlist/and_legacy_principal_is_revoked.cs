// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Authorization.for_MutationAuthorizer.when_authorizing_with_an_allowlist;

public class and_legacy_principal_is_revoked : given.an_allowlist
{
    void Establish()
    {
        _settings.Authorization.Principal = Principal;
        _settings.Authorization.Grants = [_grant];
        _settings.Authorization.RevokedPrincipals = [Principal];
    }

    void Because() => _decision = _authorizer.Authorize(_catalog.Find("stop_job"), _arguments, _settings);
    [Fact] void should_not_bypass_revocation() => _decision.Code.ShouldEqual(DenialCodes.PrincipalRevoked);
}

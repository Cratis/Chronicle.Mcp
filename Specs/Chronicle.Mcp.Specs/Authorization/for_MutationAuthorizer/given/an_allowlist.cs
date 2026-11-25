// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Mcp.Configuration;

namespace Cratis.Chronicle.Mcp.Authorization.for_MutationAuthorizer.given;

public class an_allowlist : an_authorizer
{
    protected MutationAllowlistEntry _entry;

    void Establish()
    {
        _entry = new() { EventStore = Store, Namespace = Namespace, Operations = ["stop_job"] };
        _settings.Authorization = new() { Allowlist = [_entry], ExpiresAt = Now.AddHours(1) };
    }
}

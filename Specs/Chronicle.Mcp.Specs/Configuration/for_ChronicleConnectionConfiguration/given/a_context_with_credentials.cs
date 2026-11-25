// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Configuration.for_ChronicleConnectionConfiguration.given;

public class a_context_with_credentials : a_connection_configuration
{
    void Establish()
    {
        _context.AccessToken = null;
        _context.ClientId = ReservedClientId;
        _context.ClientSecret = ReservedClientSecret;
    }
}

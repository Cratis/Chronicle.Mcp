// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Connections;

namespace Cratis.Chronicle.Mcp.Configuration.for_MutationCredentialPolicy.given;

public class resolved_development_credentials : Specification
{
    protected ChronicleConnectionString _connection;
    protected McpServerOptions _settings = new() { Profile = DeploymentProfile.Mutation };
    protected Exception? _error;

    void Establish() => _connection = new ChronicleConnectionString("chronicle://localhost:35000").WithCredentials(ChronicleConnectionString.DevelopmentClient, ChronicleConnectionString.DevelopmentClientSecret);
}

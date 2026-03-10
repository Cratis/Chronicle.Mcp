// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Mcp.Api;

namespace Cratis.Chronicle.Mcp.Specs.given;

/// <summary>
/// Base context providing access to the shared Chronicle fixture
/// with a pre-configured <see cref="ChronicleApiClient"/> and <see cref="ChronicleClient"/>.
/// </summary>
public class a_chronicle_fixture(ChronicleFixture fixture) : Specification
{
    /// <summary>
    /// Gets the <see cref="ChronicleApiClient"/> configured to talk to the running Chronicle container.
    /// </summary>
    protected ChronicleApiClient ApiClient => fixture.ApiClient;

    /// <summary>
    /// Gets the <see cref="ChronicleClient"/> configured to talk to the running Chronicle container.
    /// </summary>
    protected ChronicleClient Client => fixture.Client;
}

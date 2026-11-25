// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Connections;

namespace Cratis.Chronicle.Mcp.Configuration.for_ChronicleDestination;

public class when_a_fixed_cache_destination_is_selected : Specification
{
    bool _result;

    void Because() => _result = ChronicleDestination.IsFixed(new ChronicleConnectionString("chronicle://server.example:35000"));

    [Fact] void should_allow_destination_bound_caching() => _result.ShouldBeTrue();
}

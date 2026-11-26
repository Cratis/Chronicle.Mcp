// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Specs.Tools.for_ObserverTools.when_diagnosing_an_observer;

public class when_the_observer_identifier_is_blank : given.an_observer_query
{
    Exception _error;

    async Task Because()
    {
        ClearCalls();
        _error = await Catch.Exception(() => Diagnose((Store, Namespace, " ", Sequence)));
    }

    [Fact] void should_reject_the_identifier() => Assert.Equal("observerId", Assert.IsType<ArgumentException>(_error).ParamName);
    [Fact] void should_not_access_chronicle() => ShouldOnlyRead(0, 0, 0);
}

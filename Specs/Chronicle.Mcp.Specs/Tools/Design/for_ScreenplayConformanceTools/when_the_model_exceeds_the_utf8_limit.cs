// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using NSubstitute;

namespace Cratis.Chronicle.Mcp.Tools.Design.for_ScreenplayConformanceTools;

public class when_the_model_exceeds_the_utf8_limit : a_namespace_audit
{
    void Establish() => Argument("modelSource", string.Concat(Enumerable.Repeat(new string('é', 1023) + "\n", 33)));

    Task Because() => Audit();

    [Fact]
    void should_refuse_to_compile_the_model() => Body.GetProperty("error").GetProperty("code").GetString().ShouldEqual("conformance_model_limit");

    [Fact]
    void should_not_read_chronicle() => _reads.ReceivedCalls().ShouldBeEmpty();
}

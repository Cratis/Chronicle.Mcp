// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Configuration.for_ChronicleConnectionConfiguration;

public class when_context_credentials_belong_to_another_port : given.a_context_with_credentials
{
    void Establish()
    {
        _options.ConnectionString = "chronicle://server.example:35001?skipTlsValidation=false";
        EnableCli();
    }

    void Because() => _exception = Catch.Exception(Resolve);

    [Fact] void should_not_send_the_context_credentials() => _exception.ShouldBeOfExactType<InvalidOperationException>();
    [Fact] void should_not_disclose_the_secret() => _exception!.Message.Contains(ReservedClientSecret, StringComparison.Ordinal).ShouldBeFalse();
}

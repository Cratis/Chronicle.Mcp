// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Configuration.for_ChronicleConnectionConfiguration;

public class when_login_would_skip_tls_validation : given.a_connection_configuration
{
    void Establish()
    {
        _options.ConnectionString = "chronicle://server.example:35000?skipTlsValidation=true";
        EnableCli();
    }

    void Because() => _exception = Catch.Exception(Resolve);

    [Fact] void should_not_send_the_login_token() => _exception.ShouldBeOfExactType<InvalidOperationException>();
    [Fact] void should_not_disclose_the_token() => _exception!.Message.Contains("synthetic-login-token", StringComparison.Ordinal).ShouldBeFalse();
}

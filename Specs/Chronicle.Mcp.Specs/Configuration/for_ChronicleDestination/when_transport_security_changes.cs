// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Connections;

namespace Cratis.Chronicle.Mcp.Configuration.for_ChronicleDestination;

public class when_transport_security_changes : Specification
{
    string _secure;
    string _plaintext;
    string _unvalidated;

    void Because()
    {
        _secure = ChronicleDestination.ForTokenCache(new ChronicleConnectionString("chronicle://server.example:35000?skipTlsValidation=false"));
        _plaintext = ChronicleDestination.ForTokenCache(new ChronicleConnectionString("chronicle://server.example:35000?disableTls=true"));
        _unvalidated = ChronicleDestination.ForTokenCache(new ChronicleConnectionString("chronicle://server.example:35000?skipTlsValidation=true"));
    }

    [Fact] void should_not_reuse_tls_tokens_over_plaintext() => _secure.ShouldNotEqual(_plaintext);
    [Fact] void should_separate_tls_validation_settings() => _secure.ShouldNotEqual(_unvalidated);
}

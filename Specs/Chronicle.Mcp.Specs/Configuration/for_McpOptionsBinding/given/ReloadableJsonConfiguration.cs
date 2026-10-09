// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

namespace Cratis.Chronicle.Mcp.Configuration.for_McpOptionsBinding.given;

public class ReloadableJsonConfiguration(JsonStreamConfigurationSource source) : JsonStreamConfigurationProvider(source), IConfigurationSource
{
    public IConfigurationProvider Build(IConfigurationBuilder builder) => this;

    public void Replace(string content)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        Load(stream);
    }

    public void Publish() => OnReload();
}

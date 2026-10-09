// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Configuration;

namespace Cratis.Chronicle.Mcp.Configuration.for_McpOptionsBinding.given;

public class ReloadableMemoryConfiguration : ConfigurationProvider, IConfigurationSource
{
    public IConfigurationProvider Build(IConfigurationBuilder builder) => this;

    public void Replace(IEnumerable<KeyValuePair<string, string?>> settings) => Data = settings.ToDictionary(setting => setting.Key, setting => setting.Value, StringComparer.OrdinalIgnoreCase);

    public void Publish() => OnReload();
}

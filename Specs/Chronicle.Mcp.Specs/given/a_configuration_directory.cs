// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Specs.given;

public class a_configuration_directory : Specification
{
    protected string _directory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../.ai-work/mcp-61/spec-data", Guid.NewGuid().ToString()));

    void Establish() => Directory.CreateDirectory(_directory);

    void Destroy() => Directory.Delete(_directory, recursive: true);
}

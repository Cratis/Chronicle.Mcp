// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Tools.Design.for_ConfinedCatalogReader;

public static class LinuxCatalogPlatform
{
    public const string RequiresLinux = "Requires the Linux catalog reader.";

    public static bool IsLinux => OperatingSystem.IsLinux();
}

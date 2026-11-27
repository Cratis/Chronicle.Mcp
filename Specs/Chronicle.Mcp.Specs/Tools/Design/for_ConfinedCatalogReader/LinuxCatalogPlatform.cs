// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Mcp.Tools.Design.Conformance;

namespace Cratis.Chronicle.Mcp.Tools.Design.for_ConfinedCatalogReader;

public static class LinuxCatalogPlatform
{
    public const string RequiresLinux = "Requires 64-bit Linux on x64 or arm64.";

    public static bool SupportsCatalog => LinuxCatalogInterop.GetFlags().HasValue;
}

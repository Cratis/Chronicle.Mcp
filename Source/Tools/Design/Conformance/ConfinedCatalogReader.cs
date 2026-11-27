// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Win32.SafeHandles;

namespace Cratis.Chronicle.Mcp.Tools.Design.Conformance;

/// <summary>
/// Reads bounded snapshots of regular .play files beneath a pinned Linux root.
/// </summary>
/// <param name="flags">The native open options for this host.</param>
/// <param name="root">The owned handle for the pinned directory.</param>
internal sealed class ConfinedCatalogReader(LinuxCatalogInterop.OpenFlags flags, SafeFileHandle root) : IDisposable
{
    /// <inheritdoc/>
    public void Dispose() => root.Dispose();

    /// <summary>Reads an already-verified descriptor, rejecting any change in its recorded length.</summary>
    /// <param name="document">The opened regular file.</param>
    /// <param name="expectedSize">The bounded size previously reported by the descriptor.</param>
    /// <param name="cancellationToken">The caller's cancellation token.</param>
    /// <returns>The retrieved bytes, or null on a read failure or change in size.</returns>
    internal static byte[]? ReadContents(SafeFileHandle document, int expectedSize, CancellationToken cancellationToken)
    {
        var buffer = new byte[expectedSize + 1];
        var length = 0;
        while (length <= expectedSize)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var read = LinuxCatalogInterop.Read(document, buffer.AsSpan(length));
            if (read < 0) return null;
            if (read == 0) return length == expectedSize ? buffer[..length] : null;
            length += read;
        }

        return null;
    }

    /// <summary>Opens and validates the entire relative path, then reads from that same descriptor.</summary>
    /// <param name="path">The relative document path.</param>
    /// <param name="budget">The remaining aggregate byte budget.</param>
    /// <param name="cancellationToken">The caller's cancellation token.</param>
    /// <returns>The bytes on success, or whether refusal was due to the byte budget.</returns>
    internal (byte[]? Bytes, bool OverBudget) Read(string path, int budget, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!CatalogPaths.TryDocument(path, out _)) return (null, false);
        var (handle, _) = LinuxCatalogInterop.OpenDocument(flags, root, path);
        using var document = handle;
        if (document is null || !LinuxCatalogInterop.TryGetRegularSize(document, out var size)) return (null, false);
        if (size > (ulong)budget) return (null, true);
        return (ReadContents(document, (int)size, cancellationToken), false);
    }
}

// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Cratis.Chronicle.Mcp.Tools.Design.Conformance;

/// <summary>
/// The Linux UAPI used to confine complete path lookups and inspect the opened descriptor.
/// </summary>
internal static partial class LinuxCatalogInterop
{
    /// <summary>EINTR from asm-generic/errno-base.h.</summary>
    const int Interrupted = 4;
    const int MaxAttempts = 4;

    /// <summary>AT_EMPTY_PATH from linux/fcntl.h.</summary>
    const int EmptyPath = 0x1000;

    /// <summary>STATX_TYPE | STATX_SIZE from linux/stat.h.</summary>
    const uint TypeAndSize = 0x0201;
    const ushort FileType = 0xf000;
    const ushort Regular = 0x8000;

    /// <summary>Gets the exact native open flags for the supported Linux ABIs.</summary>
    /// <returns>The flags or null when confinement is not supported.</returns>
    internal static OpenFlags? GetFlags() => !OperatingSystem.IsLinux() || !Environment.Is64BitProcess ? null : RuntimeInformation.OSArchitecture switch
    {
        // O_DIRECTORY | O_PATH | O_CLOEXEC; O_RDONLY | O_NONBLOCK | O_NOCTTY | O_CLOEXEC;
        // RESOLVE_BENEATH | RESOLVE_NO_SYMLINKS for both, plus RESOLVE_NO_XDEV for documents (linux/openat2.h).
        // x64: asm-generic/fcntl.h and arch/x86/entry/syscalls/syscall_64.tbl.
        Architecture.X64 => new(437, 0x10000 | 0x200000 | 0x80000, 0 | 0x800 | 0x100 | 0x80000, 0x08 | 0x04, 0x08 | 0x04 | 0x01),

        // arm64: arch/arm64/include/uapi/asm/fcntl.h, asm-generic/{fcntl,unistd}.h.
        Architecture.Arm64 => new(437, 0x4000 | 0x200000 | 0x80000, 0 | 0x800 | 0x100 | 0x80000, 0x08 | 0x04, 0x08 | 0x04 | 0x01),
        _ => null
    };

    /// <summary>Opens the trusted filesystem root for resolving the configured directory.</summary>
    /// <param name="flags">The native open options.</param>
    /// <returns>The owned anchor handle or null on failure.</returns>
    internal static SafeFileHandle? OpenAnchor(OpenFlags flags) => ToHandle(Retry(() => Capture(Open("/", flags.Directory))).Value);

    /// <summary>Pins a configured root without following symlinks, allowing operator mounts on the way.</summary>
    /// <param name="flags">The native open options.</param>
    /// <param name="anchor">The filesystem root handle.</param>
    /// <param name="relative">The validated relative path of the configured directory.</param>
    /// <returns>The owned root directory handle or null on failure.</returns>
    internal static SafeFileHandle? OpenRoot(OpenFlags flags, SafeFileHandle anchor, string relative) =>
        OpenRelative(flags.OpenAt2, anchor, relative, flags.Directory, flags.ResolveRoot).Handle;

    /// <summary>Resolves a complete document path beneath the pinned root, with no links or mount crossings.</summary>
    /// <param name="flags">The native open options.</param>
    /// <param name="root">The pinned root directory.</param>
    /// <param name="relative">The validated relative document path.</param>
    /// <returns>The owned document handle on success, or the native failure code.</returns>
    internal static (SafeFileHandle? Handle, int Error) OpenDocument(OpenFlags flags, SafeFileHandle root, string relative) =>
        OpenRelative(flags.OpenAt2, root, relative, flags.Document, flags.ResolveDocument);

    /// <summary>Verifies that the opened descriptor is regular and has an available size.</summary>
    /// <param name="fd">The opened file descriptor.</param>
    /// <param name="size">The file size when verified.</param>
    /// <returns>Whether both type and size could be verified.</returns>
    internal static bool TryGetRegularSize(SafeFileHandle fd, out ulong size)
    {
        StatxData data = default;
        var result = Retry(() => Capture(Statx(fd, string.Empty, EmptyPath, TypeAndSize, out data)));
        size = data.Size;
        return result.Value == 0 && (data.Mask & TypeAndSize) == TypeAndSize && (data.Mode & FileType) == Regular;
    }

    /// <summary>Reads at most the supplied buffer length from the verified descriptor.</summary>
    /// <param name="fd">The verified file descriptor.</param>
    /// <param name="buffer">The destination.</param>
    /// <returns>The number of bytes read or -1 on failure.</returns>
    internal static unsafe int Read(SafeFileHandle fd, Span<byte> buffer)
    {
        fixed (byte* destination = buffer)
        {
            var address = (nint)destination;
            var length = (nuint)buffer.Length;
            var result = Retry(() => Capture(Read(fd, address, length))).Value;
            return result >= 0 && (nuint)result <= length ? (int)result : -1;
        }
    }

    /// <summary>Retries only EINTR and returns every other failure without weakening the operation.</summary>
    /// <param name="call">The unchanged native operation.</param>
    /// <returns>The final native result and error code.</returns>
    internal static (nint Value, int Error) Retry(Func<(nint Value, int Error)> call)
    {
        for (var attempt = 1; ; attempt++)
        {
            var result = call();
            if (result.Error != Interrupted || attempt == MaxAttempts) return result;
        }
    }

    static (SafeFileHandle? Handle, int Error) OpenRelative(nint syscall, SafeFileHandle directory, string path, int open, ulong resolve)
    {
        var how = new OpenHow { Flags = (ulong)open, Resolve = resolve };
        var (value, error) = Retry(() => Capture(Syscall(syscall, directory, path, in how, 24)));
        return (ToHandle(value), error);
    }

    static (nint Value, int Error) Capture(nint result) => (result, result < 0 ? Marshal.GetLastPInvokeError() : 0);

    static SafeFileHandle? ToHandle(nint fd) => fd >= 0 ? new SafeFileHandle(fd, ownsHandle: true) : null;

    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    [LibraryImport("libc", EntryPoint = "open", SetLastError = true, StringMarshalling = StringMarshalling.Utf8)]
    private static partial int Open(string path, int flags);

    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    [LibraryImport("libc", EntryPoint = "syscall", SetLastError = true, StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint Syscall(nint number, SafeFileHandle directory, string path, in OpenHow how, nuint size);

    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    [LibraryImport("libc", EntryPoint = "statx", SetLastError = true, StringMarshalling = StringMarshalling.Utf8)]
    private static partial int Statx(SafeFileHandle fd, string path, int flags, uint mask, out StatxData data);

    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    [LibraryImport("libc", EntryPoint = "read", SetLastError = true)]
    private static partial nint Read(SafeFileHandle fd, nint buffer, nuint count);

    /// <summary>The syscall and composed flags for one native ABI (not passed to libc).</summary>
    /// <param name="OpenAt2">The openat2 syscall number.</param>
    /// <param name="Directory">The directory open flags.</param>
    /// <param name="Document">The document open flags.</param>
    /// <param name="ResolveRoot">The resolution policy for the configured root.</param>
    /// <param name="ResolveDocument">The resolution policy for a document under that root.</param>
    [StructLayout(LayoutKind.Auto)]
    internal readonly record struct OpenFlags(nint OpenAt2, int Directory, int Document, ulong ResolveRoot, ulong ResolveDocument);

    [StructLayout(LayoutKind.Sequential)]
    struct OpenHow
    {
        public ulong Flags;
        public ulong Mode;
        public ulong Resolve;
    }

    /// <summary>The fixed linux/stat.h layout shared by both supported ABIs.</summary>
    [StructLayout(LayoutKind.Explicit, Size = 256)]
    struct StatxData
    {
        [FieldOffset(0)]
        public uint Mask;

        [FieldOffset(28)]
        public ushort Mode;

        [FieldOffset(40)]
        public ulong Size;
    }
}

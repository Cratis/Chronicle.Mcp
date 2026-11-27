// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using Cratis.Chronicle.Mcp.Tools.Design.Conformance;

namespace Cratis.Chronicle.Mcp.Tools.Design.for_ConfinedCatalogReader;

[PerFactSpecificationLifecycle]
public class when_opening_documents : Specification
{
    DirectoryInfo _sandbox;
    string _root;
    string _outside;

    static CancellationToken Cancellation => TestContext.Current.CancellationToken;

    void Establish()
    {
        _sandbox = Directory.CreateTempSubdirectory("mcp86-reader-");
        _root = Directory.CreateDirectory(Path.Combine(_sandbox.FullName, "root")).FullName;
        _outside = Directory.CreateDirectory(Path.Combine(_sandbox.FullName, "outside")).FullName;
        File.WriteAllText(Path.Combine(_root, "model.play"), "inside");
        File.WriteAllText(Path.Combine(_outside, "model.play"), "outside");
    }

    void Destroy() => _sandbox.Delete(recursive: true);

    [Fact(Skip = LinuxCatalogPlatform.RequiresLinux, SkipType = typeof(LinuxCatalogPlatform), SkipUnless = nameof(LinuxCatalogPlatform.IsLinux))]
    void should_read_the_original_root_after_its_path_is_replaced()
    {
        using var reader = OpenReader();
        Directory.Move(_root, Path.Combine(_sandbox.FullName, "pinned"));
        Directory.Move(_outside, _root);

        reader.Read("model.play", 6, Cancellation).Bytes.ShouldEqual("inside"u8.ToArray());
    }

    [Theory(Skip = LinuxCatalogPlatform.RequiresLinux, SkipType = typeof(LinuxCatalogPlatform), SkipUnless = nameof(LinuxCatalogPlatform.IsLinux))]
    [InlineData("inside.play")]
    [InlineData("escape.play")]
    [InlineData("parent/model.play")]
    [InlineData("/model.play")]
    [InlineData("../outside/model.play")]
    [InlineData("C:/model.play")]
    [InlineData("parent\\model.play")]
    [InlineData("model.play\0")]
    [InlineData("directory.play")]
    [InlineData("fifo.play")]
    [InlineData("sparse.play")]
    void should_refuse_nonregular_unconfined_or_oversized_entries(string entry)
    {
        File.CreateSymbolicLink(Path.Combine(_root, "inside.play"), "model.play");
        File.CreateSymbolicLink(Path.Combine(_root, "escape.play"), "../outside/model.play");
        Directory.CreateSymbolicLink(Path.Combine(_root, "parent"), _outside);
        Directory.CreateDirectory(Path.Combine(_root, "directory.play"));
        MakeFifo(Path.Combine(_root, "fifo.play"));
        using (var sparse = File.Create(Path.Combine(_root, "sparse.play"))) sparse.SetLength(1024 * 1024);
        using var reader = OpenReader();

        reader.Read(entry, 6, Cancellation).Bytes.ShouldBeNull();
    }

    [Theory(Skip = LinuxCatalogPlatform.RequiresLinux, SkipType = typeof(LinuxCatalogPlatform), SkipUnless = nameof(LinuxCatalogPlatform.IsLinux))]
    [InlineData(2)]
    [InlineData(8)]
    void should_refuse_a_document_that_changes_length_on_the_open_descriptor(int changedLength)
    {
        var flags = LinuxCatalogInterop.GetFlags()!.Value;
        using var anchor = LinuxCatalogInterop.OpenAnchor(flags);
        using var root = LinuxCatalogInterop.OpenRoot(flags, anchor!, _root[1..]);
        using var document = LinuxCatalogInterop.OpenDocument(flags, root!, "model.play").Handle;
        LinuxCatalogInterop.TryGetRegularSize(document!, out var size).ShouldBeTrue();
        using (var writer = File.OpenWrite(Path.Combine(_root, "model.play"))) writer.SetLength(changedLength);

        ConfinedCatalogReader.ReadContents(document!, (int)size, Cancellation).ShouldBeNull();
    }

    [Fact(Skip = LinuxCatalogPlatform.RequiresLinux, SkipType = typeof(LinuxCatalogPlatform), SkipUnless = nameof(LinuxCatalogPlatform.IsLinux))]
    void should_refuse_crossing_the_proc_mount_from_the_filesystem_root()
    {
        File.Exists("/proc/version").ShouldBeTrue();
        var flags = LinuxCatalogInterop.GetFlags()!.Value;
        using var anchor = LinuxCatalogInterop.OpenAnchor(flags);
        var result = LinuxCatalogInterop.OpenDocument(flags, anchor!, "proc/version");
        using var opened = result.Handle;

        opened.ShouldBeNull();
        result.Error.ShouldEqual(18); // EXDEV, asm-generic/errno-base.h
    }

    [Fact(Skip = LinuxCatalogPlatform.RequiresLinux, SkipType = typeof(LinuxCatalogPlatform), SkipUnless = nameof(LinuxCatalogPlatform.IsLinux))]
    async Task should_never_read_an_outside_file_while_the_leaf_is_swapped()
    {
        using var reader = OpenReader();
        using var stop = CancellationTokenSource.CreateLinkedTokenSource(Cancellation);
        var leaf = Path.Combine(_root, "model.play");
        var inside = Path.Combine(_root, "saved.play");
        reader.Read("model.play", 8, Cancellation).Bytes.ShouldEqual("inside"u8.ToArray());
        var inProgress = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var swap = Task.Run(() => SwapLeaf(leaf, inside, inProgress, stop.Token), Cancellation);
        try
        {
            await Task.WhenAny(inProgress.Task, swap).WaitAsync(Cancellation);
            if (swap.IsCompleted) await swap;
            for (var attempt = 0; attempt < 256; attempt++)
            {
                var outcome = reader.Read("model.play", 8, Cancellation).Bytes;
                outcome?.ShouldEqual("inside"u8.ToArray());
            }
        }
        finally
        {
            await stop.CancelAsync();
            await swap;
        }
    }

    [Theory]
    [InlineData(4, 4)] // EINTR is bounded.
    [InlineData(1, 1)] // EPERM (e.g. seccomp) is not retried.
    [InlineData(11, 1)] // EAGAIN is not retried.
    [InlineData(38, 1)] // ENOSYS is not retried.
    void should_preserve_the_failure_without_weakening_the_operation(int error, int attempts)
    {
        var calls = 0;
        var result = LinuxCatalogInterop.Retry(() =>
        {
            calls++;
            return (-1, error);
        });

        result.ShouldEqual((-1, error));
        calls.ShouldEqual(attempts);
    }

    void SwapLeaf(string leaf, string inside, TaskCompletionSource inProgress, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            File.Move(leaf, inside);
            File.CreateSymbolicLink(leaf, Path.Combine(_outside, "model.play"));
            inProgress.TrySetResult();
            File.Delete(leaf);
            File.Move(inside, leaf);
        }
    }

    ConfinedCatalogReader OpenReader()
    {
        var flags = LinuxCatalogInterop.GetFlags()!.Value;
        using var anchor = LinuxCatalogInterop.OpenAnchor(flags);
        var root = LinuxCatalogInterop.OpenRoot(flags, anchor!, _root[1..]);
        root.ShouldNotBeNull();
        return new(flags, root!);
    }

    static void MakeFifo(string path)
    {
        var command = new ProcessStartInfo("mkfifo") { ArgumentList = { "-m", "600", path } };
        using var process = Process.Start(command)!;
        process.WaitForExit(5000).ShouldBeTrue();
        process.ExitCode.ShouldEqual(0);
    }
}

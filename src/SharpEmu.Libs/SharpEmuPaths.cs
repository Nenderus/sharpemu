// Copyright (C) 2026 SharpEmu Emulator Project
// SPDX-License-Identifier: GPL-2.0-or-later

using System;
using System.IO;

namespace SharpEmu.Libs;

/// <summary>
/// Resolves the root directory that holds user-writable data: GUI settings,
/// the library cache, custom environment and per-game configs, save data,
/// logs and dumps. On Linux the data always lives under the XDG data
/// directory so a system-installed build never writes next to a read-only
/// executable; Windows and macOS keep the portable layout with the data
/// next to the executable. <c>SHARPEMU_USER_DIR</c> overrides the choice on
/// every platform.
/// </summary>
public static class SharpEmuPaths
{
    public static string DataRoot { get; } = ResolveDataRoot();

    private static string ResolveDataRoot()
    {
        var configured = Environment.GetEnvironmentVariable("SHARPEMU_USER_DIR");
        if (!string.IsNullOrWhiteSpace(configured))
        {
            return Path.GetFullPath(configured);
        }

        if (!OperatingSystem.IsLinux())
        {
            return AppContext.BaseDirectory;
        }

        var dataHome = Environment.GetEnvironmentVariable("XDG_DATA_HOME");
        if (string.IsNullOrWhiteSpace(dataHome))
        {
            dataHome = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".local",
                "share");
        }

        var root = Path.Combine(dataHome, "SharpEmu");
        try
        {
            // Create the root eagerly: consumers only create their own
            // subdirectories, and a missing root would fail the very first
            // settings write.
            Directory.CreateDirectory(root);
        }
        catch (Exception)
        {
            // Writers report the failure themselves; do not turn a bad root
            // into a startup crash.
        }

        return root;
    }
}

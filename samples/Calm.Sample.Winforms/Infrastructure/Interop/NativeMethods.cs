using System.Runtime.InteropServices;

namespace Calm.Sample.Winforms.Infrastructure.Interop;

/// <summary>
/// A common pattern used to organize and wrap Platform Invocation Services (P/Invoke) declarations.
/// </summary>
internal static partial class NativeMethods
{
    /// <summary>
    /// Brings the thread that created the specified window into the foreground and activates the window.
    /// Keyboard input is directed to the window, and various visual cues are changed for the user.
    /// The system assigns a slightly higher priority to the thread that created the foreground window
    /// than it does to other threads.
    /// </summary>
    /// <param name="hWnd">A handle to the window that should be activated and brought to the foreground.</param>
    /// <returns>If the window was brought to the foreground, the return value is nonzero.
    /// If the window was not brought to the foreground, the return value is zero.</returns>
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [LibraryImport("user32.dll", EntryPoint = "SetForegroundWindow")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool SetForegroundWindow(IntPtr hWnd);

    /// <summary>
    /// Determines whether the specified window is minimized (iconic).
    /// </summary>
    /// <param name="hWnd">A handle to the window to be tested.</param>
    /// <returns>If the window is iconic, the return value is nonzero.
    /// If the window is not iconic, the return value is zero.</returns>
    /// [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [LibraryImport("user32.dll", EntryPoint = "IsIconic")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool IsIconic(IntPtr hWnd);

    /// <summary>
    /// Sets the specified window's show state.
    /// </summary>
    /// <param name="hWnd">A handle to the window.</param>
    /// <param name="nCmdShow">Controls how the window is to be shown.</param>
    /// <returns>If the window was previously visible, the return value is nonzero.
    /// If the window was previously hidden, the return value is zero.
    /// </returns>
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool ShowWindow(IntPtr hWnd, int nCmdShow);

    /// <summary>
    /// Controls how the window is to be shown.
    /// </summary>
    internal static class CmdShow
    {
        /// <summary>
        /// Hides the window and activates another window.
        /// </summary>
        public const int SW_HIDE = 0;

        /// <summary>
        /// Activates and displays a window. If the window is minimized, maximized, or arranged,
        /// the system restores it to its original size and position.
        /// An application should specify this flag when displaying the window for the first time.
        /// </summary>
        public const int SW_SHOWNORMAL = 1;

        /// <summary>
        /// Activates and displays a window. If the window is minimized, maximized, or arranged,
        /// the system restores it to its original size and position.
        /// An application should specify this flag when displaying the window for the first time.
        /// </summary>
        public const int SW_NORMAL = 1;

        /// <summary>
        /// Activates the window and displays it as a minimized window.
        /// </summary>
        public const int SW_SHOWMINIMIZED = 2;

        /// <summary>
        /// Activates the window and displays it as a maximized window.
        /// </summary>
        public const int SW_SHOWMAXIMIZED = 3;

        /// <summary>
        /// Activates the window and displays it as a maximized window.
        /// </summary>
        public const int SW_MAXIMIZE = 3;

        /// <summary>
        /// Displays a window in its most recent size and position.
        /// This value is similar to SW_SHOWNORMAL, except that the window is not activated.
        /// </summary>
        public const int SW_SHOWNOACTIVATE = 4;

        /// <summary>
        /// Activates the window and displays it in its current size and position.
        /// </summary>
        public const int SW_SHOW = 5;

        /// <summary>
        /// Minimizes the specified window and activates the next top-level window in the Z order.
        /// </summary>
        public const int SW_MINIMIZE = 6;

        /// <summary>
        /// Displays the window as a minimized window.
        /// This value is similar to SW_SHOWMINIMIZED, except the window is not activated.
        /// </summary>
        public const int SW_SHOWMINNOACTIVE = 7;

        /// <summary>
        /// Displays the window in its current size and position.
        /// This value is similar to SW_SHOW, except that the window is not activated.
        /// </summary>
        public const int SW_SHOWNA = 8;

        /// <summary>
        /// Activates and displays the window. If the window is minimized, maximized, or arranged,
        /// the system restores it to its original size and position.
        /// An application should specify this flag when restoring a minimized window.
        /// </summary>
        public const int SW_RESTORE = 9;

        /// <summary>
        /// Sets the show state based on the SW_ value specified in the STARTUPINFO structure
        /// passed to the CreateProcess function by the program that started the application.
        /// </summary>
        public const int SW_SHOWDEFAULT = 10;

        /// <summary>
        /// Minimizes a window, even if the thread that owns the window is not responding.
        /// This flag should only be used when minimizing windows from a different thread.
        /// </summary>
        public const int SW_FORCEMINIMIZE = 11;
    }

    /// <summary>
    /// Compares two Unicode strings. Digits in the strings are considered
    /// as numerical content rather than text. This test is not case-sensitive.
    /// </summary>
    /// <param name="psz1">A pointer to the first null-terminated string to be compared.</param>
    /// <param name="psz2">A pointer to the second null-terminated string to be compared.</param>
    /// <returns>
    /// Returns zero if the strings are identical.
    /// Returns 1 if the string pointed to by psz1 has a greater value than that pointed to by psz2.
    /// Returns - 1 if the string pointed to by psz1 has a lesser value than that pointed to by psz2.
    /// </returns>
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [LibraryImport("shlwapi.dll", StringMarshalling = StringMarshalling.Utf16)]
    public static partial int StrCmpLogicalW(string psz1, string psz2);
}

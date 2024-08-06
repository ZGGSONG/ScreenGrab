using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace ScreenGrab.Extensions;

public static class WpfExtensions
{
    /// <summary>
    ///     Gets the absolute position of the specified window.
    /// </summary>
    /// <param name="w">The window for which to get the absolute position.</param>
    /// <returns>A <see cref="Point" /> representing the absolute position of the window.</returns>
    public static Point GetAbsolutePosition(this Window w)
    {
        // If the window is not maximized, return the top-left corner position of the window
        if (w.WindowState != WindowState.Maximized)
            return new Point(w.Left, w.Top);

        Int32Rect r;
        // Check if the system supports multiple monitors
        var multimonSupported = OSInterop.GetSystemMetrics(OSInterop.SM_CMONITORS) != 0;
        if (!multimonSupported)
        {
            // If multiple monitors are not supported, get the work area rectangle of the primary monitor
            var rc = new OSInterop.RECT();
            OSInterop.SystemParametersInfo(48, 0, ref rc, 0);
            r = new Int32Rect(rc.left, rc.top, rc.width, rc.height);
        }
        else
        {
            // If multiple monitors are supported, get the monitor information for the monitor containing the window
            var helper = new WindowInteropHelper(w);
            var hmonitor = OSInterop.MonitorFromWindow(new HandleRef(null, helper.EnsureHandle()), 2);
            var info = new OSInterop.MONITORINFOEX();
            OSInterop.GetMonitorInfo(new HandleRef(null, hmonitor), info);
            r = new Int32Rect(info.rcMonitor.left, info.rcMonitor.top, info.rcMonitor.width, info.rcMonitor.height);
        }

        // Return the top-left corner position of the monitor
        return new Point(r.X, r.Y);
    }
}
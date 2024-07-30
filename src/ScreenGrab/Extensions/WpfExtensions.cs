using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace ScreenGrab.Extensions;

public static class WpfExtensions
{
    public static Point GetAbsolutePosition(this Window w)
    {
        if (w.WindowState != WindowState.Maximized)
            return new Point(w.Left, w.Top);

        Int32Rect r;
        var multimonSupported = OSInterop.GetSystemMetrics(OSInterop.SM_CMONITORS) != 0;
        if (!multimonSupported)
        {
            var rc = new OSInterop.RECT();
            OSInterop.SystemParametersInfo(48, 0, ref rc, 0);
            r = new Int32Rect(rc.left, rc.top, rc.width, rc.height);
        }
        else
        {
            var helper = new WindowInteropHelper(w);
            var hmonitor = OSInterop.MonitorFromWindow(new HandleRef(null, helper.EnsureHandle()), 2);
            var info = new OSInterop.MONITORINFOEX();
            OSInterop.GetMonitorInfo(new HandleRef(null, hmonitor), info);
            r = new Int32Rect(info.rcMonitor.left, info.rcMonitor.top, info.rcMonitor.width, info.rcMonitor.height);
        }

        return new Point(r.X, r.Y);
    }
}
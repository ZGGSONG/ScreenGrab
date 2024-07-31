using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using ScreenGrab.Extensions;

namespace ScreenGrab.Utilities;

public static class WindowUtilities
{
    public static bool GetMousePosition(out Point mousePosition)
    {
        if (GetCursorPos(out var point))
        {
            mousePosition = new Point(point.X, point.Y);
            return true;
        }

        mousePosition = default;
        return false;
    }

    public static bool IsMouseInWindow(this Window window)
    {
        GetMousePosition(out var mousePosition);

        var dpi = VisualTreeHelper.GetDpi(window);
        var absPosPoint = window.GetAbsolutePosition();
        Rect windowRect = new(absPosPoint.X, absPosPoint.Y,
            window.ActualWidth * dpi.DpiScaleX,
            window.ActualHeight * dpi.DpiScaleY);
        return windowRect.Contains(mousePosition);
    }

    #region Dll Import

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetCursorPos(out POINT lpPoint);

    #endregion

    #region Dll Helper Structures

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        /// <summary>
        ///     x coordinate of point.
        /// </summary>
        public int X;

        /// <summary>
        ///     y coordinate of point.
        /// </summary>
        public int Y;

        /// <summary>
        ///     Construct a point of coordinates (x,y).
        /// </summary>
        public POINT(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    #endregion
}
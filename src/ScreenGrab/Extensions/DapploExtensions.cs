using System.Windows;
using Dapplo.Windows.User32;

namespace ScreenGrab.Extensions;

public static class DapploExtensions
{
    public static Point ScaledCenterPoint(this DisplayInfo displayInfo)
    {
        Rect displayRect = displayInfo.Bounds;
        NativeMethods.GetScaleFactorForMonitor(displayInfo.MonitorHandle, out var scaleFactor);
        var scaleFraction = scaleFactor / 100.0;
        var rawCenter = displayRect.CenterPoint();
        Point displayScaledCenterPoint = new(rawCenter.X / scaleFraction, rawCenter.Y / scaleFraction);
        return displayScaledCenterPoint;
    }

    public static Point ScaledCenterPoint(this WpfScreenHelper.Screen displayInfo)
    {
        Rect displayRect = displayInfo.Bounds;
        var scaleFraction = displayInfo.ScaleFactor;
        var rawCenter = displayRect.CenterPoint();
        Point displayScaledCenterPoint = new(rawCenter.X / scaleFraction, rawCenter.Y / scaleFraction);
        return displayScaledCenterPoint;
    }

    public static Rect ScaledBounds(this DisplayInfo displayInfo)
    {
        Rect displayRect = displayInfo.Bounds;
        NativeMethods.GetScaleFactorForMonitor(displayInfo.MonitorHandle, out var scaleFactor);
        var scaleFraction = scaleFactor / 100.0;

        // Scale size and position
        Rect scaledBounds = new(
            displayRect.X / scaleFraction,
            displayRect.Y / scaleFraction,
            displayRect.Width / scaleFraction,
            displayRect.Height / scaleFraction);
        return scaledBounds;
    }

    public static Rect ScaledBounds(this WpfScreenHelper.Screen displayInfo)
    {
        Rect displayRect = displayInfo.Bounds;
        var scaleFraction = displayInfo.ScaleFactor;

        // Scale size and position
        Rect scaledBounds = new(
            displayRect.X / scaleFraction,
            displayRect.Y / scaleFraction,
            displayRect.Width / scaleFraction,
            displayRect.Height / scaleFraction);
        return scaledBounds;
    }
}
using System.Windows;
using WpfScreenHelper;

namespace ScreenGrab.Extensions;

public static class ScreenExtensions
{
    public static Rect ScaledBounds(this Screen displayInfo)
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
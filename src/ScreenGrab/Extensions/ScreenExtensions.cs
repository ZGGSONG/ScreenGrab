using System.Drawing;
using System.Windows;
using System.Windows.Media;
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

    /// <summary>
    /// 在窗口显示之前预先截取所有屏幕的图像
    /// </summary>
    public static Dictionary<Screen, ImageSource> PreCaptureAllScreens(this IEnumerable<Screen> screens)
    {
        var result = new Dictionary<Screen, ImageSource>();
        foreach (var screen in screens)
        {
            var bounds = screen.Bounds;
            using var bmp = new Bitmap((int)bounds.Width, (int)bounds.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using var g = Graphics.FromImage(bmp);
            g.CopyFromScreen((int)bounds.Left, (int)bounds.Top, 0, 0, bmp.Size, CopyPixelOperation.SourceCopy);
            var imageSource = bmp.ToImageSource();
            if (imageSource != null)
            {
                result[screen] = imageSource;
            }
        }
        return result;
    }
}
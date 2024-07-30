using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ScreenGrab.Utilities;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace ScreenGrab.Extensions;

public static class ImageExtensions
{
    public static ImageSource? GetWindowBoundsImage(this Window passedWindow)
    {
        var bmp = passedWindow.GetWindowsBoundsBitmap();
        return bmp.ToImageSource();
    }

    public static Tuple<double, double> GetWidthHeight(this Window window)
    {
        var dpi = VisualTreeHelper.GetDpi(window);
        return new Tuple<double, double>(window.ActualWidth * dpi.DpiScaleX, window.ActualHeight * dpi.DpiScaleY);
    }

    public static Bitmap GetWindowsBoundsBitmap(this Window passedWindow)
    {
        var (windowWidth, windowHeight) = passedWindow.GetWidthHeight();

        var absPosPoint = passedWindow.GetAbsolutePosition();

        var thisCorrectedLeft = (int)absPosPoint.X;
        var thisCorrectedTop = (int)absPosPoint.Y;

        Bitmap bmp = new((int)windowWidth, (int)windowHeight, PixelFormat.Format32bppArgb);
        using var g = Graphics.FromImage(bmp);

        g.CopyFromScreen(thisCorrectedLeft, thisCorrectedTop, 0, 0, bmp.Size, CopyPixelOperation.SourceCopy);
        return bmp;
    }

    public static BitmapImage? ToImageSource(this Bitmap? bitmap)
    {
        if (bitmap == null) return default;
        using MemoryStream memory = new();
        using WrappingStream wrapper = new(memory);

        bitmap.Save(wrapper, ImageFormat.Bmp);
        wrapper.Position = 0;
        BitmapImage bitmapImage = new();
        bitmapImage.BeginInit();
        bitmapImage.StreamSource = wrapper;
        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
        bitmapImage.EndInit();
        bitmapImage.StreamSource = null;
        bitmapImage.Freeze();

        memory.Flush();
        wrapper.Flush();

        return bitmapImage;
    }

    public static Bitmap GetRegionOfScreenAsBitmap(this Rectangle region)
    {
        Bitmap bmp = new(region.Width, region.Height, PixelFormat.Format32bppArgb);
        using var g = Graphics.FromImage(bmp);

        g.CopyFromScreen(region.Left, region.Top, 0, 0, bmp.Size, CopyPixelOperation.SourceCopy);
        bmp = bmp.PadImage();

        return bmp;
    }

    public static Bitmap PadImage(this Bitmap image, int minW = 64, int minH = 64)
    {
        if (image.Height >= minH && image.Width >= minW)
            return image;

        var width = Math.Max(image.Width + 16, minW + 16);
        var height = Math.Max(image.Height + 16, minH + 16);

        // Create a compatible bitmap
        Bitmap destination = new(width, height, image.PixelFormat);
        using var gd = Graphics.FromImage(destination);

        gd.Clear(image.GetPixel(0, 0));
        gd.DrawImageUnscaled(image, 8, 8);

        return destination;
    }
}
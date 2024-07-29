using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ScreenGrab.Extensions;

namespace ScreenGrab.Utilities;

public static class ImageMethods
{
    public static ImageSource GetWindowBoundsImage(Window passedWindow)
    {
        Bitmap bmp = GetWindowsBoundsBitmap(passedWindow);
        return BitmapToImageSource(bmp);
    }

    public static Bitmap GetWindowsBoundsBitmap(Window passedWindow)
    {
        DpiScale dpi = VisualTreeHelper.GetDpi(passedWindow);
        int windowWidth = (int)(passedWindow.ActualWidth * dpi.DpiScaleX);
        int windowHeight = (int)(passedWindow.ActualHeight * dpi.DpiScaleY);

        var absPosPoint = passedWindow.GetAbsolutePosition();

        int thisCorrectedLeft = (int)(absPosPoint.X);
        int thisCorrectedTop = (int)(absPosPoint.Y);

        Bitmap bmp = new(windowWidth, windowHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        using Graphics g = Graphics.FromImage(bmp);

        g.CopyFromScreen(thisCorrectedLeft, thisCorrectedTop, 0, 0, bmp.Size, CopyPixelOperation.SourceCopy);
        return bmp;
    }

    public static BitmapImage BitmapToImageSource(Bitmap bitmap)
    {
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
    
    public static Bitmap GetRegionOfScreenAsBitmap(Rectangle region)
    {
        Bitmap bmp = new(region.Width, region.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        using var g = Graphics.FromImage(bmp);

        g.CopyFromScreen(region.Left, region.Top, 0, 0, bmp.Size, CopyPixelOperation.SourceCopy);
        bmp = PadImage(bmp);

        // Singleton<HistoryService>.Instance.CacheLastBitmap(bmp);
        return bmp;
    }
    
    public static Bitmap PadImage(Bitmap image, int minW = 64, int minH = 64)
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
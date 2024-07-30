using System.Drawing;
using System.Windows;
using Dapplo.Windows.User32;
using ScreenGrab.Extensions;

namespace ScreenGrab;

public abstract class ScreenGrabber
{
    public static bool IsCapturing { get; private set; }

    public static Action<Bitmap>? OnCaptured { get; set; }

    public static void Capture(bool isPolyline = false)
    {
        if (IsCapturing) return;

        IsCapturing = true;

        var allDisplayInfos = DisplayInfo.AllDisplayInfos;
        var allScreenGrab = Application.Current.Windows.OfType<ScreenGrabView>().ToList();
        var numberOfScreenGrabWindowsToCreate = allDisplayInfos.Length - allScreenGrab.Count;

        for (var i = 0; i < numberOfScreenGrabWindowsToCreate; i++)
        {
            var view = new ScreenGrabView(OnCaptured, isPolyline)
            {
                OnGrabClose = () => IsCapturing = false
            };
            allScreenGrab.Add(view);
        }

        const double sideLength = 40;

        foreach (var (displayInfo, screenGrab) in allDisplayInfos.Zip(allScreenGrab,
                     (displayInfo, screenGrab) => (displayInfo, screenGrab)))
        {
            screenGrab.WindowStartupLocation = WindowStartupLocation.Manual;
            screenGrab.Width = sideLength;
            screenGrab.Height = sideLength;
            screenGrab.WindowState = WindowState.Normal;

            var screenCenterPoint = displayInfo.ScaledCenterPoint();
            screenGrab.Left = screenCenterPoint.X - sideLength / 2;
            screenGrab.Top = screenCenterPoint.Y - sideLength / 2;

            screenGrab.Show();
            screenGrab.Activate();
        }
    }
}
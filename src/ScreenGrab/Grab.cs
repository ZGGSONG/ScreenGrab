using System.Drawing;
using System.Windows;
using Dapplo.Windows.User32;
using ScreenGrab.Extensions;

namespace ScreenGrab;

public class Grab
{
    public Action<Bitmap>? OnImageCaptured { get; set; }

    public void Capture()
    {
        var allDisplayInfos = DisplayInfo.AllDisplayInfos;
        var allScreenGrab = Application.Current.Windows.OfType<ScreenGrabView>().ToList();
        var numberOfScreenGrabWindowsToCreate = allDisplayInfos.Length - allScreenGrab.Count;

        for (var i = 0; i < numberOfScreenGrabWindowsToCreate; i++)
            allScreenGrab.Add(new ScreenGrabView(OnImageCaptured));

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
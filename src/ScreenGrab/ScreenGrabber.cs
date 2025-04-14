using System.Drawing;
using System.Windows;
using ScreenGrab.Extensions;
using WpfScreenHelper;

namespace ScreenGrab;

public abstract class ScreenGrabber
{
    public static bool IsCapturing { get; private set; }

    public static Action<Bitmap>? OnCaptured { get; set; }

    public static void Capture(bool isAuxiliary = false)
    {
        if (IsCapturing) return;

        IsCapturing = true;

        var allScreens = Screen.AllScreens;

        var allScreenGrab = Application.Current.Windows.OfType<ScreenGrabView>().ToList();
        var numberOfScreenGrabWindowsToCreate = allScreens.Count() - allScreenGrab.Count;

        for (var i = 0; i < numberOfScreenGrabWindowsToCreate; i++)
        {
            var view = new ScreenGrabView(OnCaptured, isAuxiliary)
            {
                OnGrabClose = () => IsCapturing = false
            };
            allScreenGrab.Add(view);
        }

        foreach (var (screen, screenGrab) in allScreens.Zip(allScreenGrab,
                     (displayInfo, screenGrab) => (displayInfo, screenGrab)))
        {
            screenGrab.WindowStartupLocation = WindowStartupLocation.Manual;
            screenGrab.WindowState = WindowState.Normal;
            var screenWithScaledBounds = screen.ScaledBounds();

            screenGrab.Width = screenWithScaledBounds.Width;
            screenGrab.Height = screenWithScaledBounds.Height;
            screenGrab.Left = screenWithScaledBounds.X;
            screenGrab.Top = screenWithScaledBounds.Y;

            screenGrab.Show();
            screenGrab.Activate();
        }
    }
}
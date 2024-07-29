using System.Windows;
using Dapplo.Windows.User32;
using ScreenGrab.Extensions;
using ScreenGrab.Utilities;

namespace ScreenGrab.Sample;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void Capture_Click(object sender, RoutedEventArgs e)
    {
        Clean();

        NewScreenGrab();

        // var view = new Grab();
        // if (view.ShowDialog() == true)
        // {
        //     var result = view.Image;
        //     Img.Source = ImageMethods.BitmapToImageSource(result);
        // }
    }

    private void NewScreenGrab()
    {
        DisplayInfo[] allScreens = DisplayInfo.AllDisplayInfos;
        WindowCollection allWindows = Application.Current.Windows;

        List<Grab> allFullscreenGrab = new();

        int numberOfScreens = allScreens.Count();

        foreach (Window window in allWindows)
            if (window is Grab grab)
                allFullscreenGrab.Add(grab);

        int numberOfFullscreenGrabWindowsToCreate = numberOfScreens - allFullscreenGrab.Count;

        for (int i = 0; i < numberOfFullscreenGrabWindowsToCreate; i++)
        {
            allFullscreenGrab.Add(new Grab());
        }

        int count = 0;

        double sideLength = 40;

        foreach (DisplayInfo screen in allScreens)
        {
            Grab fullScreenGrab = allFullscreenGrab[count];
            fullScreenGrab.WindowStartupLocation = WindowStartupLocation.Manual;
            fullScreenGrab.Width = sideLength;
            fullScreenGrab.Height = sideLength;
            fullScreenGrab.WindowState = WindowState.Normal;

            Point screenCenterPoint = screen.ScaledCenterPoint();

            fullScreenGrab.Left = screenCenterPoint.X - (sideLength / 2);
            fullScreenGrab.Top = screenCenterPoint.Y - (sideLength / 2);

            fullScreenGrab.Show();
            fullScreenGrab.Activate();

            count++;
        }
    }

    private void Clean_Click(object sender, RoutedEventArgs e)
    {
        Clean();
    }

    private void Clean()
    {
        Img.Source?.Freeze();
        Img.Source = null;
        GC.Collect();
    }
}
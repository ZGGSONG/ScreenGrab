using System.Windows;
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

        var grab = new Grab
        {
            OnImageCaptured = bitmap => Img.Source = bitmap.ToImageSource()
        };
        grab.Capture();
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
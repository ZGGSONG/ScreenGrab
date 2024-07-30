using System.Windows;
using System.Windows.Input;
using NHotkey;
using NHotkey.Wpf;
using ScreenGrab.Utilities;

namespace ScreenGrab.Sample;

public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void Capture()
    {
        Clean();

        var grab = new Grab
        {
            OnImageCaptured = bitmap => Img.Source = bitmap.ToImageSource()
        };
        grab.Capture();
    }

    private void Capture(object? sender, HotkeyEventArgs e)
    {
        Capture();
    }

    private void Capture_Click(object sender, RoutedEventArgs e)
    {
        Capture();
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

    private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        HotkeyManager.Current.AddOrReplace("Capture", Key.A, ModifierKeys.Windows | ModifierKeys.Shift, Capture);
    }

    private void MainWindow_OnUnloaded(object sender, RoutedEventArgs e)
    {
        HotkeyManager.Current.Remove("Capture");
    }
}
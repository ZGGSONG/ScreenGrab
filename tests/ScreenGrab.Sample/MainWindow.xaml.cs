using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using NHotkey;
using NHotkey.Wpf;
using ScreenGrab.Extensions;
using Wpf.Ui.Tray.Controls;

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

        ScreenGrabber.OnCaptured = bitmap =>
        {
            Img.Source = bitmap.ToImageSource();
            if (WindowState != WindowState.Normal)
                WindowState = WindowState.Normal;
            if (!IsVisible)
                Show();
            Activate();
        };
        ScreenGrabber.Capture(AuxiliaryCb.IsChecked ?? false);
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

    protected override void OnClosing(CancelEventArgs e)
    {
        e.Cancel = true;
        Hide();
    }

    private void NotifyIcon_OnLeftClick(NotifyIcon sender, RoutedEventArgs e)
    {
        Show();
        Activate();
    }

    private void MenuItem_OnClick(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }
}
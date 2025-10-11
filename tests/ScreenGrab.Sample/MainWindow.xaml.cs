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

    private async void Capture()
    {
        Clean();

        // 1. 回调等待结果
        //ScreenGrabber.OnCaptured = bitmap =>
        //{
        //    Img.Source = bitmap.ToImageSource();
        //    if (WindowState != WindowState.Normal)
        //        WindowState = WindowState.Normal;
        //    if (!IsVisible)
        //        Show();
        //    Activate();
        //};
        //ScreenGrabber.Capture(AuxiliaryCb.IsChecked ?? false);

        // 2. 同步获取结果（类似Dialog）
        //var bitmap = ScreenGrabber.CaptureDialog(AuxiliaryCb.IsChecked ?? false);

        //// 如果用户取消截图，bitmap 将为 null
        //if (bitmap is null)
        //    return;

        //Img.Source = bitmap.ToImageSource();
        //if (WindowState != WindowState.Normal)
        //    WindowState = WindowState.Normal;
        //if (!IsVisible)
        //    Show();
        //Activate();

        // 3. 异步获取结果
        var bitmap = await ScreenGrabber.CaptureAsync(AuxiliaryCb.IsChecked ?? false);

        // 如果用户取消截图，bitmap 将为 null
        if (bitmap is null)
            return;

        Img.Source = bitmap.ToImageSource();
        if (WindowState != WindowState.Normal)
            WindowState = WindowState.Normal;
        if (!IsVisible)
            Show();
        Activate();
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
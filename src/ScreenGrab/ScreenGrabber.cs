using ScreenGrab.Extensions;
using System.Drawing;
using System.Windows;
using System.Windows.Threading;
using WpfScreenHelper;

namespace ScreenGrab;

public abstract class ScreenGrabber
{
    public static bool IsCapturing { get; private set; }

    public static Action<Bitmap>? OnCaptured { get; set; }

    private static TaskCompletionSource<Bitmap?>? _captureTaskCompletionSource;

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

    /// <summary>
    /// 同步方式捕获屏幕截图，类似 Window.ShowDialog() 的阻塞式调用
    /// </summary>
    /// <param name="isAuxiliary">是否显示辅助线</param>
    /// <returns>返回捕获的 Bitmap，如果用户取消则返回 null</returns>
    public static Bitmap? CaptureDialog(bool isAuxiliary = false)
    {
        if (IsCapturing)
            return null;

        Bitmap? result = null;
        var frame = new DispatcherFrame();

        IsCapturing = true;

        var allScreens = Screen.AllScreens;
        var allScreenGrab = Application.Current.Windows.OfType<ScreenGrabView>().ToList();
        var numberOfScreenGrabWindowsToCreate = allScreens.Count() - allScreenGrab.Count;

        for (var i = 0; i < numberOfScreenGrabWindowsToCreate; i++)
        {
            var view = new ScreenGrabView(bitmap =>
            {
                // 截图成功时保存结果并退出消息循环
                result = bitmap;
                frame.Continue = false;
            }, isAuxiliary)
            {
                OnGrabClose = () =>
                {
                    IsCapturing = false;
                    // 关闭时退出消息循环
                    frame.Continue = false;
                }
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

        // 阻塞等待用户完成截图或取消
        Dispatcher.PushFrame(frame);

        return result;
    }

    /// <summary>
    /// 异步方式捕获屏幕截图，类似 Dialog 的使用方式
    /// </summary>
    /// <param name="isAuxiliary">是否显示辅助线</param>
    /// <returns>返回捕获的 Bitmap，如果用户取消则返回 null</returns>
    public static Task<Bitmap?> CaptureAsync(bool isAuxiliary = false)
    {
        if (IsCapturing)
            return Task.FromResult<Bitmap?>(null);

        _captureTaskCompletionSource = new TaskCompletionSource<Bitmap?>();

        IsCapturing = true;

        var allScreens = Screen.AllScreens;
        var allScreenGrab = Application.Current.Windows.OfType<ScreenGrabView>().ToList();
        var numberOfScreenGrabWindowsToCreate = allScreens.Count() - allScreenGrab.Count;

        for (var i = 0; i < numberOfScreenGrabWindowsToCreate; i++)
        {
            var view = new ScreenGrabView(bitmap =>
            {
                // 截图成功时完成任务
                _captureTaskCompletionSource?.TrySetResult(bitmap);
            }, isAuxiliary)
            {
                OnGrabClose = () =>
                {
                    IsCapturing = false;
                },
                OnCancel = () =>
                {
                    _captureTaskCompletionSource?.TrySetResult(null);
                }
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

        return _captureTaskCompletionSource.Task;
    }
}
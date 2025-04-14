using System.Drawing;
using System.IO;
using System.Windows;
using ScreenGrab.Extensions;

namespace ScreenGrab;

public abstract class ScreenGrabber
{
    public static bool IsCapturing { get; private set; }

    public static Action<Bitmap>? OnCaptured { get; set; }

    public static void Capture(bool isAuxiliary = false)
    {
        if (IsCapturing) return;

        IsCapturing = true;

        var allDisplayInfos = WpfScreenHelper.Screen.AllScreens;

        var allScreenGrab = Application.Current.Windows.OfType<ScreenGrabView>().ToList();
        var numberOfScreenGrabWindowsToCreate = allDisplayInfos.Count() - allScreenGrab.Count;

        for (var i = 0; i < numberOfScreenGrabWindowsToCreate; i++)
        {
            var view = new ScreenGrabView(OnCaptured, isAuxiliary)
            {
                OnGrabClose = () => IsCapturing = false
            };
            allScreenGrab.Add(view);
        }

        foreach (var (displayInfo, screenGrab) in allDisplayInfos.Zip(allScreenGrab,
                     (displayInfo, screenGrab) => (displayInfo, screenGrab)))
        {
            screenGrab.WindowStartupLocation = WindowStartupLocation.Manual;
            screenGrab.WindowState = WindowState.Normal;

            screenGrab.Width = displayInfo.ScaledBounds().Width;
            screenGrab.Height = displayInfo.ScaledBounds().Height;
            screenGrab.Left = displayInfo.ScaledBounds().X;
            screenGrab.Top = displayInfo.ScaledBounds().Y;

            // 打印display信息及screengrab top left 宽高
            Log($"显示器 #{allDisplayInfos.ToList().IndexOf(displayInfo) + 1}:");
            Log($"  设备名称: {displayInfo.DeviceName}");
            Log($"  主显示器: {displayInfo.Primary}");
            Log($"  缩放比例: {displayInfo.ScaleFactor}");
            Log($"  工作区: X={displayInfo.WorkingArea.X}, Y={displayInfo.WorkingArea.Y}, 宽={displayInfo.WorkingArea.Width}, 高={displayInfo.WorkingArea.Height}");
            Log($"★ 缩放后边界: X={displayInfo.ScaledBounds().X}, Y={displayInfo.ScaledBounds().Y}, 宽={displayInfo.ScaledBounds().Width}, 高={displayInfo.ScaledBounds().Height}");

            screenGrab.Show();
            screenGrab.Activate();
        }
    }

    private static void Log(string content)
    {
        string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "screen_grab_log.txt");
        File.AppendAllText(logPath, $"{content}\r\n");
    }
}
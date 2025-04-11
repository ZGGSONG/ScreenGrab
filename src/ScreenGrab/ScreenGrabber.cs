using System.Drawing;
using System.IO;
using System.Text;
using System.Windows;
using Dapplo.Windows.User32;
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

        // 添加日志功能，将显示器信息写入文件
        LogDisplayInfo(allDisplayInfos.ToList());

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

    private static void LogDisplayInfo(List<WpfScreenHelper.Screen> displayInfos)
    {
        try
        {
            string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "screen_grab_log.txt");
            StringBuilder logContent = new();

            logContent.AppendLine($"[{DateTime.Now}] 捕获到显示器数量: {displayInfos.Count()}");

            for (int i = 0; i < displayInfos.Count(); i++)
            {
                var display = displayInfos[i];
                logContent.AppendLine($"显示器 #{i + 1}:");
                logContent.AppendLine($"  设备名称: {display.DeviceName}");
                logContent.AppendLine($"  主显示器: {display.Primary}");
                // 添加更多显示器信息
                logContent.AppendLine($"  坐标和尺寸信息:");
                logContent.AppendLine($"    Bounds: X={display.Bounds.X}, Y={display.Bounds.Y}, 宽={display.Bounds.Width}, 高={display.Bounds.Height}");

                logContent.AppendLine($"    工作区: X={display.WorkingArea.X}, Y={display.WorkingArea.Y}, 宽={display.WorkingArea.Width}, 高={display.WorkingArea.Height}");

                // 获取缩放信息
                var scaledBounds = display.ScaledBounds();
                logContent.AppendLine($"    缩放后边界: X={scaledBounds.X}, Y={scaledBounds.Y}, 宽={scaledBounds.Width}, 高={scaledBounds.Height}");

                //var centerPoint = display.ScaledCenterPoint();
                //logContent.AppendLine($"    缩放后中心点: X={centerPoint.X}, Y={centerPoint.Y}");

                // 尝试记录 DPI 缩放因子
                logContent.AppendLine($"    DPI缩放因子: {display.ScaledBounds()}({display.ScaleFactor})");

                logContent.AppendLine();
            }

            // 追加模式写入文件
            File.AppendAllText(logPath, logContent.ToString());
        }
        catch (Exception ex)
        {
            // 如果日志写入失败，避免影响主程序功能
            try
            {
                File.WriteAllText(
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log_error.txt"),
                    $"[{DateTime.Now}] 日志写入失败: {ex.Message}");
            }
            catch
            {
                // 忽略最终的错误
            }
        }
    }
}
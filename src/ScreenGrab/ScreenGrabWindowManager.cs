namespace ScreenGrab;

public class ScreenGrabWindowManager
{
    private static readonly List<ScreenGrabView> _windows = [];

    public static void RegisterWindow(ScreenGrabView window)
    {
        _windows.Add(window);
    }

    public static void UnregisterWindow(ScreenGrabView window)
    {
        _windows.Remove(window);
    }

    public static List<ScreenGrabView> GetAllWindows()
    {
        return [.. _windows];
    }
}
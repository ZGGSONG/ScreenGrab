using System.Runtime.InteropServices;

internal static partial class NativeMethods
{
#if NET7_0_OR_GREATER

    [LibraryImport("shcore.dll")]
    public static partial void GetScaleFactorForMonitor(IntPtr hMon, out uint pScale);
#else
    [DllImport("shcore.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern void GetScaleFactorForMonitor(IntPtr hMon, out uint pScale);
#endif
}
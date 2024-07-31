using System.Windows;
using System.Windows.Threading;

namespace ScreenGrab.Sample;

public partial class App
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        Current.DispatcherUnhandledException += CurrentDispatcherUnhandledException;
    }

    private void CurrentDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        // unhandled exceptions thrown from UI thread
        Console.WriteLine($"Unhandled exception: {e.Exception}");
        e.Handled = true;
    }
}

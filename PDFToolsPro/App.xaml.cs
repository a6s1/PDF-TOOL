using System.Windows;
using System.Windows.Threading;
using PDFToolsPro.Helpers;

namespace PDFToolsPro;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Global exception handlers
        DispatcherUnhandledException += App_DispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

        // Add converters to application resources
        Resources.Add("EnumBooleanConverter", new EnumBooleanConverter());
        Resources.Add("BoolToLanguageConverter", new BoolToLanguageConverter());
        Resources.Add("BoolToVisibilityConverter", new BoolToVisibilityConverter());
        Resources.Add("InverseBoolConverter", new InverseBoolConverter());
        Resources.Add("EnumToVisibilityConverter", new EnumToVisibilityConverter());
    }

    private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show($"Error: {e.Exception.Message}\n\n{e.Exception.StackTrace}", "Application Error", 
            MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true;
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            MessageBox.Show($"Critical Error: {ex.Message}\n\n{ex.StackTrace}", "Critical Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        MessageBox.Show($"Task Error: {e.Exception.Message}", "Task Error",
            MessageBoxButton.OK, MessageBoxImage.Error);
        e.SetObserved();
    }
}

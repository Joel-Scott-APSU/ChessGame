using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Windows;

namespace ChessGame
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Handle exceptions on non-UI threads
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                Exception ex = args.ExceptionObject as Exception;
                Debug.WriteLine("Unhandled exception: " + ex?.ToString());
                // Optionally, log to file or show message box here
            };

            // Handle unobserved exceptions in tasks
            TaskScheduler.UnobservedTaskException += (sender, args) =>
            {
                Debug.WriteLine("Unobserved task exception: " + args.Exception.ToString());
                args.SetObserved(); // Prevent process termination
                                    // Optionally, log or show message box here
            };

            // Handle exceptions on UI thread
            this.DispatcherUnhandledException += (sender, args) =>
            {
                Debug.WriteLine("Dispatcher unhandled exception: " + args.Exception.ToString());
                args.Handled = true; // Prevent app from crashing, remove if you want to crash on error
                                     // Optionally, log or show message box here
            };
        }
    }


}

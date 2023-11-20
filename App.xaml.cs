using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MM2Buddy
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Subscribe to the unhandled exception event
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            // Other startup code...
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception exception = e.ExceptionObject as Exception;
            if (exception != null)
            {
                // Log the exception to the crash report file
                LogCrashReport(exception);

                // Optionally, display an error message to the user
                MessageBox.Show($"An unexpected error occurred: {exception.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                // Optionally, gracefully exit the application
                Environment.Exit(1);
            }
        }

        private void LogCrashReport(Exception exception)
        {
            try
            {
                var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var crashReportFilePath = System.IO.Path.Combine(appDataPath, "MM2Buddy", "crashreport.txt");

                // Write crash report details to the file
                System.IO.File.WriteAllText(crashReportFilePath, $"{DateTime.Now}: {exception}");
            }
            catch (Exception ex)
            {
                // Log the error to the console or take other appropriate action
                Console.WriteLine($"Error writing crash report: {ex.Message}");
            }
        }
    }
}

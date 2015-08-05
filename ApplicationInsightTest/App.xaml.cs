using System;
using System.Windows;

namespace ApplicationInsightTest
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        #region Variables

        private DataContext _context;
        private ApplicationInsightHelper _applicationInsightHelper;

        #endregion

        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                _applicationInsightHelper = new ApplicationInsightHelper();
                _context = new DataContext(_applicationInsightHelper);


                //We use this to get access to unhandled exceptions so we can report app crashes to the Telemetry client
                var currentDomain = AppDomain.CurrentDomain;
                currentDomain.UnhandledException += CurrentDomain_UnhandledException;
                currentDomain.ProcessExit += CurrentDomain_ProcessExit;

                var mainWindow = new MainWindow(_context, _applicationInsightHelper);
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        /// <summary>
        /// Flush TelemetryClient data when the application exits
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            _applicationInsightHelper.FlushData();
        }

        /// <summary>
        /// Flush TelemtryClient data on an uncaught exception (app crash)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _applicationInsightHelper.TrackFatalException(e.ExceptionObject as Exception);
            _applicationInsightHelper.FlushData();
        }
    }
}

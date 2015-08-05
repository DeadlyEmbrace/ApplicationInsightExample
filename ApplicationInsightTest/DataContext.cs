using System;
using System.ComponentModel;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using ApplicationInsightTest.HelperClasses;

namespace ApplicationInsightTest
{
    public class DataContext : INotifyPropertyChanged
    {
        #region Variables

        private readonly ApplicationInsightHelper _applicationInsightHelper;

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Properties

        public string UserName { get; private set; }
        public string OsName { get; private set; }
        public string SessionKey { get; private set; }
        public string Application { get; private set; }
        public string Version { get; private set; }
        public string Manufacturer { get; private set; }
        public string Model { get; private set; }

        public ICommand ExceptionCommand => new RelayCommand(param => CauseException());
        public ICommand UncaughtExceptionCommand => new RelayCommand(param => CauseUncaughtException());

        #endregion

        #region Constructor

        public DataContext(ApplicationInsightHelper applicationInsightHelper)
        {
            _applicationInsightHelper = applicationInsightHelper;
            GatherDetails();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gather details to display
        /// </summary>
        private void GatherDetails()
        {
            SessionKey = Guid.NewGuid().ToString();
            UserName = Environment.UserName;
            OsName = GetWindowsFriendlyName();

            Version = $"v.{ Assembly.GetEntryAssembly().GetName().Version}";
            Application = $"{ Assembly.GetEntryAssembly().GetName().Name} {Version}";
            Manufacturer = (from x in
                new ManagementObjectSearcher("SELECT Manufacturer FROM Win32_ComputerSystem").Get()
                    .OfType<ManagementObject>()
                             select x.GetPropertyValue("Manufacturer")).FirstOrDefault()?.ToString() ?? "Unknown";
            Model = (from x in
                new ManagementObjectSearcher("SELECT Model FROM Win32_ComputerSystem").Get()
                    .OfType<ManagementObject>()
                      select x.GetPropertyValue("Model")).FirstOrDefault()?.ToString() ?? "Unknown";
        }

        private string GetWindowsFriendlyName()
        {
            var name = (from x in new ManagementObjectSearcher("SELECT Caption FROM Win32_OperatingSystem").Get().OfType<ManagementObject>()
                        select x.GetPropertyValue("Caption")).FirstOrDefault();

            return name?.ToString() ?? Environment.OSVersion.ToString();
        }

        /// <summary>
        /// Throw and catch an exception
        /// </summary>
        private void CauseException()
        {
            try
            {
                MessageBox.Show("Causing non-fatal exception");
                throw new Exception("User caused exception");
            }
            catch (Exception ex)
            {
                _applicationInsightHelper.TrackNonFatalExceptions(ex);
            }
        }

        /// <summary>
        /// Throw an exception
        /// </summary>
        private void CauseUncaughtException()
        {
            throw  new Exception("Uncaught!!");
        }

        #endregion

    }
}
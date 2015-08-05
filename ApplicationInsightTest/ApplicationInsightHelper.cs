using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Windows;
using Microsoft.ApplicationInsights;

namespace ApplicationInsightTest
{
    public class ApplicationInsightHelper
    {
        #region Variables

        private readonly TelemetryClient _telemetryClient;
        private string _sessionKey;
        private string _userName;
        private string _osName;
        private string _version;
        private string _application;
        private string _manufacturer;
        private string _model;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        public ApplicationInsightHelper()
        {
            _telemetryClient = new TelemetryClient() {InstrumentationKey = "{your instrumentation key here}" };
            GatherDetails();
            SetupTelemetry();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// This method flushes the Telemetry Client data to the server
        /// </summary>
        public void FlushData()
        {
            DoDataFlush();
        }

        /// <summary>
        /// Track a page view
        /// </summary>
        /// <param name="pageName">The name of the page</param>
        public void TrackPageView(string pageName)
        {
            _telemetryClient.TrackPageView(pageName);
        }

        /// <summary>
        /// Track a non-fatal exception
        /// </summary>
        /// <param name="ex">The exception details</param>
        public void TrackNonFatalExceptions(Exception ex)
        {
            var metrics = new Dictionary<string, double> { { "Non-fatal Exception", 1 } };
            _telemetryClient.TrackException(ex, null, metrics);
        }

        /// <summary>
        /// Track a fatal exception (app crash)
        /// </summary>
        /// <param name="ex"></param>
        public void TrackFatalException(Exception ex)
        {
            var exceptionTelemetry = new Microsoft.ApplicationInsights.DataContracts.ExceptionTelemetry(new Exception());
            exceptionTelemetry.HandledAt = Microsoft.ApplicationInsights.DataContracts.ExceptionHandledAt.Unhandled;
            _telemetryClient.TrackException(exceptionTelemetry);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Set up the Telemetry client context data
        /// This records a variety of details to enhance our data collection
        /// </summary>
        private void SetupTelemetry()
        {
            _telemetryClient.Context.Properties.Add("Application Version", _version);
            _telemetryClient.Context.User.Id = _userName;
            _telemetryClient.Context.User.UserAgent = _application;
            _telemetryClient.Context.Component.Version = _version;

            _telemetryClient.Context.Session.Id = _sessionKey;

            _telemetryClient.Context.Device.OemName = _manufacturer;
            _telemetryClient.Context.Device.Model = _model;
            _telemetryClient.Context.Device.OperatingSystem = _osName;
        }

        /// <summary>
        /// Flush the TelemetryClient data to the server
        /// Gives a message so user knows the data has been sent
        /// </summary>
        private void DoDataFlush()
        {
            _telemetryClient.Flush();
            MessageBox.Show("Application Insight Data sent");
        }

        /// <summary>
        /// Gathers the details used to add details to our telemetry data
        /// </summary>
        private void GatherDetails()
        {
            _sessionKey = Guid.NewGuid().ToString();
            _userName = Environment.UserName;
            _osName = GetWindowsFriendlyName();

            _version = $"v.{ Assembly.GetEntryAssembly().GetName().Version}";
            _application = $"{ Assembly.GetEntryAssembly().GetName().Name} {_version}";
            _manufacturer = (from x in
                new ManagementObjectSearcher("SELECT Manufacturer FROM Win32_ComputerSystem").Get()
                    .OfType<ManagementObject>()
                            select x.GetPropertyValue("Manufacturer")).FirstOrDefault()?.ToString() ?? "Unknown";
            _model = (from x in
                new ManagementObjectSearcher("SELECT Model FROM Win32_ComputerSystem").Get()
                    .OfType<ManagementObject>()
                     select x.GetPropertyValue("Model")).FirstOrDefault()?.ToString() ?? "Unknown";
        }

        /// <summary>
        /// Retrieve the Windows friendly name instead of just a version
        /// </summary>
        /// <returns></returns>
        private string GetWindowsFriendlyName()
        {
            var name = (from x in new ManagementObjectSearcher("SELECT Caption FROM Win32_OperatingSystem").Get().OfType<ManagementObject>()
                        select x.GetPropertyValue("Caption")).FirstOrDefault();

            return name?.ToString() ?? Environment.OSVersion.ToString();
        }

        #endregion
    }
}
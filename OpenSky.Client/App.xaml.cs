// --------------------------------------------------------------------------------------------------------------------
// <copyright file="App.xaml.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Threading;
    using System.Windows;
    using System.Windows.Threading;

    using JetBrains.Annotations;

    using OpenSky.Client.Properties;
    using OpenSky.Client.Tools;

    using XDMessaging;

    /// -------------------------------------------------------------------------------------------------
    /// <content>
    /// Main entry point class for WPF application.
    /// </content>
    ///// -------------------------------------------------------------------------------------------------
    public partial class App
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Named mutex that ensures single instance.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [NotNull]
        private static readonly Mutex Mutex = new(false, "OpenSky.Client.SingleInstance." + Environment.UserName);

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The after shutdown clean up code delegate.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [CanBeNull]
        private Delegate afterShutdownCleanUpCodeDelegate;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Requests a graceful shutdown of the application.
        /// </summary>
        /// <remarks>
        /// sushi.at, 12/03/2021.
        /// </remarks>
        /// <param name="afterShutdownCleanUpCode">
        /// The after shutdown clean up code (only executed if the user hasn't aborted the shutdown).
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public void RequestShutdown([CanBeNull] Delegate afterShutdownCleanUpCode)
        {
            this.afterShutdownCleanUpCodeDelegate = afterShutdownCleanUpCode;
            new Thread(this.PerformShutdown) { Name = "ShutdownManager" }.Start();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Raises the <see cref="E:System.Windows.Application.Startup" /> event.
        /// </summary>
        /// <remarks>
        /// sushi.at, 11/03/2021.
        /// </remarks>
        /// <param name="e">
        /// A <see cref="T:System.Windows.StartupEventArgs" /> that contains the event data.
        /// </param>
        /// <seealso cref="M:System.Windows.Application.OnStartup(StartupEventArgs)"/>
        /// -------------------------------------------------------------------------------------------------
        protected override void OnStartup([NotNull] StartupEventArgs e)
        {
            // Check if we need to upgrade the user settings file
            if (Settings.Default.SettingsUpdateRequired)
            {
                try
                {
                    Debug.WriteLine("Updating user settings file to new version...");
                    Settings.Default.Upgrade();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Unable to upgrade existing user settings file to this version: " + ex);
                }

                Settings.Default.SettingsUpdateRequired = false;
                Settings.Default.Save();
            }

            // Check for command line arguments
            var updatedToken = false;
            foreach (var arg in e.Args)
            {
                if (arg.StartsWith("opensky-client://") || arg.StartsWith("opensky-client-debug://"))
                {
                    var appTokenUri = arg.Replace("opensky-client://", string.Empty).Replace("opensky-client-debug://", string.Empty).TrimEnd('/');
                    var parameters = appTokenUri.Split('&');

                    string token = null;
                    DateTime? tokenExpiration = null;
                    string refresh = null;
                    DateTime? refreshExpiration = null;
                    string user = null;

                    foreach (var parameter in parameters)
                    {
                        if (parameter.StartsWith("token="))
                        {
                            token = parameter.Replace("token=", string.Empty);
                        }

                        if (parameter.StartsWith("tokenExpiration="))
                        {
                            if (long.TryParse(parameter.Replace("tokenExpiration=", string.Empty), out var ticks))
                            {
                                tokenExpiration = DateTime.FromFileTimeUtc(ticks);
                            }
                        }

                        if (parameter.StartsWith("refresh="))
                        {
                            refresh = parameter.Replace("refresh=", string.Empty);
                        }

                        if (parameter.StartsWith("refreshExpiration="))
                        {
                            if (long.TryParse(parameter.Replace("refreshExpiration=", string.Empty), out var ticks))
                            {
                                refreshExpiration = DateTime.FromFileTimeUtc(ticks);
                            }
                        }

                        if (parameter.StartsWith("user="))
                        {
                            user = parameter.Replace("user=", string.Empty);
                        }
                    }

                    if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(refresh) && !string.IsNullOrEmpty(user) && tokenExpiration.HasValue && refreshExpiration.HasValue)
                    {
                        Settings.Default.OpenSkyApiToken = token;
                        Settings.Default.OpenSkyTokenExpiration = tokenExpiration.Value;
                        Settings.Default.OpenSkyRefreshToken = refresh;
                        Settings.Default.OpenSkyRefreshTokenExpiration = refreshExpiration.Value;
                        Settings.Default.OpenSkyUsername = user;
                        Settings.Default.Save();
                        updatedToken = true;
                    }
                    else
                    {
                        ModernWpf.MessageBox.Show("Invalid command line parameters, aborting.", "OpenSky", MessageBoxButton.OK, MessageBoxImage.Warning);
                        Environment.Exit(1);
                    }
                }
            }

            // Ensure single instance
            try
            {
                if (!Mutex.WaitOne(TimeSpan.FromSeconds(3), false))
                {
                    if (!updatedToken)
                    {
                        ModernWpf.MessageBox.Show("The OpenSky client is already running.", "OpenSky", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                    {
                        // We updated the token in the settings, before we exit let the running instance now about this
                        var client = new XDMessagingClient();
                        var broadcaster = client.Broadcasters.GetBroadcasterForMode(XDTransportMode.Compatibility);
                        broadcaster.SendToChannel("OPENSKY-CLIENT", "TokensUpdated");
                    }

                    Environment.Exit(1);
                }
            }
            catch (AbandonedMutexException)
            {
                // Nothing to do here, lets continue starting the application
            }
            catch (Exception)
            {
                // Something went wrong, lets abort
                Views.Startup.StartupFailed = true;
                Environment.Exit(2);
            }

            // Add debug log handler
#if DEBUG
            var openSkyFolder = Environment.ExpandEnvironmentVariables("%localappdata%\\OpenSky");
            if (!Directory.Exists(openSkyFolder))
            {
                Directory.CreateDirectory(openSkyFolder);
            }

            var logFilePath = Environment.ExpandEnvironmentVariables("%localappdata%\\OpenSky\\client_debug.log");
            var traceListener = new DateTimeTextWriterTraceListener(File.Open(logFilePath, FileMode.Append, FileAccess.Write));
            Debug.AutoFlush = true;
            Debug.Listeners.Add(traceListener);
#endif
            Debug.WriteLine("========================================================================================");
            Debug.WriteLine($"OPENSKY Client {Assembly.GetExecutingAssembly().GetName().Version.ToString(4)} STARTING UP");
            Debug.WriteLine("========================================================================================");

            // Unexpected error handler
            this.DispatcherUnhandledException += AppDispatcherUnhandledException;

            // Continue startup
            base.OnStartup(e);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Handles unhandled exceptions.
        /// </summary>
        /// <remarks>
        /// sushi.at, 11/03/2021.
        /// </remarks>
        /// <param name="sender">
        /// The sender of the event.
        /// </param>
        /// <param name="e">
        /// The event arguments, containing the exception.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private static void AppDispatcherUnhandledException(
            [CanBeNull] object sender,
            [NotNull] DispatcherUnhandledExceptionEventArgs e)
        {
            var crashReport = "==============================================================================\r\n";
            crashReport += "  OPENSKY CLIENT CRASH REPORT\r\n";
            crashReport += "  " + DateTime.Now + "\r\n";
            crashReport += "==============================================================================\r\n";
            crashReport += e.Exception + "\r\n";
            crashReport += "==============================================================================\r\n";
            crashReport += "\r\n\r\n";

            Debug.WriteLine(crashReport);
            var filePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\OpenSky.Client.Crash.txt";

            try
            {
                File.AppendAllText(filePath, crashReport);
                ModernWpf.MessageBox.Show(
                    e.Exception.Message + "\r\n\r\nPlease check OpenSky.Client.Crash.txt for details!",
                    "Unexpected error!",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (Exception)
            {
                ModernWpf.MessageBox.Show(e.Exception.ToString(), "Unexpected error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            Environment.Exit(3);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Performs the shutdown actions in a background thread.
        /// </summary>
        /// <remarks>
        /// sushi.at, 12/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void PerformShutdown()
        {
            // todo Check if there is anything that needs saving or is in progress

            // Run clean-up delegate code
            if (this.afterShutdownCleanUpCodeDelegate != null)
            {
                this.Dispatcher?.Invoke(this.afterShutdownCleanUpCodeDelegate);
            }

            // Do the actual WPF application shutdown (this can't be cancelled anymore)
            UpdateGUIDelegate shutdownApp = this.Shutdown;
            this.Dispatcher?.BeginInvoke(shutdownApp);
        }
    }
}
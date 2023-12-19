// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StartupViewModel.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Views.Models
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using System.Threading;
    using System.Windows;

    using JetBrains.Annotations;

    using OpenSky.Client.Controls;
    using OpenSky.Client.Controls.Models;
    using OpenSky.Client.MVVM;
    using OpenSky.Client.Tools;

    using OpenSkyApi;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// The startup view model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 11/03/2021.
    /// </remarks>
    /// <seealso cref="T:OpenSky.Client.MVVM.ViewModel"/>
    /// -------------------------------------------------------------------------------------------------
    public class StartupViewModel : ViewModel
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="StartupViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 15/06/2021.
        /// </remarks>
        /// <exception cref="Exception">
        /// Thrown when an exception error condition occurs.
        /// </exception>
        /// -------------------------------------------------------------------------------------------------

        // ReSharper disable NotNullMemberIsNotInitialized
        public StartupViewModel()
        {
            if (!Startup.StartupFailed)
            {
                if (Instance != null)
                {
                    throw new Exception("Only one instance of the startup view model may be created!");
                }

                Instance = this;
            }

            // Initialize commands
            this.StartupChecksCommand = new AsynchronousCommand(this.StartupChecks);
            this.StartupChecksCommand.DoExecute(null);

            // Check for update
            UpdateGUIDelegate autoUpdate = () => new AutoUpdate().Show();
            Application.Current.Dispatcher.BeginInvoke(autoUpdate);

            // Start background worker threads
            new Thread(this.CheckNotifications) { Name = "OpenSky.StartupViewModel.CheckNotifications" }.Start();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Check for notifications.
        /// </summary>
        /// <remarks>
        /// sushi.at, 18/12/2023.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void CheckNotifications()
        {
            // Wait for window to load up properly
            Thread.Sleep(5000);

            while (!SleepScheduler.IsShutdownInProgress)
            {
                if (UserSessionService.Instance.IsUserLoggedIn)
                {
                    try
                    {
                        var result = OpenSkyService.Instance.GetNotificationsAsync(NotificationTarget.Client).Result;
                        if (!result.IsError)
                        {
                            if (result.Data.Count > 0)
                            {
                                UpdateGUIDelegate showNotifications = () =>
                                {
                                    foreach (var notificationData in result.Data)
                                    {
                                        if (notificationData.Style is NotificationStyle.ToastInfo or NotificationStyle.ToastWarning or NotificationStyle.ToastError)
                                        {
                                            var icon = notificationData.Style switch
                                            {
                                                NotificationStyle.ToastWarning => ExtendedMessageBoxImage.Warning,
                                                NotificationStyle.ToastError => ExtendedMessageBoxImage.Error,
                                                _ => ExtendedMessageBoxImage.Information
                                            };

                                            foreach (var mainInstance in Main.Instances)
                                            {
                                                var notification = new OpenSkyNotification($"Notification from \"{notificationData.Sender}\"", notificationData.Message, MessageBoxButton.OK, icon, notificationData.DisplayTimeout ?? 0);
                                                if (notificationData.Style == NotificationStyle.ToastWarning)
                                                {
                                                    notification.SetWarningColorStyle();
                                                }

                                                if (notificationData.Style == NotificationStyle.ToastError)
                                                {
                                                    notification.SetErrorColorStyle();
                                                }

                                                mainInstance.ShowNotification(notification);
                                            }
                                        }

                                        if (notificationData.Style is NotificationStyle.MessageBoxInfo or NotificationStyle.MessageBoxWarning or NotificationStyle.MessageBoxError)
                                        {
                                            var icon = notificationData.Style switch
                                            {
                                                NotificationStyle.MessageBoxWarning => ExtendedMessageBoxImage.Warning,
                                                NotificationStyle.MessageBoxError => ExtendedMessageBoxImage.Error,
                                                _ => ExtendedMessageBoxImage.Information
                                            };

                                            var messageBox = new OpenSkyMessageBox($"Notification from \"{notificationData.Sender}\"", notificationData.Message, MessageBoxButton.OK, icon, notificationData.DisplayTimeout ?? 0);
                                            if (notificationData.Style == NotificationStyle.MessageBoxWarning)
                                            {
                                                messageBox.SetWarningColorStyle();
                                            }

                                            if (notificationData.Style == NotificationStyle.MessageBoxError)
                                            {
                                                messageBox.SetErrorColorStyle();
                                            }

                                            Main.Instances[0].ShowMessageBox(messageBox);
                                        }
                                    }
                                };
                                Application.Current.Dispatcher.BeginInvoke(showNotifications);

                                foreach (var notification in result.Data)
                                {
                                    _ = OpenSkyService.Instance.ConfirmNotificationPickupAsync(notification.Id, NotificationTarget.Client).Result;
                                }
                            }
                        }
                        else
                        {
                            Debug.WriteLine($"Error checking for notifications: {result.Message}\r\n{result.ErrorDetails}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Error checking for notifications: " + ex);
                    }

                    SleepScheduler.SleepFor(TimeSpan.FromMinutes(1));
                }
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Startup checks.
        /// </summary>
        /// <remarks>
        /// sushi.at, 15/06/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void StartupChecks()
        {
            var checksStarted = DateTime.Now;

            // Is no user logged in?
            if (!UserSessionService.Instance.IsUserLoggedIn)
            {
                this.StartupChecksCommand.ReportProgress(() => LoginNotification.Open());
            }
            else
            {
                _ = UserSessionService.Instance.UpdateUserRoles().Result;
                _ = UserSessionService.Instance.RefreshUserAccountOverview().Result;
                _ = UserSessionService.Instance.RefreshLinkedAccounts().Result;
                try
                {
                    AirportPackageClientHandler.DownloadPackage();
                }
                catch
                {
                    // Ignore here
                }
            }

            UserSessionService.Instance.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(UserSessionService.Instance.IsUserLoggedIn) && UserSessionService.Instance.IsUserLoggedIn)
                {
                    _ = UserSessionService.Instance.UpdateUserRoles().Result;
                    _ = UserSessionService.Instance.RefreshUserAccountOverview().Result;
                    _ = UserSessionService.Instance.RefreshLinkedAccounts().Result;

                    try
                    {
                        AirportPackageClientHandler.DownloadPackage();
                    }
                    catch
                    {
                        // Ignore here
                    }
                }
            };

            // Show the splash screen for at least for 2 seconds, then open the main window and trigger the close window event
            Thread.Sleep(Math.Max(2000 - (int)(DateTime.Now - checksStarted).TotalMilliseconds, 0));
            this.StartupChecksCommand.ReportProgress(
                () =>
                {
                    new Main().Show();
                    this.CloseWindow?.Invoke(this, EventArgs.Empty);
                });
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the startup checks command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand StartupChecksCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the single instance of the startup view model.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [CanBeNull]
        public static StartupViewModel Instance { get; private set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the version string of the application assembly.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string VersionString => $"v{Assembly.GetExecutingAssembly().GetName().Version}";

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Occurs when the view model wants to close the window.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public event EventHandler CloseWindow;
    }
}
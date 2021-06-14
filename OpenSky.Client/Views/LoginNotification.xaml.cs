// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LoginNotification.xaml.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Views
{
    using System;
    using System.Windows;

    using OpenSky.Client.Tools;
    using OpenSky.Client.Views.Models;

    /// -------------------------------------------------------------------------------------------------
    /// <content>
    /// Login notification window.
    /// </content>
    /// -------------------------------------------------------------------------------------------------
    public partial class LoginNotification
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="LoginNotification"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 01/06/2021.
        /// </remarks>
        /// <param name="timeout">
        /// The timeout for the notification in milliseconds.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private LoginNotification(int timeout = 60 * 1000)
        {
            this.InitializeComponent();

            if (this.DataContext is LoginNotificationViewModel viewModel)
            {
                viewModel.Timeout = DateTime.Now.AddMilliseconds(timeout);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The single instance of this view.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static LoginNotification Instance { get; private set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Open new login notification or bring the existing instance into view (+extend timeout, +play
        /// ding dong again)
        /// </summary>
        /// <remarks>
        /// sushi.at, 04/06/2021.
        /// </remarks>
        /// <param name="timeout">
        /// The timeout for the notification in milliseconds.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public static void Open(int timeout = 60 * 1000)
        {
            if (Instance == null)
            {
                Instance = new LoginNotification(timeout);
                Instance.Closed += (_, _) => Instance = null;
                Instance.Show();
            }
            else
            {
                Instance.ExtendTimeout(timeout);
                Instance.BringIntoView();
                Instance.Activate();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Extend timeout.
        /// </summary>
        /// <remarks>
        /// sushi.at, 04/06/2021.
        /// </remarks>
        /// <param name="timeout">
        /// The timeout for the notification in milliseconds.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void ExtendTimeout(int timeout)
        {
            if (this.DataContext is LoginNotificationViewModel viewModel)
            {
                viewModel.Timeout = DateTime.Now.AddMilliseconds(timeout);
                viewModel.PlaySoundAgain();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Login notification loaded.
        /// </summary>
        /// <remarks>
        /// sushi.at, 01/06/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Routed event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void LoginNotificationOnLoaded(object sender, RoutedEventArgs e)
        {
            this.PositionWindowToNotificationArea();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Occurs when the view model wants to close the window.
        /// </summary>
        /// <remarks>
        /// sushi.at, 01/06/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void LoginNotificationViewModelOnCloseWindow(object sender, EventArgs e)
        {
            UpdateGUIDelegate closethis = () =>
            {
                try
                {
                    this.Close();
                }
                catch
                {
                    // Ignore
                }
            };
            this.Dispatcher.BeginInvoke(closethis);
        }
    }
}
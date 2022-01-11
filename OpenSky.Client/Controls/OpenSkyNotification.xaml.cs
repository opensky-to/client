// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OpenSkyNotification.xaml.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Controls
{
    using System.Threading;
    using System.Windows;

    using OpenSky.Client.Tools;

    /// -------------------------------------------------------------------------------------------------
    /// <content>
    /// OpenSky notification control.
    /// </content>
    /// -------------------------------------------------------------------------------------------------
    public partial class OpenSkyNotification
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="OpenSkyNotification"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 10/01/2022.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public OpenSkyNotification(string title, string message, MessageBoxButton? buttons = null, MessageBoxImage image = MessageBoxImage.None, int autoCloseTimeout = 0)
        {
            this.InitializeComponent();

            this.Title.Text = title;
            this.Message.Text = message;

            if (autoCloseTimeout > 0)
            {
                this.TimeoutText.Text = $"{autoCloseTimeout}";
                this.TimeoutProgress.Maximum = autoCloseTimeout;
                this.TimeoutText.Visibility = Visibility.Visible;
                this.TimeoutProgress.Visibility = Visibility.Visible;
                new Thread(
                    () =>
                    {
                        while (autoCloseTimeout > 0)
                        {
                            Thread.Sleep(1000);
                            if (SleepScheduler.IsShutdownInProgress)
                            {
                                return;
                            }

                            autoCloseTimeout--;
                            UpdateGUIDelegate updateTimeout = () =>
                            {
                                // ReSharper disable AccessToModifiedClosure
                                this.TimeoutText.Text = $"{autoCloseTimeout}";
                                this.TimeoutProgress.Progress = autoCloseTimeout;
                                // ReSharper restore AccessToModifiedClosure
                            };
                            this.Dispatcher.BeginInvoke(updateTimeout);
                        }

                        UpdateGUIDelegate hideNotification = () => this.Visibility = Visibility.Collapsed;
                        this.Dispatcher.BeginInvoke(hideNotification);
                    })
                { Name = "OpenSkyNotification.AutoCloseTimeout" }.Start();
            }
        }
    }
}
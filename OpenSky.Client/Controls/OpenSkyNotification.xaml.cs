// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OpenSkyNotification.xaml.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Controls
{
    using System;
    using System.Threading;
    using System.Windows;
    using System.Windows.Media;

    using OpenSky.Client.Controls.Models;
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
        /// The automatic close result.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly MessageBoxResult autoCloseResult;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The automatic close timeout.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private int autoCloseTimeout;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="OpenSkyNotification"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 10/01/2022.
        /// </remarks>
        /// <param name="title">
        /// The title.
        /// </param>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="buttons">
        /// (Optional) The buttons.
        /// </param>
        /// <param name="image">
        /// (Optional) The image.
        /// </param>
        /// <param name="autoCloseTimeout">
        /// (Optional)
        /// The automatic close timeout.
        /// </param>
        /// <param name="autoCloseResult">
        /// (Optional) The automatic close result.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public OpenSkyNotification(string title, string message, MessageBoxButton? buttons = null, ExtendedMessageBoxImage image = ExtendedMessageBoxImage.None, int autoCloseTimeout = 0, MessageBoxResult autoCloseResult = MessageBoxResult.None)
        {
            this.InitializeComponent();

            this.Title.Text = title;
            this.Message.Text = message;

            if (buttons.HasValue)
            {
                this.ButtonPanel.Visibility = Visibility.Visible;
                switch (buttons.Value)
                {
                    case MessageBoxButton.OK:
                        this.OkButton.Visibility = Visibility.Visible;
                        break;
                    case MessageBoxButton.OKCancel:
                        this.OkButton.Visibility = Visibility.Visible;
                        this.CancelButton.Visibility = Visibility.Visible;
                        break;
                    case MessageBoxButton.YesNo:
                        this.YesButton.Visibility = Visibility.Visible;
                        this.NoButton.Visibility = Visibility.Visible;
                        break;
                    case MessageBoxButton.YesNoCancel:
                        this.YesButton.Visibility = Visibility.Visible;
                        this.NoButton.Visibility = Visibility.Visible;
                        this.CancelButton.Visibility = Visibility.Visible;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(buttons), buttons.Value, null);
                }
            }

            switch (image)
            {
                case ExtendedMessageBoxImage.Asterisk:
                    this.Image.Source = this.FindResource("Asterisks") as DrawingImage;
                    break;
                case ExtendedMessageBoxImage.Check:
                    this.Image.Source = this.FindResource("Check") as DrawingImage;
                    break;
                case ExtendedMessageBoxImage.Error:
                    this.Image.Source = this.FindResource("Error") as DrawingImage;
                    break;
                case ExtendedMessageBoxImage.Exclamation:
                    this.Image.Source = this.FindResource("Exclamation") as DrawingImage;
                    break;
                case ExtendedMessageBoxImage.Hand:
                    this.Image.Source = this.FindResource("Hand") as DrawingImage;
                    break;
                case ExtendedMessageBoxImage.Information:
                    this.Image.Source = this.FindResource("Info") as DrawingImage;
                    break;
                case ExtendedMessageBoxImage.None:
                    break;
                case ExtendedMessageBoxImage.Question:
                    this.Image.Source = this.FindResource("Question") as DrawingImage;
                    break;
                case ExtendedMessageBoxImage.Stop:
                    this.Image.Source = this.FindResource("Stop") as DrawingImage;
                    break;
                case ExtendedMessageBoxImage.Warning:
                    this.Image.Source = this.FindResource("Warning") as DrawingImage;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(image), image, null);
            }

            this.autoCloseTimeout = autoCloseTimeout;
            this.autoCloseResult = autoCloseResult;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the notification background brush.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Brush NotificationBackground
        {
            get => this.BackgroundBorder.Background;
            set => this.BackgroundBorder.Background = value;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Occurs when the notification is closed.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public event EventHandler Closed;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the notification result.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public MessageBoxResult Result { get; private set; } = MessageBoxResult.None;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Cancel button click.
        /// </summary>
        /// <remarks>
        /// sushi.at, 12/01/2022.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Routed event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void CancelButtonOnClick(object sender, RoutedEventArgs e)
        {
            this.Result = MessageBoxResult.Cancel;
            this.Visibility = Visibility.Collapsed;
            this.Closed?.Invoke(this, EventArgs.Empty);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// No button click.
        /// </summary>
        /// <remarks>
        /// sushi.at, 12/01/2022.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Routed event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void NoButtonOnClick(object sender, RoutedEventArgs e)
        {
            this.Result = MessageBoxResult.No;
            this.Visibility = Visibility.Collapsed;
            this.Closed?.Invoke(this, EventArgs.Empty);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Ok button click.
        /// </summary>
        /// <remarks>
        /// sushi.at, 12/01/2022.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Routed event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void OkButtonOnClick(object sender, RoutedEventArgs e)
        {
            this.Result = MessageBoxResult.OK;
            this.Visibility = Visibility.Collapsed;
            this.Closed?.Invoke(this, EventArgs.Empty);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// OpenSky notification loaded.
        /// </summary>
        /// <remarks>
        /// sushi.at, 11/01/2022.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Routed event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void OpenSkyNotificationOnLoaded(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Visible;

            if (this.autoCloseTimeout > 0)
            {
                this.TimeoutText.Text = $"{this.autoCloseTimeout}";
                this.TimeoutProgress.Maximum = this.autoCloseTimeout;
                this.TimeoutText.Visibility = Visibility.Visible;
                this.TimeoutProgress.Visibility = Visibility.Visible;
                new Thread(
                        () =>
                        {
                            while (this.autoCloseTimeout > 0 && this.Visibility == Visibility.Visible)
                            {
                                Thread.Sleep(1000);
                                if (SleepScheduler.IsShutdownInProgress)
                                {
                                    return;
                                }

                                this.autoCloseTimeout--;
                                UpdateGUIDelegate updateTimeout = () =>
                                {
                                    this.TimeoutText.Text = $"{this.autoCloseTimeout}";
                                    this.TimeoutProgress.Progress = this.autoCloseTimeout;
                                };
                                this.Dispatcher.BeginInvoke(updateTimeout);
                            }

                            UpdateGUIDelegate hideNotification = () =>
                            {
                                if (this.autoCloseTimeout == 0)
                                {
                                    this.Result = this.autoCloseResult;
                                }

                                this.Visibility = Visibility.Collapsed;
                                this.Closed?.Invoke(this, EventArgs.Empty);
                            };
                            this.Dispatcher.BeginInvoke(hideNotification);
                        })
                    { Name = "OpenSkyNotification.AutoCloseTimeout" }.Start();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Yes button click.
        /// </summary>
        /// <remarks>
        /// sushi.at, 12/01/2022.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Routed event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void YesButtonOnClick(object sender, RoutedEventArgs e)
        {
            this.Result = MessageBoxResult.Yes;
            this.Visibility = Visibility.Collapsed;
            this.Closed?.Invoke(this, EventArgs.Empty);
        }
    }
}
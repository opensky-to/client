// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OpenSkyNotification.xaml.cs" company="OpenSky">
// OpenSky project 2021-2022
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
        private readonly ExtendedMessageBoxResult autoCloseResult;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The error details (that can be passed on to a OpenSkyMessageBox).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly ErrorDetails errorDetails;

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
        /// sushi.at, 12/01/2022.
        /// </remarks>
        /// <param name="title">
        /// The title.
        /// </param>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="errorDetails">
        /// The error details (that can be passed on to a OpenSkyMessageBox).
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
        public OpenSkyNotification(
            ErrorDetails errorDetails,
            string title,
            string message,
            ExtendedMessageBoxImage image = ExtendedMessageBoxImage.Error,
            int autoCloseTimeout = 0,
            ExtendedMessageBoxResult autoCloseResult = ExtendedMessageBoxResult.None)
        {
            this.InitializeComponent();

            this.Title.Text = title;
            this.Message.Text = message;
            this.errorDetails = errorDetails;

            this.NotificationBackground = new SolidColorBrush(Color.FromRgb(180, 0, 0));

            this.ButtonPanel.Visibility = Visibility.Visible;
            this.OkButton.Visibility = Visibility.Visible;
            this.OkButton.Style = this.FindResource("OpenSkyRedButtonStyle") as Style;
            if (errorDetails != null)
            {
                this.DetailsButton.Visibility = Visibility.Visible;
                this.DetailsButton.Style = this.FindResource("OpenSkyRedButtonStyle") as Style;
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
        public OpenSkyNotification(
            string title,
            string message,
            MessageBoxButton? buttons = null,
            ExtendedMessageBoxImage image = ExtendedMessageBoxImage.None,
            int autoCloseTimeout = 0,
            ExtendedMessageBoxResult autoCloseResult = ExtendedMessageBoxResult.None)
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
        /// Occurs when the notification is closed.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public event EventHandler Closed;

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
        /// Gets the notification result.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ExtendedMessageBoxResult Result { get; private set; } = ExtendedMessageBoxResult.None;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Sets error color style (red background with red buttons).
        /// </summary>
        /// <remarks>
        /// sushi.at, 13/01/2022.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public void SetErrorColorStyle()
        {
            this.NotificationBackground = new SolidColorBrush(Color.FromRgb(180, 0, 0));
            this.OkButton.Style = this.FindResource("OpenSkyRedButtonStyle") as Style;
            this.CancelButton.Style = this.FindResource("OpenSkyRedButtonStyle") as Style;
            this.YesButton.Style = this.FindResource("OpenSkyRedButtonStyle") as Style;
            this.NoButton.Style = this.FindResource("OpenSkyRedButtonStyle") as Style;
            this.DetailsButton.Style = this.FindResource("OpenSkyRedButtonStyle") as Style;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Sets warning color style (light orange background with orange buttons, text set to black).
        /// </summary>
        /// <remarks>
        /// sushi.at, 14/01/2022.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public void SetWarningColorStyle()
        {
            this.NotificationBackground = this.FindResource("OpenSkyWarningOrangeLightBrush") as SolidColorBrush;
            this.Foreground = new SolidColorBrush(Colors.Black);
            this.OkButton.Style = this.FindResource("OpenSkyOrangeButtonStyle") as Style;
            this.CancelButton.Style = this.FindResource("OpenSkyOrangeButtonStyle") as Style;
            this.YesButton.Style = this.FindResource("OpenSkyOrangeButtonStyle") as Style;
            this.NoButton.Style = this.FindResource("OpenSkyOrangeButtonStyle") as Style;
            this.DetailsButton.Style = this.FindResource("OpenSkyOrangeButtonStyle") as Style;
        }

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
            this.Result = ExtendedMessageBoxResult.Cancel;
            this.Visibility = Visibility.Collapsed;
            this.Closed?.Invoke(this, EventArgs.Empty);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Details button click.
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
        private void DetailsButtonOnClick(object sender, RoutedEventArgs e)
        {
            this.Result = ExtendedMessageBoxResult.Details;
            this.Visibility = Visibility.Collapsed;
            this.Closed?.Invoke(this, new ErrorDetailsEventArgs { ErrorDetails = this.errorDetails });
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
            this.Result = ExtendedMessageBoxResult.No;
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
            this.Result = ExtendedMessageBoxResult.OK;
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
            this.Result = ExtendedMessageBoxResult.Yes;
            this.Visibility = Visibility.Collapsed;
            this.Closed?.Invoke(this, EventArgs.Empty);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Additional information for error details events.
        /// </summary>
        /// <remarks>
        /// sushi.at, 12/01/2022.
        /// </remarks>
        /// <seealso cref="T:System.EventArgs"/>
        /// -------------------------------------------------------------------------------------------------
        public class ErrorDetailsEventArgs : EventArgs
        {
            /// -------------------------------------------------------------------------------------------------
            /// <summary>
            /// Gets or sets the error details.
            /// </summary>
            /// -------------------------------------------------------------------------------------------------
            public ErrorDetails ErrorDetails { get; set; }
        }
    }
}
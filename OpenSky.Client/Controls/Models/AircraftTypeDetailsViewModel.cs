// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AircraftTypeDetailsViewModel.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Controls.Models
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Windows;
    using System.Windows.Media.Imaging;

    using OpenSky.Client.MVVM;
    using OpenSky.Client.Tools;
    using OpenSky.Client.Views;

    using OpenSkyApi;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Aircraft type details user control view model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 16/02/2022.
    /// </remarks>
    /// <seealso cref="OpenSky.Client.MVVM.ViewModel"/>
    /// -------------------------------------------------------------------------------------------------
    public class AircraftTypeDetailsViewModel : ViewModel
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The aircraft image.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private BitmapImage aircraftImage;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The loading visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Visibility loadingVisibility = Visibility.Collapsed;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The aircraft type.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private AircraftType type;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="AircraftTypeDetailsViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 19/02/2022.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public AircraftTypeDetailsViewModel()
        {
            this.AircraftImage = new BitmapImage(new Uri("pack://application:,,,/OpenSky.Client;component/Resources/aircraftTypePlaceholder.png"));

            this.GetAircraftImageCommand = new AsynchronousCommand(this.GetAircraftImage);
            this.GetAircraftImageCommand.DoExecute(null);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the aircraft image.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public BitmapImage AircraftImage
        {
            get => this.aircraftImage;

            set
            {
                if (Equals(this.aircraftImage, value))
                {
                    return;
                }

                this.aircraftImage = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the get aircraft image command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand GetAircraftImageCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the loading visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Visibility LoadingVisibility
        {
            get => this.loadingVisibility;

            set
            {
                if (Equals(this.loadingVisibility, value))
                {
                    return;
                }

                this.loadingVisibility = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the aircraft type.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AircraftType Type
        {
            get => this.type;

            set
            {
                if (Equals(this.type, value))
                {
                    return;
                }

                this.type = value;
                this.NotifyPropertyChanged();
                if (value != null)
                {
                    this.GetAircraftImageCommand.DoExecute(null);
                }
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Get the aircraft type image from the OpenSky API.
        /// </summary>
        /// <remarks>
        /// sushi.at, 19/02/2022.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void GetAircraftImage()
        {
            if (this.Type == null)
            {
                return;
            }

            try
            {
                this.LoadingVisibility = Visibility.Visible;
                var result = OpenSkyService.Instance.GetAircraftTypeImageAsync(this.Type.Id).Result;
                if (!result.IsError)
                {
                    if (result.Data is { Length: > 0 })
                    {
                        var image = new BitmapImage();
                        using (var mem = new MemoryStream(result.Data))
                        {
                            image.BeginInit();
                            image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                            image.CacheOption = BitmapCacheOption.OnLoad;
                            image.UriSource = null;
                            image.StreamSource = mem;
                            image.EndInit();
                        }

                        image.Freeze();
                        this.AircraftImage = image;
                    }
                    else
                    {
                        this.AircraftImage = new BitmapImage(new Uri("pack://application:,,,/OpenSky.Client;component/Resources/aircraftTypePlaceholder.png"));
                    }
                }
                else
                {
                    this.GetAircraftImageCommand.ReportProgress(
                        () =>
                        {
                            Debug.WriteLine("Error retrieving aircraft type image: " + result.Message);
                            if (!string.IsNullOrEmpty(result.ErrorDetails))
                            {
                                Debug.WriteLine(result.ErrorDetails);
                            }

                            var notification = new OpenSkyNotification("Error retrieving aircraft type image", result.Message, MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                            notification.SetErrorColorStyle();
                            Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.ViewReference, this.GetAircraftImageCommand, "Error retrieving aircraft type image");
            }
            finally
            {
                this.LoadingVisibility = Visibility.Collapsed;
            }
        }
    }
}
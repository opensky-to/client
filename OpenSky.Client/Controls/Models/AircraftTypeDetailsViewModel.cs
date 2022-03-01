// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AircraftTypeDetailsViewModel.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Controls.Models
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;
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
        /// (Immutable) the image cache.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private static readonly Dictionary<Guid, BitmapImage> ImageCache = new();

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The aircraft image.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private BitmapImage aircraftImage;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The aircraft image placeholder text.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string aircraftImagePlaceholderText = "No aircraft type selected";

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
        /// Gets or sets the aircraft image placeholder text.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string AircraftImagePlaceholderText
        {
            get => this.aircraftImagePlaceholderText;

            set
            {
                if (Equals(this.aircraftImagePlaceholderText, value))
                {
                    return;
                }

                this.aircraftImagePlaceholderText = value;
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
                if (!this.GetAircraftImageCommand.IsExecuting)
                {
                    this.GetAircraftImageCommand.DoExecute(null);
                }
                else
                {
                    new Thread(
                            () =>
                            {
                                while (this.GetAircraftImageCommand.IsExecuting)
                                {
                                    Thread.Sleep(100);
                                }

                                UpdateGUIDelegate getImage = () => this.GetAircraftImageCommand.DoExecute(null);
                                Application.Current.Dispatcher.BeginInvoke(getImage);
                            })
                        { Name = "AircraftTypeDetailsViewModel.WaitForGetAircraftImageCommand" }.Start();
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
            var typeCopy = this.Type;
            Debug.WriteLine($"Getting aircraft type image for type: {typeCopy}");
            if (typeCopy == null)
            {
                this.GetAircraftImageCommand.ReportProgress(
                    () =>
                    {
                        this.AircraftImage = new BitmapImage(new Uri("pack://application:,,,/OpenSky.Client;component/Resources/aircraftTypePlaceholder.png"));
                        this.AircraftImagePlaceholderText = "No aircraft type selected";
                    });
                return;
            }

            if (ImageCache.ContainsKey(typeCopy.Id))
            {
                if (ImageCache[typeCopy.Id] != null)
                {
                    this.AircraftImage = ImageCache[typeCopy.Id];
                    this.AircraftImagePlaceholderText = string.Empty;
                }
                else
                {
                    this.GetAircraftImageCommand.ReportProgress(
                        () =>
                        {
                            this.AircraftImage = new BitmapImage(new Uri("pack://application:,,,/OpenSky.Client;component/Resources/aircraftTypePlaceholder.png"));
                            this.AircraftImagePlaceholderText = "No image available";
                        });
                }

                return;
            }

            try
            {
                this.AircraftImagePlaceholderText = string.Empty;
                this.LoadingVisibility = Visibility.Visible;
                var result = OpenSkyService.Instance.GetAircraftTypeImageAsync(typeCopy.Id).Result;
                if (!result.IsError)
                {
                    if (result.Data is { Length: > 0 })
                    {
                        this.GetAircraftImageCommand.ReportProgress(
                            () =>
                            {
                                try
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
                                    ImageCache.Add(typeCopy.Id, image);
                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine($"Error loading aircraft type image: {ex}");
                                    this.AircraftImage = new BitmapImage(new Uri("pack://application:,,,/OpenSky.Client;component/Resources/aircraftTypePlaceholder.png"));
                                    this.AircraftImagePlaceholderText = "Error loading aircraft type image";
                                }
                            });
                    }
                    else
                    {
                        this.GetAircraftImageCommand.ReportProgress(
                            () =>
                            {
                                this.AircraftImage = new BitmapImage(new Uri("pack://application:,,,/OpenSky.Client;component/Resources/aircraftTypePlaceholder.png"));
                                this.AircraftImagePlaceholderText = "No image available";
                                ImageCache.Add(typeCopy.Id, null);
                            });
                    }
                }
                else
                {
                    this.GetAircraftImageCommand.ReportProgress(
                        () =>
                        {
                            this.AircraftImage = new BitmapImage(new Uri("pack://application:,,,/OpenSky.Client;component/Resources/aircraftTypePlaceholder.png"));
                            this.AircraftImagePlaceholderText = "Error loading aircraft type image";

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
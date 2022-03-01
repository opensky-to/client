// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AirportDetailsViewModel.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Controls.Models
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Windows;

    using OpenSky.Client.MVVM;
    using OpenSky.Client.Tools;
    using OpenSky.Client.Views;

    using OpenSkyApi;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Airport details user control view model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 01/03/2022.
    /// </remarks>
    /// <seealso cref="OpenSky.Client.MVVM.ViewModel"/>
    /// -------------------------------------------------------------------------------------------------
    public class AirportDetailsViewModel : ViewModel
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The airport.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Airport airport;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the airport.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Airport Airport
        {
            get => this.airport;
        
            set
            {
                if(Equals(this.airport, value))
                {
                   return;
                }
        
                this.airport = value;
                this.NotifyPropertyChanged();

                this.AirportMarkers.Clear();
                if (value != null)
                {
                    foreach (var runway in value.Runways)
                    {
                        var runwayMarker = new TrackingEventMarker(runway);
                        this.AirportMarkers.Add(runwayMarker);
                    }
                }
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the load airport command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand LoadAirportCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The loading visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Visibility loadingVisibility = Visibility.Collapsed;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the airport markers.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<TrackingEventMarker> AirportMarkers { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="AirportDetailsViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 01/03/2022.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public AirportDetailsViewModel()
        {
            this.AirportMarkers = new ObservableCollection<TrackingEventMarker>();

            this.LoadAirportCommand = new AsynchronousCommand(this.LoadAirport);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Loads the airport (when only ICAO code was available).
        /// </summary>
        /// <remarks>
        /// sushi.at, 01/03/2022.
        /// </remarks>
        /// <param name="parameter">
        /// The parameter, set to string ICAO code.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void LoadAirport(object parameter)
        {
            if (parameter is string icao && !string.IsNullOrEmpty(icao))
            {
                Debug.WriteLine($"Loading aircraft for detail view: {icao}");
                this.LoadingVisibility = Visibility.Visible;
                try
                {
                    var result = OpenSkyService.Instance.GetAirportAsync(icao).Result;
                    if (!result.IsError)
                    {
                        this.LoadAirportCommand.ReportProgress(() => { this.Airport = result.Data; });
                    }
                    else
                    {
                        this.LoadAirportCommand.ReportProgress(
                            () =>
                            {
                                Debug.WriteLine("Error retrieving airport details: " + result.Message);
                                if (!string.IsNullOrEmpty(result.ErrorDetails))
                                {
                                    Debug.WriteLine(result.ErrorDetails);
                                }

                                var notification = new OpenSkyNotification("Error retrieving airport details", result.Message, MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                                notification.SetErrorColorStyle();
                                Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                            });
                    }
                }
                catch (Exception ex)
                {
                    ex.HandleApiCallException(this.ViewReference, this.LoadAirportCommand, "Error retrieving airport details");
                }
                finally
                {
                    this.LoadingVisibility = Visibility.Collapsed;
                }
            }
        }

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
    }
}
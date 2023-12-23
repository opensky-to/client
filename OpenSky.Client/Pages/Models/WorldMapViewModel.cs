// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WorldMapViewModel.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Pages.Models
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Windows;

    using Microsoft.Maps.MapControl.WPF;

    using OpenSky.Client.Controls;
    using OpenSky.Client.Controls.Models;
    using OpenSky.Client.MVVM;
    using OpenSky.Client.Tools;
    using OpenSky.Client.Views;

    using OpenSkyApi;

    using TomsToolbox.Essentials;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// World map view model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 18/11/2021.
    /// </remarks>
    /// <seealso cref="T:OpenSky.Client.MVVM.ViewModel"/>
    /// -------------------------------------------------------------------------------------------------
    public class WorldMapViewModel : ViewModel
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The deselect aircraft visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Visibility deselectAircraftVisibility = Visibility.Collapsed;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The selected aircraft.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private AircraftPosition selectedAircraft;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="WorldMapViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 18/11/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public WorldMapViewModel()
        {
            // Initialize data structures
            this.AircraftPositions = new ObservableCollection<AircraftPosition>();

            // Create commands
            this.RefreshCommand = new AsynchronousCommand(this.Refresh);
            this.DeselectAircraftCommand = new Command(this.DeselectAircraft);
            this.RefreshAircraftTrailCommand = new AsynchronousCommand(this.RefreshAircraftTrail);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the aircraft positions.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<AircraftPosition> AircraftPositions { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the aircraft trail locations.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public LocationCollection AircraftTrailLocations { get; } = new();

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the deselect aircraft command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command DeselectAircraftCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the deselect aircraft visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Visibility DeselectAircraftVisibility
        {
            get => this.deselectAircraftVisibility;

            set
            {
                if (Equals(this.deselectAircraftVisibility, value))
                {
                    return;
                }

                this.deselectAircraftVisibility = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the refresh aircraft trail command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand RefreshAircraftTrailCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the refresh command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand RefreshCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the select aircraft.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AircraftPosition SelectAircraft
        {
            get => this.selectedAircraft;

            set
            {
                if (Equals(this.selectedAircraft, value))
                {
                    return;
                }

                this.selectedAircraft = value;
                this.NotifyPropertyChanged();
                this.DeselectAircraftVisibility = value != null ? Visibility.Visible : Visibility.Collapsed;

                foreach (var aircraftPosition in this.AircraftPositions)
                {
                    if (value != null && aircraftPosition.Registry == value.Registry)
                    {
                        aircraftPosition.IsSelected = true;
                    }
                    else
                    {
                        aircraftPosition.IsSelected = false;
                    }
                }

                this.RefreshAircraftTrailCommand.DoExecute(null);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Deselect aircraft.
        /// </summary>
        /// <remarks>
        /// sushi.at, 23/12/2023.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void DeselectAircraft()
        {
            this.SelectAircraft = null;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Refreshes the world map.
        /// </summary>
        /// <remarks>
        /// sushi.at, 18/11/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void Refresh()
        {
            try
            {
                var result = OpenSkyService.Instance.GetWorldMapFlightsAsync().Result;
                if (!result.IsError)
                {
                    this.RefreshCommand.ReportProgress(
                        () =>
                        {
                            try
                            {
                                foreach (var flight in result.Data)
                                {
                                    var tooltip =
                                        $"Flight {flight.FullFlightNumber} operated by {flight.Operator}\r\n-----------------------------------------------\r\nPilot: {flight.Pilot}\r\n{flight.AircraftType} [{flight.AircraftRegistry.RemoveSimPrefix()}]\r\n{flight.Origin} ▶ {flight.Destination}\r\nPhase: {flight.FlightPhase}\r\n";
                                    var flightInfo = $"Pilot: {flight.Pilot}\r\n{flight.AircraftType} [{flight.AircraftRegistry.RemoveSimPrefix()}]\r\n{flight.Origin} ▶ {flight.Destination}\r\nPhase: {flight.FlightPhase}\r\n";
                                    if (flight.OnGround)
                                    {
                                        tooltip += $"On the ground, {flight.GroundSpeed:F0} kts, heading: {flight.Heading:F0}";
                                        flightInfo += $"On the ground, {flight.GroundSpeed:F0} kts, heading: {flight.Heading:F0}";
                                    }
                                    else
                                    {
                                        tooltip += $"Airborne {flight.Altitude:F0} feet, {flight.GroundSpeed:F0} kts GS, heading: {flight.Heading:F0}";
                                        flightInfo += $"Airborne {flight.Altitude:F0} feet, {flight.GroundSpeed:F0} kts GS, heading: {flight.Heading:F0}";
                                    }


                                    var existingPosition = this.AircraftPositions.SingleOrDefault(p => p.Registry == flight.AircraftRegistry);
                                    if (existingPosition != null)
                                    {
                                        existingPosition.Heading = flight.Heading;
                                        existingPosition.Location = new Location(flight.Latitude, flight.Longitude, flight.Altitude);
                                        existingPosition.ToolTip = tooltip;
                                    }
                                    else
                                    {
                                        var newPosition = new AircraftPosition
                                        {
                                            Heading = flight.Heading,
                                            Location = new Location(flight.Latitude, flight.Longitude, flight.Altitude),
                                            Registry = flight.AircraftRegistry,
                                            ToolTip = tooltip,
                                            FlightID = flight.Id,
                                            FlightNumber = flight.FullFlightNumber,
                                            FlightInfo = flightInfo
                                        };
                                        newPosition.MouseLeftButtonDown += (_, _) => { this.SelectAircraft = newPosition; };
                                        this.AircraftPositions.Add(newPosition);
                                    }
                                }

                                var toRemove = new List<AircraftPosition>();
                                foreach (var position in this.AircraftPositions)
                                {
                                    var flight = result.Data.SingleOrDefault(f => f.AircraftRegistry == position.Registry);
                                    if (flight == null)
                                    {
                                        toRemove.Add(position);
                                    }
                                }

                                this.AircraftPositions.RemoveRange(toRemove);

                                // After the aircraft are updated, update the trail - if an aircraft is currently selected
                                this.RefreshCommand.ReportProgress(
                                    () =>
                                    {
                                        this.RefreshAircraftTrailCommand.DoExecute(null);
                                    });
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine("Error updating aircraft positions on world map: " + ex);
                            }
                        });
                }
                else
                {
                    this.RefreshCommand.ReportProgress(
                        () =>
                        {
                            Debug.WriteLine("Error refreshing world map: " + result.Message);
                            if (!string.IsNullOrEmpty(result.ErrorDetails))
                            {
                                Debug.WriteLine(result.ErrorDetails);
                            }

                            var notification = new OpenSkyNotification("Error refreshing world map", result.Message, MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                            notification.SetErrorColorStyle();
                            Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.ViewReference, this.RefreshCommand, "Error refreshing world map");
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Refresh aircraft trail.
        /// </summary>
        /// <remarks>
        /// sushi.at, 23/12/2023.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void RefreshAircraftTrail()
        {
            try
            {
                // Clear old trail
                this.RefreshAircraftTrailCommand.ReportProgress(
                    () =>
                    {
                        this.AircraftTrailLocations.Clear();
                    });

                if (this.SelectAircraft?.FlightID.HasValue == true)
                {
                    var result = OpenSkyService.Instance.GetWorldMapFlightTrailAsync(this.SelectAircraft.FlightID.Value).Result;
                    if (!result.IsError)
                    {
                        this.RefreshAircraftTrailCommand.ReportProgress(
                            () =>
                            {
                                this.AircraftTrailLocations.AddRange(result.Data.PositionReports.Select(pr => new Location(pr.Latitude, pr.Longitude, pr.Altitude)));
                            });
                    }
                    else
                    {
                        this.RefreshAircraftTrailCommand.ReportProgress(
                            () =>
                            {
                                Debug.WriteLine("Error refreshing aircraft trail: " + result.Message);
                                if (!string.IsNullOrEmpty(result.ErrorDetails))
                                {
                                    Debug.WriteLine(result.ErrorDetails);
                                }

                                var notification = new OpenSkyNotification("Error refreshing aircraft trail", result.Message, MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                                notification.SetErrorColorStyle();
                                Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                            });
                    }
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.ViewReference, this.RefreshCommand, "Error refreshing aircraft trail");
            }
        }
    }
}
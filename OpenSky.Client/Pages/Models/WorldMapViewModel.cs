// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WorldMapViewModel.cs" company="OpenSky">
// OpenSky project 2021
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

    using OpenSky.Client.Controls.Models;
    using OpenSky.Client.MVVM;
    using OpenSky.Client.Tools;

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
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the refresh command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand RefreshCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the aircraft positions.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<AircraftPosition> AircraftPositions { get; }

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
                    this.RefreshCommand.ReportProgress(() =>
                        {
                            try
                            {
                                foreach (var flight in result.Data)
                                {
                                    var tooltip = $"Flight {flight.FullFlightNumber} operated by {flight.Operator}\r\n-----------------------------------------------\r\nPilot: {flight.Pilot}\r\n{flight.AircraftType} [{flight.AircraftRegistry}]\r\n{flight.Origin} ▶ {flight.Destination}\r\nPhase: {flight.FlightPhase}\r\n";
                                    if (flight.OnGround)
                                    {
                                        tooltip += $"On the ground, {flight.GroundSpeed:F0} kts, heading: {flight.Heading:F0}";
                                    }
                                    else
                                    {
                                        tooltip += $"Airborne {flight.Altitude:F0} feet, {flight.GroundSpeed:F0} kts GS, heading: {flight.Heading:F0}";
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
                                            ToolTip = tooltip
                                        };
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

                            ModernWpf.MessageBox.Show(result.Message, "Error refreshing world map", MessageBoxButton.OK, MessageBoxImage.Error);
                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.RefreshCommand, "Error refreshing world map");
            }
        }
    }
}
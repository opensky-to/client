﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MapViewViewModel.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Controls.Models
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Device.Location;
    using System.Linq;
    using System.Linq.Dynamic.Core;
    using System.Threading;
    using System.Windows;
    using System.Windows.Media;

    using Microsoft.Maps.MapControl.WPF;

    using OpenSky.Client.MVVM;
    using OpenSky.Client.Tools;
    using OpenSky.S2Geometry.Extensions;

    using OpenSkyApi;

    using TomsToolbox.Essentials;

    using Airport = AirportsJSON.Airport;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// MapView view model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 04/11/2021.
    /// </remarks>
    /// <seealso cref="T:OpenSky.Client.MVVM.ViewModel"/>
    /// -------------------------------------------------------------------------------------------------
    public class MapViewViewModel : ViewModel
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The detailed airports.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly Dictionary<string, List<TrackingEventMarker>> detailedAirports = new();

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The dynamic airports.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly Dictionary<string, TrackingEventMarker> dynamicAirports = new();

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// True to enable, false to disable the worldwide airports.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private bool airportsEnabled;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The last user map interaction date/time.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private DateTime lastUserMapInteraction = DateTime.MinValue;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The currently selected simulator, or NULL for all simulators.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Simulator? simulator;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// True to show, false to hide the airports (if enabled).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private bool showAirports = true;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="MapViewViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 08/12/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public MapViewViewModel()
        {
            this.Airports = new ObservableCollection<TrackingEventMarker>();
            this.Simulators = new ObservableCollection<Simulator>();

            foreach (Simulator sim in Enum.GetValues(typeof(Simulator)))
            {
                this.Simulators.Add(sim);
            }

            if (Properties.Settings.Default.DefaultSimulator != -1)
            {
                this.Simulator = (Simulator)Properties.Settings.Default.DefaultSimulator;
            }

            this.EnableAirportsCommand = new AsynchronousCommand(this.EnableAirports);
            this.ClearSimulatorCommand = new Command(this.ClearSimulator);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the airports.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<TrackingEventMarker> Airports { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the clear simulator command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command ClearSimulatorCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the enable airports command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand EnableAirportsCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the date/time of the last user map interaction.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public DateTime LastUserMapInteraction
        {
            get => this.lastUserMapInteraction;

            set
            {
                if (Equals(this.lastUserMapInteraction, value))
                {
                    return;
                }

                this.lastUserMapInteraction = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether the airports are shown (if enabled).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool ShowAirports
        {
            get => this.showAirports;

            set
            {
                if (Equals(this.showAirports, value))
                {
                    return;
                }

                this.showAirports = value;
                this.NotifyPropertyChanged();

                foreach (var airport in this.Airports)
                {
                    airport.Opacity = value ? 1 : 0;
                }
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the currently selected simulator, or NULL for all simulators.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Simulator? Simulator
        {
            get => this.simulator;

            set
            {
                if (Equals(this.simulator, value))
                {
                    return;
                }

                this.simulator = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the simulators.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<Simulator> Simulators { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Update the airports collection depending on the current zoom level and map location.
        /// </summary>
        /// <remarks>
        /// sushi.at, 08/12/2021.
        /// </remarks>
        /// <param name="zoomLevel">
        /// The zoom level.
        /// </param>
        /// <param name="boundingRectangle">
        /// The bounding rectangle of the current map view.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public void UpdateAirports(double zoomLevel, LocationRect boundingRectangle)
        {
            if (this.airportsEnabled)
            {
                new Thread(
                        () =>
                        {
                            var airportPackage = AirportPackageClientHandler.GetPackage();
                            if (airportPackage != null)
                            {
                                if (zoomLevel > 12)
                                {
                                    lock (this.detailedAirports)
                                    {
                                        // Add detailed airport "bounded" markers with runways
                                        var rectCoverage = OpenSkyS2.RectangleCoverage(boundingRectangle.Northwest.Latitude, boundingRectangle.Northwest.Longitude, boundingRectangle.Southeast.Latitude, boundingRectangle.Southeast.Longitude);
                                        var cells = rectCoverage.Cells.Select(c => c.Id).ToList();
                                        var simClause = this.Simulator switch
                                        {
                                            OpenSkyApi.Simulator.MSFS => " and MSFS ",
                                            OpenSkyApi.Simulator.XPlane11 => " and XP11 ",
                                            _ => string.Empty
                                        };
                                        var airports = airportPackage.Airports.AsQueryable().Where($"Size > -1 and !IsClosed and !IsMilitary{simClause} and @0.Contains(S2Cell{rectCoverage.Level})", cells).ToList();

                                        // First remove the airports we don't need anymore
                                        var toRemove = new List<string>();
                                        foreach (var icao in this.detailedAirports.Keys)
                                        {
                                            if (airports.All(a => a.ICAO != icao))
                                            {
                                                toRemove.Add(icao);
                                            }
                                        }

                                        // Then figure out which ones to add
                                        var toAdd = new Dictionary<string, Airport>();
                                        foreach (var airport in airports)
                                        {
                                            if (!this.detailedAirports.ContainsKey(airport.ICAO))
                                            {
                                                toAdd.Add(airport.ICAO, airport);
                                            }
                                        }

                                        // Then perform the updates
                                        UpdateGUIDelegate updateAirports = () =>
                                        {
                                            foreach (var icao in toRemove)
                                            {
                                                this.Airports.RemoveRange(this.detailedAirports[icao]);
                                                this.detailedAirports.Remove(icao);
                                            }

                                            foreach (var add in toAdd)
                                            {
                                                var markerColor = add.Value.Size switch
                                                {
                                                    6 => OpenSkyColors.OpenSkyTealLight,
                                                    5 => OpenSkyColors.OpenSkyTeal,
                                                    4 => Colors.Gold,
                                                    3 => Colors.DarkMagenta,
                                                    2 => Colors.DarkOrange,
                                                    _ => Colors.DarkRed
                                                };

                                                var fontColor = add.Value.Size switch
                                                {
                                                    2 or 4 or 6 => Colors.Black,
                                                    _ => Colors.White
                                                };

                                                var detailedAirportMarkers = new List<TrackingEventMarker> { new(add.Value, markerColor, fontColor) { Opacity = this.ShowAirports ? 1 : 0 } };
                                                foreach (var runway in add.Value.Runways)
                                                {
                                                    detailedAirportMarkers.Add(new TrackingEventMarker(runway) { Opacity = this.ShowAirports ? 1 : 0 });
                                                }

                                                this.detailedAirports.Add(add.Key, detailedAirportMarkers);
                                                this.Airports.AddRange(this.detailedAirports[add.Key]);
                                            }
                                        };
                                        Application.Current.Dispatcher.Invoke(updateAirports);
                                    }
                                }
                                else if (zoomLevel > 5)
                                {
                                    lock (this.dynamicAirports)
                                    {
                                        var sizeQuery = zoomLevel switch
                                        {
                                            <= 8 => "Size = 4",
                                            <= 9 => "Size >= 2 and Size <= 4",
                                            _ => "Size >= 0 and Size <= 4"
                                        };

                                        var rectCoverage = OpenSkyS2.RectangleCoverage(boundingRectangle.Northwest.Latitude, boundingRectangle.Northwest.Longitude, boundingRectangle.Southeast.Latitude, boundingRectangle.Southeast.Longitude);
                                        var cells = rectCoverage.Cells.Select(c => c.Id).ToList();
                                        var simClause = this.Simulator switch
                                        {
                                            OpenSkyApi.Simulator.MSFS => " and MSFS ",
                                            OpenSkyApi.Simulator.XPlane11 => " and XP11 ",
                                            _ => string.Empty
                                        };
                                        var airports = airportPackage.Airports.AsQueryable().Where($"{sizeQuery} and !IsClosed and !IsMilitary{simClause} and @0.Contains(S2Cell{rectCoverage.Level})", cells).ToList();

                                        // First remove the airports we don't need anymore
                                        var toRemove = new List<string>();
                                        foreach (var icao in this.dynamicAirports.Keys)
                                        {
                                            if (airports.All(a => a.ICAO != icao))
                                            {
                                                toRemove.Add(icao);
                                            }
                                        }

                                        // Then figure out which ones to add
                                        var toAdd = new Dictionary<string, Airport>();
                                        foreach (var airport in airports)
                                        {
                                            if (!this.dynamicAirports.ContainsKey(airport.ICAO))
                                            {
                                                toAdd.Add(airport.ICAO, airport);
                                            }
                                        }

                                        // Then perform the updates
                                        UpdateGUIDelegate updateAirports = () =>
                                        {
                                            foreach (var icao in toRemove)
                                            {
                                                this.Airports.Remove(this.dynamicAirports[icao]);
                                                this.dynamicAirports.Remove(icao);
                                            }

                                            foreach (var add in toAdd)
                                            {
                                                var markerColor = add.Value.Size switch
                                                {
                                                    4 => Colors.Gold,
                                                    3 => Colors.DarkMagenta,
                                                    2 => Colors.DarkOrange,
                                                    _ => Colors.DarkRed
                                                };

                                                var fontColor = add.Value.Size switch
                                                {
                                                    2 or 4 => Colors.Black,
                                                    _ => Colors.White
                                                };

                                                this.dynamicAirports.Add(add.Key, new TrackingEventMarker(new GeoCoordinate(add.Value.Latitude, add.Value.Longitude), add.Key, markerColor, fontColor, true, add.Value.Size, add.Value.SupportsSuper) { Opacity = this.ShowAirports ? 1 : 0 });
                                                this.Airports.Add(this.dynamicAirports[add.Key]);
                                            }
                                        };
                                        Application.Current.Dispatcher.Invoke(updateAirports);
                                    }

                                    lock (this.detailedAirports)
                                    {
                                        // Remove all detailed airports
                                        UpdateGUIDelegate removeAirports = () =>
                                        {
                                            foreach (var markers in this.detailedAirports.Values)
                                            {
                                                this.Airports.RemoveRange(markers);
                                            }

                                            this.detailedAirports.Clear();
                                        };
                                        Application.Current.Dispatcher.Invoke(removeAirports);
                                    }
                                }
                                else
                                {
                                    lock (this.detailedAirports)
                                    {
                                        // Remove all detailed airports
                                        UpdateGUIDelegate removeAirports = () =>
                                        {
                                            foreach (var markers in this.detailedAirports.Values)
                                            {
                                                this.Airports.RemoveRange(markers);
                                            }

                                            this.detailedAirports.Clear();
                                        };
                                        Application.Current.Dispatcher.Invoke(removeAirports);
                                    }

                                    lock (this.dynamicAirports)
                                    {
                                        // Remove all non 5/6 airports
                                        UpdateGUIDelegate removeAirports = () =>
                                        {
                                            this.Airports.RemoveRange(this.dynamicAirports.Values);
                                            this.dynamicAirports.Clear();
                                        };
                                        Application.Current.Dispatcher.Invoke(removeAirports);
                                    }
                                }
                            }
                        })
                { Name = "MapViewViewModel.UpdateAirports" }.Start();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Clears the simulator.
        /// </summary>
        /// <remarks>
        /// sushi.at, 25/07/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void ClearSimulator()
        {
            this.Simulator = null;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Enables the worldwide airports.
        /// </summary>
        /// <remarks>
        /// sushi.at, 09/12/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void EnableAirports()
        {
            this.airportsEnabled = true;

            // Add all size 5 and 6 airports
            var airportPackage = AirportPackageClientHandler.GetPackage();
            if (airportPackage != null)
            {
                this.EnableAirportsCommand.ReportProgress(
                    () =>
                    {
                        var airportMarkers = new List<TrackingEventMarker>();
                        foreach (var airport in airportPackage.Airports.Where(a => a.Size >= 5 && !a.IsClosed && !a.IsMilitary))
                        {
                            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                            if (airport.Size == 6)
                            {
                                airportMarkers.Add(new TrackingEventMarker(new GeoCoordinate(airport.Latitude, airport.Longitude), airport.ICAO, OpenSkyColors.OpenSkyTealLight, Colors.Black, true, airport.Size, airport.SupportsSuper));
                            }
                            else
                            {
                                airportMarkers.Add(new TrackingEventMarker(new GeoCoordinate(airport.Latitude, airport.Longitude), airport.ICAO, OpenSkyColors.OpenSkyTeal, Colors.White, true, airport.Size, airport.SupportsSuper));
                            }
                        }

                        this.Airports.AddRange(airportMarkers);
                    });
            }
        }
    }
}
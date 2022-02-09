// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FlightPlanViewModel.Airports.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Pages.Models
{
    using System;
    using System.Collections.Generic;
    using System.Device.Location;
    using System.Diagnostics;
    using System.Linq;
    using System.Windows;
    using System.Windows.Media;
    using System.Xml.XPath;

    using Microsoft.Maps.MapControl.WPF;

    using OpenSky.Client.Controls;
    using OpenSky.Client.Controls.Models;
    using OpenSky.Client.MVVM;
    using OpenSky.Client.OpenAPIs.ModelExtensions;
    using OpenSky.Client.Tools;
    using OpenSky.Client.Views;

    using OpenSkyApi;

    using TomsToolbox.Essentials;

    /// -------------------------------------------------------------------------------------------------
    /// <content>
    /// Flight plan view model - Airports.
    /// </content>
    /// -------------------------------------------------------------------------------------------------
    public partial class FlightPlanViewModel
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The airport cache.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly Dictionary<string, Airport> airportCache = new();

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The airport markers cache.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly Dictionary<string, List<TrackingEventMarker>> airportMarkersCache = new();

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The alternate airport.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Airport alternateAirport;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The alternate icao.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string alternateICAO;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The alternate invalid warning visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Visibility alternateInvalidWarningVisibility = Visibility.Collapsed;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Destination airport.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Airport destinationAirport;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The destination icao.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string destinationICAO;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Destination invalid warning visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Visibility destinationInvalidWarningVisibility = Visibility.Collapsed;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The origin airport.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Airport originAirport;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The origin icao.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string originICAO;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The origin invalid warning visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Visibility originInvalidWarningVisibility = Visibility.Collapsed;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the alternate airport.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Airport AlternateAirport
        {
            get => this.alternateAirport;

            set
            {
                if (Equals(this.alternateAirport, value))
                {
                    return;
                }

                this.alternateAirport = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged(nameof(this.AlternateAirportFuelWarningVisibility));
                this.NotifyPropertyChanged(nameof(this.AlternateAirportShortRunwayWarningVisibility));
                this.NotifyPropertyChanged(nameof(this.AlternateAirportShortRunwayErrorVisibility));
                this.NotifyPropertyChanged(nameof(this.AlternateAirportClosedErrorVisibility));
                this.NotifyPropertyChanged(nameof(this.AlternateWrongSimErrorVisibility));
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the alternate wrong simulator error visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Visibility AlternateWrongSimErrorVisibility
        {
            get
            {
                if (this.AlternateAirport != null && this.SelectedAircraft != null)
                {
                    if (this.SelectedAircraft.Type.Simulator == Simulator.MSFS && !this.AlternateAirport.Msfs)
                    {
                        return Visibility.Visible;
                    }

                    if (this.SelectedAircraft.Type.Simulator == Simulator.XPlane11 && !this.AlternateAirport.XP11)
                    {
                        return Visibility.Visible;
                    }
                }

                return Visibility.Collapsed;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the origin wrong simulator error visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Visibility OriginWrongSimErrorVisibility
        {
            get
            {
                if (this.OriginAirport != null && this.SelectedAircraft != null)
                {
                    if (this.SelectedAircraft.Type.Simulator == Simulator.MSFS && !this.OriginAirport.Msfs)
                    {
                        return Visibility.Visible;
                    }

                    if (this.SelectedAircraft.Type.Simulator == Simulator.XPlane11 && !this.OriginAirport.XP11)
                    {
                        return Visibility.Visible;
                    }
                }

                return Visibility.Collapsed;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the destination wrong simulator error visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Visibility DestinationWrongSimErrorVisibility
        {
            get
            {
                if (this.DestinationAirport != null && this.SelectedAircraft != null)
                {
                    if (this.SelectedAircraft.Type.Simulator == Simulator.MSFS && !this.DestinationAirport.Msfs)
                    {
                        return Visibility.Visible;
                    }

                    if (this.SelectedAircraft.Type.Simulator == Simulator.XPlane11 && !this.DestinationAirport.XP11)
                    {
                        return Visibility.Visible;
                    }
                }

                return Visibility.Collapsed;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the alternate airport closed error visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Visibility AlternateAirportClosedErrorVisibility => this.AlternateAirport?.IsClosed == true ? Visibility.Visible : Visibility.Collapsed;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the alternate airport fuel warning visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Visibility AlternateAirportFuelWarningVisibility
        {
            get
            {
                if (this.AlternateAirport != null && this.SelectedAircraft != null)
                {
                    if (this.SelectedAircraft.Type.FuelType == FuelType.AvGas && !this.AlternateAirport.HasAvGas)
                    {
                        return Visibility.Visible;
                    }

                    if (this.SelectedAircraft.Type.FuelType == FuelType.JetFuel && !this.AlternateAirport.HasJetFuel)
                    {
                        return Visibility.Visible;
                    }
                }

                return Visibility.Collapsed;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the alternate airport short runway error visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Visibility AlternateAirportShortRunwayErrorVisibility
        {
            get
            {
                if (this.AlternateAirport != null && this.SelectedAircraft != null)
                {
                    var runways = this.AlternateAirport.Runways;
                    if (this.SelectedAircraft.Type.RequiresHardSurfaceRunway)
                    {
                        runways = runways.Where(r => r.Surface.ParseRunwaySurface().IsHardSurface()).ToList();
                    }

                    if (runways.Count == 0)
                    {
                        return Visibility.Visible;
                    }

                    var longestRunwayDelta = runways.Max(r => r.Length) - this.SelectedAircraft.Type.MinimumRunwayLength;
                    return longestRunwayDelta < -1000 ? Visibility.Visible : Visibility.Collapsed;
                }

                return Visibility.Collapsed;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the alternate airport short runway warning visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Visibility AlternateAirportShortRunwayWarningVisibility
        {
            get
            {
                if (this.AlternateAirport != null && this.SelectedAircraft != null)
                {
                    var runways = this.AlternateAirport.Runways;
                    if (this.SelectedAircraft.Type.RequiresHardSurfaceRunway)
                    {
                        runways = runways.Where(r => r.Surface.ParseRunwaySurface().IsHardSurface()).ToList();
                    }

                    if (runways.Count == 0)
                    {
                        return Visibility.Collapsed;
                    }

                    var longestRunwayDelta = runways.Max(r => r.Length) - this.SelectedAircraft.Type.MinimumRunwayLength;
                    return longestRunwayDelta is >= -1000 and < 0 ? Visibility.Visible : Visibility.Collapsed;
                }

                return Visibility.Collapsed;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the alternate icao.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string AlternateICAO
        {
            get => this.alternateICAO;

            set
            {
                if (Equals(this.alternateICAO, value))
                {
                    return;
                }

                this.alternateICAO = value;
                this.NotifyPropertyChanged();
                this.IsDirty = true;
                UpdateGUIDelegate updateAirports = () => this.UpdateAirportsCommand.DoExecute(null);
                Application.Current.Dispatcher.BeginInvoke(updateAirports);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the alternate invalid warning visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Visibility AlternateInvalidWarningVisibility
        {
            get => this.alternateInvalidWarningVisibility;

            set
            {
                if (Equals(this.alternateInvalidWarningVisibility, value))
                {
                    return;
                }

                this.alternateInvalidWarningVisibility = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets destination airport.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Airport DestinationAirport
        {
            get => this.destinationAirport;

            set
            {
                if (Equals(this.destinationAirport, value))
                {
                    return;
                }

                this.destinationAirport = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged(nameof(this.DestinationAirportFuelWarningVisibility));
                this.NotifyPropertyChanged(nameof(this.DestinationAirportShortRunwayWarningVisibility));
                this.NotifyPropertyChanged(nameof(this.DestinationAirportShortRunwayErrorVisibility));
                this.NotifyPropertyChanged(nameof(this.DestinationAirportClosedErrorVisibility));
                this.NotifyPropertyChanged(nameof(this.DestinationWrongSimErrorVisibility));
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets destination airport closed error visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Visibility DestinationAirportClosedErrorVisibility => this.DestinationAirport?.IsClosed == true ? Visibility.Visible : Visibility.Collapsed;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets destination airport fuel warning visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Visibility DestinationAirportFuelWarningVisibility
        {
            get
            {
                if (this.DestinationAirport != null && this.SelectedAircraft != null)
                {
                    if (this.SelectedAircraft.Type.FuelType == FuelType.AvGas && !this.DestinationAirport.HasAvGas)
                    {
                        return Visibility.Visible;
                    }

                    if (this.SelectedAircraft.Type.FuelType == FuelType.JetFuel && !this.DestinationAirport.HasJetFuel)
                    {
                        return Visibility.Visible;
                    }
                }

                return Visibility.Collapsed;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets destination airport short runway error visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Visibility DestinationAirportShortRunwayErrorVisibility
        {
            get
            {
                if (this.DestinationAirport != null && this.SelectedAircraft != null)
                {
                    var runways = this.DestinationAirport.Runways;
                    if (this.SelectedAircraft.Type.RequiresHardSurfaceRunway)
                    {
                        runways = runways.Where(r => r.Surface.ParseRunwaySurface().IsHardSurface()).ToList();
                    }

                    if (runways.Count == 0)
                    {
                        return Visibility.Visible;
                    }

                    var longestRunwayDelta = runways.Max(r => r.Length) - this.SelectedAircraft.Type.MinimumRunwayLength;
                    return longestRunwayDelta < -1000 ? Visibility.Visible : Visibility.Collapsed;
                }

                return Visibility.Collapsed;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets destination airport short runway warning visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Visibility DestinationAirportShortRunwayWarningVisibility
        {
            get
            {
                if (this.DestinationAirport != null && this.SelectedAircraft != null)
                {
                    var runways = this.DestinationAirport.Runways;
                    if (this.SelectedAircraft.Type.RequiresHardSurfaceRunway)
                    {
                        runways = runways.Where(r => r.Surface.ParseRunwaySurface().IsHardSurface()).ToList();
                    }

                    if (runways.Count == 0)
                    {
                        return Visibility.Collapsed;
                    }

                    var longestRunwayDelta = runways.Max(r => r.Length) - this.SelectedAircraft.Type.MinimumRunwayLength;
                    return longestRunwayDelta is >= -1000 and < 0 ? Visibility.Visible : Visibility.Collapsed;
                }

                return Visibility.Collapsed;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the destination icao.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string DestinationICAO
        {
            get => this.destinationICAO;

            set
            {
                if (Equals(this.destinationICAO, value))
                {
                    return;
                }

                this.destinationICAO = value;
                this.NotifyPropertyChanged();
                this.IsDirty = true;
                UpdateGUIDelegate updateAirports = () => this.UpdateAirportsCommand.DoExecute(null);
                Application.Current.Dispatcher.BeginInvoke(updateAirports);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets destination invalid warning visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Visibility DestinationInvalidWarningVisibility
        {
            get => this.destinationInvalidWarningVisibility;

            set
            {
                if (Equals(this.destinationInvalidWarningVisibility, value))
                {
                    return;
                }

                this.destinationInvalidWarningVisibility = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the origin airport.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Airport OriginAirport
        {
            get => this.originAirport;

            set
            {
                if (Equals(this.originAirport, value))
                {
                    return;
                }

                this.originAirport = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged(nameof(this.FuelPricePerGallon));
                this.NotifyPropertyChanged(nameof(this.FuelPrice));
                this.NotifyPropertyChanged(nameof(this.OriginWrongSimErrorVisibility));
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the origin icao.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string OriginICAO
        {
            get => this.originICAO;

            set
            {
                if (Equals(this.originICAO, value))
                {
                    return;
                }

                this.originICAO = value;
                this.NotifyPropertyChanged();
                this.IsDirty = true;

                UpdateGUIDelegate updateOriginRelated = () =>
                {
                    this.UpdateAirportsCommand.DoExecute(null);
                    this.UpdateAircraftDistances();
                    this.UpdatePlannablePayloads();
                };
                Application.Current.Dispatcher.BeginInvoke(updateOriginRelated);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the origin invalid warning visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Visibility OriginInvalidWarningVisibility
        {
            get => this.originInvalidWarningVisibility;

            set
            {
                if (Equals(this.originInvalidWarningVisibility, value))
                {
                    return;
                }

                this.originInvalidWarningVisibility = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the update airports command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand UpdateAirportsCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Add map markers for the specified airport.
        /// </summary>
        /// <remarks>
        /// sushi.at, 26/01/2022.
        /// </remarks>
        /// <param name="airport">
        /// The airport.
        /// </param>
        /// <param name="markerColor">
        /// The marker color.
        /// </param>
        /// <param name="textColor">
        /// The text color.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void AddMarkersForAirport(Airport airport, Color markerColor, Color textColor)
        {
            if (!this.airportMarkersCache.ContainsKey(airport.Icao))
            {
                this.airportMarkersCache.Add(airport.Icao, new List<TrackingEventMarker>());
                var airportMarker = new TrackingEventMarker(new GeoCoordinate(airport.Latitude, airport.Longitude), airport.Icao, markerColor, textColor);
                this.airportMarkersCache[airport.Icao].Add(airportMarker);
                this.TrackingEventMarkers.Add(airportMarker);

                var airportDetailMarker = new TrackingEventMarker(airport, markerColor, textColor);
                this.airportMarkersCache[airport.Icao].Add(airportDetailMarker);
                this.TrackingEventMarkers.Add(airportDetailMarker);

                foreach (var runway in airport.Runways)
                {
                    var runwayMarker = new TrackingEventMarker(runway);
                    this.airportMarkersCache[airport.Icao].Add(runwayMarker);
                    this.TrackingEventMarkers.Add(runwayMarker);
                }
            }
            else
            {
                this.TrackingEventMarkers.AddRange(this.airportMarkersCache[airport.Icao]);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Update airports and their map markers.
        /// </summary>
        /// <remarks>
        /// sushi.at, 26/01/2022.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void UpdateAirports()
        {
            Debug.WriteLine("Updating airports...");
            try
            {
                if (this.TrackingEventMarkers.Count > 0)
                {
                    this.UpdateAirportsCommand.ReportProgress(
                        () =>
                        {
                            this.TrackingEventMarkers.Clear();
                            this.RouteTrailLocations.Clear();
                        }, true);
                }

                // Alternate
                if (!string.IsNullOrEmpty(this.AlternateICAO))
                {
                    try
                    {
                        this.AlternateAirport = null;
                        if (!this.airportCache.ContainsKey(this.AlternateICAO))
                        {
                            if (this.AlternateICAO.Length < 3)
                            {
                                this.AlternateInvalidWarningVisibility = Visibility.Visible;
                            }
                            else
                            {
                                var result = OpenSkyService.Instance.GetAirportAsync(this.AlternateICAO).Result;
                                if (!result.IsError)
                                {
                                    if (result.Data.Icao != "XXXX")
                                    {
                                        this.airportCache.Add(this.AlternateICAO, result.Data);
                                        this.AlternateAirport = result.Data;
                                        this.AlternateInvalidWarningVisibility = Visibility.Collapsed;
                                        this.UpdateAirportsCommand.ReportProgress(() => this.AddMarkersForAirport(this.AlternateAirport, OpenSkyColors.OpenSkyWarningOrange, Colors.Black));
                                    }
                                    else
                                    {
                                        this.AlternateInvalidWarningVisibility = Visibility.Visible;
                                        this.airportCache.Add(this.AlternateICAO, null);
                                    }
                                }
                                else
                                {
                                    var message = result.Message;
                                    if (!string.IsNullOrEmpty(result.ErrorDetails))
                                    {
                                        message += $"\r\n\r\n{result.ErrorDetails}";
                                    }

                                    throw new Exception(message);
                                }
                            }
                        }
                        else
                        {
                            if (this.airportCache[this.AlternateICAO] != null)
                            {
                                this.AlternateAirport = this.airportCache[this.AlternateICAO];
                                this.AlternateInvalidWarningVisibility = Visibility.Collapsed;
                                this.UpdateAirportsCommand.ReportProgress(() => this.AddMarkersForAirport(this.AlternateAirport, OpenSkyColors.OpenSkyWarningOrange, Colors.Black));
                            }
                            else
                            {
                                this.AlternateInvalidWarningVisibility = Visibility.Visible;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ex.HandleApiCallException(this.ViewReference, this.UpdateAirportsCommand, "Error retrieving alternate airport information.");
                    }
                }

                // Origin
                if (!string.IsNullOrEmpty(this.OriginICAO))
                {
                    try
                    {
                        this.OriginAirport = null;
                        if (!this.airportCache.ContainsKey(this.OriginICAO))
                        {
                            if (this.OriginICAO.Length < 3)
                            {
                                this.OriginInvalidWarningVisibility = Visibility.Visible;
                            }
                            else
                            {
                                var result = OpenSkyService.Instance.GetAirportAsync(this.OriginICAO).Result;
                                if (!result.IsError)
                                {
                                    if (result.Data.Icao != "XXXX")
                                    {
                                        this.airportCache.Add(this.OriginICAO, result.Data);
                                        this.OriginAirport = result.Data;
                                        this.OriginInvalidWarningVisibility = Visibility.Collapsed;
                                        this.UpdateAirportsCommand.ReportProgress(() =>
                                        {
                                            this.RouteTrailLocations.Add(new Location(this.OriginAirport.Latitude, this.OriginAirport.Longitude));
                                            this.AddMarkersForAirport(this.OriginAirport, OpenSkyColors.OpenSkyTeal, Colors.White);
                                        });
                                    }
                                    else
                                    {
                                        this.OriginInvalidWarningVisibility = Visibility.Visible;
                                        this.airportCache.Add(this.OriginICAO, null);
                                    }
                                }
                                else
                                {
                                    var message = result.Message;
                                    if (!string.IsNullOrEmpty(result.ErrorDetails))
                                    {
                                        message += $"\r\n\r\n{result.ErrorDetails}";
                                    }

                                    throw new Exception(message);
                                }
                            }
                        }
                        else
                        {
                            if (this.airportCache[this.OriginICAO] != null)
                            {
                                this.OriginAirport = this.airportCache[this.OriginICAO];
                                this.OriginInvalidWarningVisibility = Visibility.Collapsed;
                                this.UpdateAirportsCommand.ReportProgress(
                                    () =>
                                    {
                                        this.RouteTrailLocations.Add(new Location(this.OriginAirport.Latitude, this.OriginAirport.Longitude));
                                        this.AddMarkersForAirport(this.OriginAirport, OpenSkyColors.OpenSkyTeal, Colors.White);
                                    });
                            }
                            else
                            {
                                this.OriginInvalidWarningVisibility = Visibility.Visible;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ex.HandleApiCallException(this.ViewReference, this.UpdateAirportsCommand, "Error retrieving origin airport information.");
                    }
                }

                // Destination
                if (!string.IsNullOrEmpty(this.DestinationICAO))
                {
                    try
                    {
                        this.DestinationAirport = null;
                        if (!this.airportCache.ContainsKey(this.DestinationICAO))
                        {
                            if (this.DestinationICAO.Length < 3)
                            {
                                this.DestinationInvalidWarningVisibility = Visibility.Visible;
                            }
                            else
                            {
                                var result = OpenSkyService.Instance.GetAirportAsync(this.DestinationICAO).Result;
                                if (!result.IsError)
                                {
                                    if (result.Data.Icao != "XXXX")
                                    {
                                        this.airportCache.Add(this.DestinationICAO, result.Data);
                                        this.DestinationAirport = result.Data;
                                        this.DestinationInvalidWarningVisibility = Visibility.Collapsed;
                                        this.UpdateAirportsCommand.ReportProgress(() =>
                                        {
                                            this.RouteTrailLocations.Add(new Location(this.DestinationAirport.Latitude, this.DestinationAirport.Longitude));
                                            this.AddMarkersForAirport(this.DestinationAirport, OpenSkyColors.OpenSkyTeal, Colors.White);
                                        });
                                    }
                                    else
                                    {
                                        this.DestinationInvalidWarningVisibility = Visibility.Visible;
                                        this.airportCache.Add(this.DestinationICAO, null);
                                    }
                                }
                                else
                                {
                                    var message = result.Message;
                                    if (!string.IsNullOrEmpty(result.ErrorDetails))
                                    {
                                        message += $"\r\n\r\n{result.ErrorDetails}";
                                    }

                                    throw new Exception(message);
                                }
                            }
                        }
                        else
                        {
                            if (this.airportCache[this.DestinationICAO] != null)
                            {
                                this.DestinationAirport = this.airportCache[this.DestinationICAO];
                                this.DestinationInvalidWarningVisibility = Visibility.Collapsed;
                                this.UpdateAirportsCommand.ReportProgress(
                                    () =>
                                    {
                                        this.RouteTrailLocations.Add(new Location(this.DestinationAirport.Latitude, this.DestinationAirport.Longitude));
                                        this.AddMarkersForAirport(this.DestinationAirport, OpenSkyColors.OpenSkyTeal, Colors.White);
                                    });
                            }
                            else
                            {
                                this.DestinationInvalidWarningVisibility = Visibility.Visible;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ex.HandleApiCallException(this.ViewReference, this.UpdateAirportsCommand, "Error retrieving destination airport information.");
                    }
                }

                this.UpdateAirportsCommand.ReportProgress(() => { this.MapUpdated?.Invoke(this, EventArgs.Empty); });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                this.UpdateAirportsCommand.ReportProgress(
                    () =>
                    {
                        var notification = new OpenSkyNotification(new ErrorDetails { DetailedMessage = ex.Message, Exception = ex }, "Update airports", ex.Message, ExtendedMessageBoxImage.Error, 30);
                        notification.SetErrorColorStyle();
                        Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                    });
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the route trail locations.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public LocationCollection RouteTrailLocations { get; }
    }
}
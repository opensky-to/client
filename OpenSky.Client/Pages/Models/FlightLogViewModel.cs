// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FlightLogViewModel.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Pages.Models
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Device.Location;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Text;
    using System.Windows;
    using System.Windows.Media;
    using System.Xml.Linq;

    using Microsoft.Maps.MapControl.WPF;

    using OpenSky.Client.Controls;
    using OpenSky.Client.Controls.Models;
    using OpenSky.Client.MVVM;
    using OpenSky.Client.Tools;
    using OpenSky.Client.Views;
    using OpenSky.FlightLogXML;

    using OpenSkyApi;

    using TrackingEventLogEntry = OpenSky.Client.Controls.Models.TrackingEventLogEntry;
    using TrackingEventMarker = OpenSky.Client.Controls.Models.TrackingEventMarker;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Flight log view model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 17/11/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class FlightLogViewModel : ViewModel
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// True if chart altitude is checked.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private bool chartAltitudeChecked = true;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// True if chart fuel is checked.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private bool chartFuelChecked = true;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// True if chart ground speed is checked.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private bool chartGroundSpeedChecked = true;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// True if chart simulation rate is checked.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private bool chartSimRateChecked = true;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The flight log.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private OpenSkyApi.FlightLog flightLog;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Information describing the landing grade.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string landingGradeDescription;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The landing report visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Visibility landingReportVisibility = Visibility.Collapsed;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The loading text.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string loadingText;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The OFP HTML.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string ofpHtml;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The OFP visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Visibility ofpVisibility = Visibility.Collapsed;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The payload.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string payload;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// GThe payload weight.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private double payloadWeight;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The show hide landing report button text.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string showHideLandingReportButtonText = "Show Landing Report";

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The show hide ofp button text.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string showHideOFPButtonText = "Show OFP";

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="FlightLogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 17/11/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public FlightLogViewModel()
        {
            // Initialize data structures
            this.AircraftTrailLocations = new LocationCollection();
            this.TrackingEventLogEntries = new ObservableCollection<TrackingEventLogEntry>();
            this.TrackingEventMarkers = new ObservableCollection<TrackingEventMarker>();
            this.SimbriefRouteLocations = new LocationCollection();
            this.SimbriefWaypointMarkers = new ObservableCollection<SimbriefWaypointMarker>();
            this.TouchDowns = new List<TouchDown>();

            // Create commands
            this.LoadFlightLogCommand = new AsynchronousCommand(this.LoadFlightLog);
            this.ToggleOfpCommand = new Command(this.ToggleOfp);
            this.ToggleLandingReportCommand = new Command(this.ToggleLandingReport);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Occurs when the map was updated, tell the mapviewer to zoom to contained child elements.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public event EventHandler MapUpdated;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the aircraft trail locations.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public LocationCollection AircraftTrailLocations { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the airspeed (only from first touchdown).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double Airspeed => this.TouchDowns.Count > 0 ? this.TouchDowns[0].Airspeed : 0.0;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the altitude series visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Visibility AltitudeAxisAndSeriesVisibility => this.ChartAltitudeChecked ? Visibility.Visible : Visibility.Collapsed;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the bounces.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int Bounces => this.TouchDowns.Count > 1 ? this.TouchDowns.Count - 1 : 0;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether the chart altitude is checked.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool ChartAltitudeChecked
        {
            get => this.chartAltitudeChecked;

            set
            {
                if (Equals(this.chartAltitudeChecked, value))
                {
                    return;
                }

                this.chartAltitudeChecked = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged(nameof(this.AltitudeAxisAndSeriesVisibility));
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether the chart fuel is checked.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool ChartFuelChecked
        {
            get => this.chartFuelChecked;

            set
            {
                if (Equals(this.chartFuelChecked, value))
                {
                    return;
                }

                this.chartFuelChecked = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged(nameof(this.FuelAxisAndSeriesVisibility));
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether the chart ground speed is checked.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool ChartGroundSpeedChecked
        {
            get => this.chartGroundSpeedChecked;

            set
            {
                if (Equals(this.chartGroundSpeedChecked, value))
                {
                    return;
                }

                this.chartGroundSpeedChecked = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged(nameof(this.GroundSpeedAxisAndSeriesVisibility));
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether the chart simulation rate is checked.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool ChartSimRateChecked
        {
            get => this.chartSimRateChecked;

            set
            {
                if (Equals(this.chartSimRateChecked, value))
                {
                    return;
                }

                this.chartSimRateChecked = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged(nameof(this.SimRateAxisAndSeriesVisibility));
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the cross wind (only from first touchdown).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string CrossWind => this.TouchDowns.Count > 0 ? this.TouchDowns[0].CrossWind.ToString("Left, 0.00;Right, 0.00;None, 0") : "0";

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the flight log.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public OpenSkyApi.FlightLog FlightLog
        {
            get => this.flightLog;

            set
            {
                if (Equals(this.flightLog, value))
                {
                    return;
                }

                this.flightLog = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the fuel axis and series visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Visibility FuelAxisAndSeriesVisibility => this.ChartFuelChecked ? Visibility.Visible : Visibility.Collapsed;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the ground speed (only from first touchdown).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double GroundSpeed => this.TouchDowns.Count > 0 ? this.TouchDowns[0].GroundSpeed : 0.0;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the ground speed series visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Visibility GroundSpeedAxisAndSeriesVisibility => this.ChartGroundSpeedChecked ? Visibility.Visible : Visibility.Collapsed;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the head wind (only from first touchdown).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string HeadWind => this.TouchDowns.Count > 0 ? this.TouchDowns[0].HeadWind.ToString("Tail, 0.00;Head, 0.00;None, 0") : "0";

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the landing grade (also sets/updates LandingGradeDescription).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string LandingGrade
        {
            get
            {
                // ----------------------------------------------------
                // Landing rate (absolute)
                // ----------------------------------------------------
                // A+ Butter <=60 (PISTON,TURBO)
                // A+ Perfect >60 <=130 (JET)
                // A- Too soft <=60 (JET)
                // A  Good >60 <=180
                // B  OK >180 <=240
                // C  Hard >240 <=600
                // D  Rough >600 <=1000 (Inspection)
                // F  Crash >1000 (Repair)

                // ----------------------------------------------------
                // G-Force
                // ----------------------------------------------------
                // C  Hard >2.1

                // ----------------------------------------------------
                // Bounces
                // ----------------------------------------------------
                // C  Bouncy >1
                // D  Porpoising >2

                // ----------------------------------------------------
                // Wind todo add wind ratings
                // ----------------------------------------------------
                // E  Dangerous Cross >40 (JET) >20 (PISTON,TURBO)
                // E  Dangerous Tail >15 (JET) >5 (PISTON,TURBO)

                // ----------------------------------------------------
                // Sideslip
                // ----------------------------------------------------
                // E  Dangerous <-15 or >15

                // ----------------------------------------------------
                // Bank angle
                // ----------------------------------------------------
                // E  Dangerous <-5 or >5

                var landingRateAbs = Math.Abs(this.MaxLandingRate);
                var grade = "A+";
                var desc = "Butter landing";

                if (landingRateAbs > 1000)
                {
                    grade = "F";
                    desc = "Crash landing";
                }

                if (grade == "A+" && (this.MaxBankAngle is < -5.0 or > 5.0))
                {
                    grade = "E";
                    desc = "Dangerous bank angle";
                }

                if (grade == "A+" && (this.MaxSideSlipAngle is < -15.0 or > 15.0))
                {
                    grade = "E";
                    desc = "Dangerous sideslip angle";
                }

                if (grade == "A+" && landingRateAbs is > 600 and <= 1000)
                {
                    grade = "D";
                    desc = "Rough landing";
                }

                if (grade == "A+" && this.Bounces > 2)
                {
                    grade = "D";
                    desc = "Porpoising landing";
                }

                if (grade == "A+" && (this.MaxGForce is < -2.1 or > 2.1))
                {
                    grade = "C";
                    desc = "Hard landing";
                }

                if (grade == "A+" && landingRateAbs is > 240 and <= 600)
                {
                    grade = "C";
                    desc = "Hard landing";
                }

                if (grade == "A+" && this.Bounces > 1)
                {
                    grade = "C";
                    desc = "Bouncy landing";
                }

                if (grade == "A+" && landingRateAbs is > 180 and <= 240)
                {
                    grade = "B";
                    desc = "OK landing";
                }

                if (grade == "A+" && landingRateAbs is > 60 and <= 180)
                {
                    grade = "A";
                    desc = "Good landing";

                    if (landingRateAbs <= 130 && this.FlightLog?.AircraftEngineType == EngineType.Jet)
                    {
                        grade = "A+";
                        desc = "Perfect landing";
                    }
                }

                if (grade == "A+" && this.FlightLog?.AircraftEngineType == EngineType.Jet)
                {
                    grade = "A-";
                    desc = "Lading too soft";
                }

                this.LandingGradeDescription = desc;
                return grade;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the information string describing the landing grade.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string LandingGradeDescription
        {
            get => this.landingGradeDescription;

            private set
            {
                if (Equals(this.landingGradeDescription, value))
                {
                    return;
                }

                this.landingGradeDescription = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the landing report visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Visibility LandingReportVisibility
        {
            get => this.landingReportVisibility;

            set
            {
                if (Equals(this.landingReportVisibility, value))
                {
                    return;
                }

                this.landingReportVisibility = value;
                this.NotifyPropertyChanged();
                this.ShowHideLandingReportButtonText = value == Visibility.Collapsed ? "Show Landing Report" : "Hide Landing Report";
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the load flight log command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand LoadFlightLogCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the loading text.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string LoadingText
        {
            get => this.loadingText;

            set
            {
                if (Equals(this.loadingText, value))
                {
                    return;
                }

                this.loadingText = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the maximum bank angle.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double MaxBankAngle
        {
            get
            {
                if (this.TouchDowns.Count > 0)
                {
                    var max = this.TouchDowns.Max(lr => lr.BankAngle);
                    var min = this.TouchDowns.Min(lr => lr.BankAngle);
                    return Math.Abs(min) > Math.Abs(max) ? min : max;
                }

                return 0;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the maximum bank angle info string.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string MaxBankAngleInfo => this.MaxBankAngle.ToString("Left, 0.00;Right, 0.00;None, 0");

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the maximum g-force.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double MaxGForce => this.TouchDowns.Count > 0 ? this.TouchDowns.Max(lr => lr.GForce) : 0.0;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the maximum landing rate (Max landing rate is actually MIN (as landing rates are negative)).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double MaxLandingRate => this.TouchDowns.Count > 0 ? this.TouchDowns.Min(lr => lr.LandingRate) : 0.0;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the maximum side slip angle.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double MaxSideSlipAngle
        {
            get
            {
                if (this.TouchDowns.Count > 0)
                {
                    var max = this.TouchDowns.Max(lr => lr.SideSlipAngle);
                    var min = this.TouchDowns.Min(lr => lr.SideSlipAngle);
                    return Math.Abs(min) > Math.Abs(max) ? min : max;
                }

                return 0;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the maximum side slip angle info text.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string MaxSideSlipAngleInfo => this.MaxSideSlipAngle.ToString("Left, 0.00;Right, 0.00;None, 0");

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the OFP HTML.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string OfpHtml
        {
            get => this.ofpHtml;

            set
            {
                if (Equals(this.ofpHtml, value))
                {
                    return;
                }

                this.ofpHtml = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the OFP visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Visibility OfpVisibility
        {
            get => this.ofpVisibility;

            set
            {
                if (Equals(this.ofpVisibility, value))
                {
                    return;
                }

                this.ofpVisibility = value;
                this.NotifyPropertyChanged();
                this.ShowHideOFPButtonText = value == Visibility.Collapsed ? "Show OFP" : "Hide OFP";
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the payload.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string Payload
        {
            get => this.payload;

            set
            {
                if (Equals(this.payload, value))
                {
                    return;
                }

                this.payload = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the payload weight.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double PayloadWeight
        {
            get => this.payloadWeight;

            set
            {
                if (Equals(this.payloadWeight, value))
                {
                    return;
                }

                this.payloadWeight = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the show hide landing report button text.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string ShowHideLandingReportButtonText
        {
            get => this.showHideLandingReportButtonText;

            set
            {
                if (Equals(this.showHideLandingReportButtonText, value))
                {
                    return;
                }

                this.showHideLandingReportButtonText = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the show hide ofp button text.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string ShowHideOFPButtonText
        {
            get => this.showHideOFPButtonText;

            set
            {
                if (Equals(this.showHideOFPButtonText, value))
                {
                    return;
                }

                this.showHideOFPButtonText = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the simbrief route locations.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public LocationCollection SimbriefRouteLocations { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the simbrief waypoint markers.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<SimbriefWaypointMarker> SimbriefWaypointMarkers { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the simulation rate axis and series visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Visibility SimRateAxisAndSeriesVisibility => this.ChartSimRateChecked ? Visibility.Visible : Visibility.Collapsed;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the toggle landing report command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command ToggleLandingReportCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the toggle OFP visibility command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command ToggleOfpCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the touch downs.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public List<TouchDown> TouchDowns { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the tracking event log entries.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<TrackingEventLogEntry> TrackingEventLogEntries { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the tracking event markers.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<TrackingEventMarker> TrackingEventMarkers { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the wind angle (compound, only from first touchdown).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double WindAngle => this.TouchDowns.Count > 0 ? this.TouchDowns[0].WindAngle : 0.0;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the wind knots (only from first touchdown).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double WindKnots => this.TouchDowns.Count > 0 ? this.TouchDowns[0].WindKnots : 0.0;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Loads the specified flight log.
        /// </summary>
        /// <remarks>
        /// sushi.at, 17/11/2021.
        /// </remarks>
        /// <param name="obj">
        /// The object.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void LoadFlightLog(object obj)
        {
            if (obj is OpenSkyApi.FlightLog log)
            {
                this.LoadingText = $"Loading flight log {log.FullFlightNumber}...";
                try
                {
                    Debug.WriteLine($"Loading flight log {log.FullFlightNumber}...");
                    this.FlightLog = log;

                    var result = OpenSkyService.Instance.GetFlightLogDetailsAsync(log.Id).Result;
                    if (!result.IsError)
                    {
                        this.OfpHtml = result.Data.OfpHtml;

                        var sourceStream = new MemoryStream(Convert.FromBase64String(result.Data.FlightLog));
                        var xmlStream = new MemoryStream();
                        using (var gzip = new GZipStream(sourceStream, CompressionMode.Decompress))
                        {
                            gzip.CopyTo(xmlStream);
                        }

                        xmlStream.Seek(0, SeekOrigin.Begin);
                        var xmlText = Encoding.UTF8.GetString(xmlStream.ToArray());

                        var flightLogXml = new FlightLogXML.FlightLog();
                        flightLogXml.RestoreFlightLog(XElement.Parse(xmlText));

                        this.LoadFlightLogCommand.ReportProgress(
                            () =>
                            {
                                this.PayloadWeight = flightLogXml.PayloadPounds;
                                this.Payload = flightLogXml.Payload;

                                this.TrackingEventMarkers.Clear();

                                var originMarker = new TrackingEventMarker(new GeoCoordinate(flightLogXml.Origin.Latitude, flightLogXml.Origin.Longitude), flightLogXml.Origin.Icao, OpenSkyColors.OpenSkyTeal, Colors.White);
                                this.TrackingEventMarkers.Add(originMarker);
                                var alternateMarker = new TrackingEventMarker(new GeoCoordinate(flightLogXml.Alternate.Latitude, flightLogXml.Alternate.Longitude), flightLogXml.Alternate.Icao, OpenSkyColors.OpenSkyWarningOrange, Colors.Black);
                                this.TrackingEventMarkers.Add(alternateMarker);
                                var destinationMarker = new TrackingEventMarker(new GeoCoordinate(flightLogXml.Destination.Latitude, flightLogXml.Destination.Longitude), flightLogXml.Destination.Icao, OpenSkyColors.OpenSkyTeal, Colors.White);
                                this.TrackingEventMarkers.Add(destinationMarker);

                                // Fetch airport details from local airport package
                                var localAirportPackage = AirportPackageClientHandler.GetPackage();
                                {
                                    var airport = localAirportPackage?.Airports.SingleOrDefault(a => a.ICAO == flightLogXml.Origin.Icao);
                                    if (airport != null)
                                    {
                                        var detailMarker = new TrackingEventMarker(airport, OpenSkyColors.OpenSkyTeal, Colors.White);
                                        this.TrackingEventMarkers.Add(detailMarker);
                                        foreach (var runway in airport.Runways)
                                        {
                                            var runwayMarker = new TrackingEventMarker(runway);
                                            this.TrackingEventMarkers.Add(runwayMarker);
                                        }
                                    }
                                }
                                {
                                    var airport = localAirportPackage?.Airports.SingleOrDefault(a => a.ICAO == flightLogXml.Alternate.Icao);
                                    if (airport != null)
                                    {
                                        var detailMarker = new TrackingEventMarker(airport, OpenSkyColors.OpenSkyWarningOrange, Colors.Black);
                                        this.TrackingEventMarkers.Add(detailMarker);
                                        foreach (var runway in airport.Runways)
                                        {
                                            var runwayMarker = new TrackingEventMarker(runway);
                                            this.TrackingEventMarkers.Add(runwayMarker);
                                        }
                                    }
                                }
                                {
                                    var airport = localAirportPackage?.Airports.SingleOrDefault(a => a.ICAO == flightLogXml.Destination.Icao);
                                    if (airport != null)
                                    {
                                        var detailMarker = new TrackingEventMarker(airport, OpenSkyColors.OpenSkyTeal, Colors.White);
                                        this.TrackingEventMarkers.Add(detailMarker);
                                        foreach (var runway in airport.Runways)
                                        {
                                            var runwayMarker = new TrackingEventMarker(runway);
                                            this.TrackingEventMarkers.Add(runwayMarker);
                                        }
                                    }
                                }

                                this.AircraftTrailLocations.Clear();
                                foreach (var location in flightLogXml.PositionReports)
                                {
                                    this.AircraftTrailLocations.Add(new AircraftTrailLocation(location));
                                }

                                this.TrackingEventLogEntries.Clear();
                                foreach (var logEntry in flightLogXml.TrackingEventLogEntries)
                                {
                                    this.TrackingEventLogEntries.Add(new TrackingEventLogEntry(logEntry));
                                }

                                foreach (var marker in flightLogXml.TrackingEventMarkers)
                                {
                                    this.TrackingEventMarkers.Add(new TrackingEventMarker(marker));
                                }

                                this.SimbriefRouteLocations.Clear();
                                this.SimbriefWaypointMarkers.Clear();
                                if (flightLogXml.NavLogWaypoints.Count > 0)
                                {
                                    this.SimbriefRouteLocations.Add(new Location(flightLogXml.Origin.Latitude, flightLogXml.Origin.Longitude));
                                    foreach (var waypoint in flightLogXml.NavLogWaypoints)
                                    {
                                        var waypointMarker = new SimbriefWaypointMarker(waypoint);
                                        this.SimbriefWaypointMarkers.Add(waypointMarker);
                                        this.SimbriefRouteLocations.Add(new Location(waypoint.Latitude, waypoint.Longitude));
                                    }

                                    this.SimbriefRouteLocations.Add(new Location(flightLogXml.Destination.Latitude, flightLogXml.Destination.Longitude));
                                }

                                this.TouchDowns.Clear();
                                if (flightLogXml.TouchDowns.Count > 0)
                                {
                                    foreach (var touchdown in flightLogXml.TouchDowns)
                                    {
                                        this.TouchDowns.Add(touchdown);
                                    }

                                    this.NotifyPropertyChanged(nameof(this.MaxLandingRate));
                                    this.NotifyPropertyChanged(nameof(this.MaxGForce));
                                    this.NotifyPropertyChanged(nameof(this.MaxSideSlipAngleInfo));
                                    this.NotifyPropertyChanged(nameof(this.Bounces));
                                    this.NotifyPropertyChanged(nameof(this.MaxBankAngleInfo));
                                    this.NotifyPropertyChanged(nameof(this.HeadWind));
                                    this.NotifyPropertyChanged(nameof(this.CrossWind));
                                    this.NotifyPropertyChanged(nameof(this.WindAngle));
                                    this.NotifyPropertyChanged(nameof(this.WindKnots));
                                    this.NotifyPropertyChanged(nameof(this.LandingGrade));
                                    this.NotifyPropertyChanged(nameof(this.Airspeed));
                                    this.NotifyPropertyChanged(nameof(this.GroundSpeed));
                                }

                                this.LoadFlightLogCommand.ReportProgress(() => this.MapUpdated?.Invoke(this, EventArgs.Empty));
                            });
                    }
                    else
                    {
                        this.LoadFlightLogCommand.ReportProgress(
                            () =>
                            {
                                Debug.WriteLine($"Error loading flight log {log.FullFlightNumber}: " + result.Message);
                                if (!string.IsNullOrEmpty(result.ErrorDetails))
                                {
                                    Debug.WriteLine(result.ErrorDetails);
                                }

                                var notification = new OpenSkyNotification($"Error loading flight log {log.FullFlightNumber}", result.Message, MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                                notification.SetErrorColorStyle();
                                Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                            });
                    }
                }
                catch (Exception ex)
                {
                    ex.HandleApiCallException(this.ViewReference, this.LoadFlightLogCommand, $"Error loading flight log {log.FullFlightNumber}");
                }
                finally
                {
                    this.LoadingText = null;
                }
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Toggle landing report visibility.
        /// </summary>
        /// <remarks>
        /// sushi.at, 18/11/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void ToggleLandingReport()
        {
            this.LandingReportVisibility = this.LandingReportVisibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Toggles the OFP.
        /// </summary>
        /// <remarks>
        /// sushi.at, 17/11/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void ToggleOfp()
        {
            this.OfpVisibility = this.OfpVisibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
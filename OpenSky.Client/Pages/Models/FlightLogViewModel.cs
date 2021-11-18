// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FlightLogViewModel.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Pages.Models
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Compression;
    using System.Text;
    using System.Windows;
    using System.Xml.Linq;

    using Microsoft.Maps.MapControl.WPF;

    using OpenSky.Client.Controls.Models;
    using OpenSky.Client.MVVM;
    using OpenSky.Client.Pages.Models.FlightLog;
    using OpenSky.Client.Tools;

    using OpenSkyApi;

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
        /// The loading text.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string loadingText;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The show hide ofp button text.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string showHideOFPButtonText = "Show OFP";

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

            // Create commands
            this.LoadFlightLogCommand = new AsynchronousCommand(this.LoadFlightLog);
            this.ToggleOfpCommand = new Command(this.ToggleOfp);
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
        /// The flight log.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private OpenSkyApi.FlightLog flightLog;

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
        /// The OFP HTML.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string ofpHtml;

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
        /// The OFP visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Visibility ofpVisibility = Visibility.Collapsed;

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
        /// Gets the toggle OFP visibility command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command ToggleOfpCommand { get; }

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
        /// Gets the aircraft trail locations.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public LocationCollection AircraftTrailLocations { get; }

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

                                this.TrackingEventMarkers.Clear();
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
                            });

                        // todo restore landing report, and decide how/where to display it
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

                                ModernWpf.MessageBox.Show(result.Message, $"Error loading flight log {log.FullFlightNumber}", MessageBoxButton.OK, MessageBoxImage.Error);
                            });
                    }
                }
                catch (Exception ex)
                {
                    ex.HandleApiCallException(this.LoadFlightLogCommand, $"Error loading flight log {log.FullFlightNumber}");
                }
                finally
                {
                    this.LoadingText = null;
                }
            }
        }
    }
}
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FlightPlanViewModel.SimBrief.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Pages.Models
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Threading;
    using System.Windows;
    using System.Xml.Linq;

    using Microsoft.Maps.MapControl.WPF;

    using OpenSky.Client.Controls;
    using OpenSky.Client.Controls.Models;
    using OpenSky.Client.MVVM;
    using OpenSky.Client.Tools;
    using OpenSky.Client.Views;

    using OpenSkyApi;

    /// -------------------------------------------------------------------------------------------------
    /// <content>
    /// Flight plan view model - simBrief.
    /// </content>
    /// -------------------------------------------------------------------------------------------------
    public partial class FlightPlanViewModel
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The navlog fixes.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly List<FlightNavlogFix> navlogFixes = new();

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The alternate route.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string alternateRoute;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The dispatcher remarks.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string dispatcherRemarks;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The simBrief OFP HTML.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string ofpHtml;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The route.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string route;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the alternate route.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string AlternateRoute
        {
            get => this.alternateRoute;

            set
            {
                if (Equals(this.alternateRoute, value))
                {
                    return;
                }

                this.alternateRoute = value;
                this.NotifyPropertyChanged();
                this.IsDirty = true;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the create simulation brief command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command CreateSimBriefCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the dispatcher remarks.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string DispatcherRemarks
        {
            get => this.dispatcherRemarks;

            set
            {
                if (Equals(this.dispatcherRemarks, value))
                {
                    return;
                }

                this.dispatcherRemarks = value;
                this.NotifyPropertyChanged();
                this.IsDirty = true;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the download simulation brief command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand DownloadSimBriefCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the simBrief OFP HTML.
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
                this.IsDirty = true;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the route.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string Route
        {
            get => this.route;

            set
            {
                if (Equals(this.route, value))
                {
                    return;
                }

                this.route = value;
                this.NotifyPropertyChanged();
                this.IsDirty = true;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the simbrief route locations.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public LocationCollection SimbriefRouteLocations { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the simbrief waypoint markers.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<SimbriefWaypointMarker> SimbriefWaypointMarkers { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Creates the dispatch URL for simbrief with the selected flight plan options.
        /// </summary>
        /// <remarks>
        /// sushi.at, 02/11/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void CreateSimBrief()
        {
            if (string.IsNullOrEmpty(UserSessionService.Instance.LinkedAccounts?.SimbriefUsername))
            {
                var notification = new OpenSkyNotification("simBrief OFP", "To use this feature, please configure your simBrief username in the settings!", MessageBoxButton.OK, ExtendedMessageBoxImage.Warning, 30);
                notification.SetWarningColorStyle();
                Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                return;
            }

            var url = "https://www.simbrief.com/system/dispatch.php?";
            if (this.IsAirlineFlight && !string.IsNullOrEmpty(this.Airline.Iata))
            {
                url += $"airline={this.Airline.Iata}&";
            }

            if (this.IsAirlineFlight && !string.IsNullOrEmpty(this.Airline.Icao))
            {
                url += $"airline={this.Airline.Icao}&";
            }

            url += $"fltnum={this.FlightNumber}&";

            if (!string.IsNullOrEmpty(this.OriginICAO))
            {
                url += $"orig={this.OriginICAO}&";
            }

            if (!string.IsNullOrEmpty(this.DestinationICAO))
            {
                url += $"dest={this.DestinationICAO}&";
            }

            if (!string.IsNullOrEmpty(this.Route))
            {
                url += $"route={WebUtility.UrlEncode(this.Route)}&";
            }

            if (!string.IsNullOrEmpty(this.AlternateICAO))
            {
                url += $"altn={this.AlternateICAO}&";

                // todo revisit alternate simBrief upload, seems to ignore all the advanced stuff and only accepts the primary
                url += "altn_count=1&";
                url += $"altn_1_id={this.AlternateICAO}&";

                if (!string.IsNullOrEmpty(this.AlternateRoute))
                {
                    url += $"altn_1_route={WebUtility.UrlEncode(this.AlternateRoute)}&";
                }
            }

            url += $"date={this.PlannedDepartureTime:ddMMMyy}&";
            url += $"deph={this.DepartureHour:00}&";
            url += $"depm={this.DepartureMinute:00}&";

            if (this.SelectedAircraft != null)
            {
                url += $"reg={this.SelectedAircraft.Registry.RemoveSimPrefix()}&";
            }

            url += $"cpt={WebUtility.UrlEncode(UserSessionService.Instance.Username)}&"; // todo add assigned airline pilot once we add that
            url += $"dxname={WebUtility.UrlEncode(UserSessionService.Instance.Username)}&";

            if (!string.IsNullOrEmpty(this.DispatcherRemarks))
            {
                url += $"manualrmk={WebUtility.UrlEncode(this.DispatcherRemarks.Replace("\r", string.Empty).Replace("\n", "\\n"))}&";
            }

            url += $"static_id={this.ID}&";

            Debug.WriteLine(url);
            Process.Start(url);

            var fuelNotification = new OpenSkyNotification(
                "simBrief OFP",
                "If you are planning to use the fuel numbers from the simBrief OFP, please make sure you correctly set the passengers and cargo after selecting your aircraft type.",
                MessageBoxButton.OK,
                ExtendedMessageBoxImage.Information,
                30);
            Main.ShowNotificationInSameViewAs(this.ViewReference, fuelNotification);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Downloads the created simBrief OFP.
        /// </summary>
        /// <remarks>
        /// sushi.at, 03/11/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void DownloadSimBrief(object param)
        {
            if (string.IsNullOrEmpty(UserSessionService.Instance.LinkedAccounts?.SimbriefUsername))
            {
                this.DownloadSimBriefCommand.ReportProgress(
                    () =>
                    {
                        var notification = new OpenSkyNotification("simBrief OFP", "To use this feature, please configure your simBrief username in the settings!", MessageBoxButton.OK, ExtendedMessageBoxImage.Warning, 30);
                        notification.SetWarningColorStyle();
                        Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                    });
                return;
            }

            this.LoadingText = "Downloading simBrief OFP...";
            ExtendedMessageBoxResult? answer;
            try
            {
                var ofpFetchUrl = $"https://www.simbrief.com/api/xml.fetcher.php?username={WebUtility.UrlEncode(UserSessionService.Instance.LinkedAccounts?.SimbriefUsername)}&static_id={this.ID}";
                if (param is true)
                {
                    ofpFetchUrl = $"https://www.simbrief.com/api/xml.fetcher.php?username={WebUtility.UrlEncode(UserSessionService.Instance.LinkedAccounts?.SimbriefUsername)}";
                }

                Debug.WriteLine(ofpFetchUrl);

                using var client = new WebClient();
                var xml = client.DownloadString(ofpFetchUrl);
                var ofp = XElement.Parse(xml);

                // Origin and Destination
                var sbOriginICAO = (string)ofp.Element("origin")?.Element("icao_code");
                var sbDestinationICAO = (string)ofp.Element("destination")?.Element("icao_code");
                if (!string.IsNullOrEmpty(sbOriginICAO) && !string.IsNullOrEmpty(sbDestinationICAO) &&
                    (!sbOriginICAO.Equals(this.OriginICAO, StringComparison.InvariantCultureIgnoreCase) || !sbDestinationICAO.Equals(this.DestinationICAO, StringComparison.InvariantCultureIgnoreCase)))
                {
                    if (string.IsNullOrEmpty(this.OriginICAO) && string.IsNullOrEmpty(this.DestinationICAO))
                    {
                        this.DownloadSimBriefCommand.ReportProgress(
                            () =>
                            {
                                this.OriginICAO = sbOriginICAO;
                                this.DestinationICAO = sbDestinationICAO;
                            });
                    }
                    else
                    {
                        answer = null;
                        this.DownloadSimBriefCommand.ReportProgress(
                            () =>
                            {
                                var messageBox = new OpenSkyMessageBox(
                                    "simBrief OFP",
                                    "Origin or destination ICAO don't match this flight plan. Do you want to use the values from simBrief?",
                                    MessageBoxButton.YesNo,
                                    ExtendedMessageBoxImage.Question);
                                messageBox.Closed += (_, _) => { answer = messageBox.Result; };
                                Main.ShowMessageBoxInSaveViewAs(this.ViewReference, messageBox);
                            });
                        while (answer == null && !SleepScheduler.IsShutdownInProgress)
                        {
                            Thread.Sleep(500);
                        }

                        if (answer == ExtendedMessageBoxResult.Yes)
                        {
                            this.DownloadSimBriefCommand.ReportProgress(
                                () =>
                                {
                                    this.OriginICAO = sbOriginICAO;
                                    this.DestinationICAO = sbDestinationICAO;
                                });
                        }
                    }
                }

                // Alternate
                var sbAlternateICAO = (string)ofp.Element("alternate")?.Element("icao_code");
                if (!string.IsNullOrEmpty(sbAlternateICAO))
                {
                    if (string.IsNullOrEmpty(this.AlternateICAO))
                    {
                        this.DownloadSimBriefCommand.ReportProgress(
                            () =>
                            {
                                this.AlternateICAO = sbAlternateICAO;
                            });
                    }
                    else if (!sbAlternateICAO.Equals(this.AlternateICAO, StringComparison.InvariantCultureIgnoreCase))
                    {
                        answer = null;
                        this.DownloadSimBriefCommand.ReportProgress(
                            () =>
                            {
                                var messageBox = new OpenSkyMessageBox(
                                    "simBrief OFP",
                                    "Alternate ICAO don't match this flight plan. Do you want to use the values from simBrief?",
                                    MessageBoxButton.YesNo,
                                    ExtendedMessageBoxImage.Question);
                                messageBox.Closed += (_, _) => { answer = messageBox.Result; };
                                Main.ShowMessageBoxInSaveViewAs(this.ViewReference, messageBox);
                            });
                        while (answer == null && !SleepScheduler.IsShutdownInProgress)
                        {
                            Thread.Sleep(500);
                        }

                        if (answer == ExtendedMessageBoxResult.Yes)
                        {
                            this.DownloadSimBriefCommand.ReportProgress(
                                () =>
                                {
                                    this.AlternateICAO = sbAlternateICAO;
                                });
                        }
                    }
                }

                // Fuel
                if (this.SelectedAircraft != null)
                {
                    var fuelPlanRamp = double.Parse((string)ofp.Element("fuel")?.Element("plan_ramp"));
                    var weightUnit = (string)ofp.Element("params")?.Element("units");
                    if ("kgs".Equals(weightUnit, StringComparison.InvariantCultureIgnoreCase))
                    {
                        // Convert to lbs
                        fuelPlanRamp *= 2.20462;
                    }

                    var plannedGallons = fuelPlanRamp / this.SelectedAircraft.Type.FuelWeightPerGallon;
                    if (plannedGallons < (this.FuelGallons ?? 0))
                    {
                        answer = null;
                        this.DownloadSimBriefCommand.ReportProgress(
                            () =>
                            {
                                var messageBox = new OpenSkyMessageBox(
                                    "simBrief OFP",
                                    "simBrief planned fuel is less than the currently selected value, do you want to use it anyway?",
                                    MessageBoxButton.YesNo,
                                    ExtendedMessageBoxImage.Question);
                                messageBox.Closed += (_, _) => { answer = messageBox.Result; };
                                Main.ShowMessageBoxInSaveViewAs(this.ViewReference, messageBox);
                            });
                        while (answer == null && !SleepScheduler.IsShutdownInProgress)
                        {
                            Thread.Sleep(500);
                        }

                        if (answer == ExtendedMessageBoxResult.Yes)
                        {
                            this.DownloadSimBriefCommand.ReportProgress(() => this.FuelGallons = plannedGallons);
                        }
                    }
                    else
                    {
                        this.DownloadSimBriefCommand.ReportProgress(() => this.FuelGallons = plannedGallons);
                    }
                }
                else
                {
                    this.DownloadSimBriefCommand.ReportProgress(
                        () =>
                        {
                            var notification = new OpenSkyNotification("simBrief OFP", "No aircraft selected, can't set planned fuel.", MessageBoxButton.OK, ExtendedMessageBoxImage.Information, 10);
                            Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                        });
                }

                // ZFW sanity check?
                // todo decide if we want to add ZFW sanity check to see if the aircraft from simbrief at least roughly matches the one here

                // Route
                var sbRoute = (string)ofp.Element("general")?.Element("route");
                if (!string.IsNullOrEmpty(sbRoute))
                {
                    // Add airports and runways to route
                    var sbOriginRunway = (string)ofp.Element("origin")?.Element("plan_rwy");
                    var sbDestinationRunway = (string)ofp.Element("destination")?.Element("plan_rwy");
                    if (!string.IsNullOrEmpty(sbOriginICAO))
                    {
                        var prefix = sbOriginICAO;
                        if (!string.IsNullOrEmpty(sbOriginRunway))
                        {
                            prefix += $"/{sbOriginRunway}";
                        }

                        sbRoute = $"{prefix} {sbRoute}";
                    }

                    if (!string.IsNullOrEmpty(sbDestinationICAO))
                    {
                        var postFix = sbDestinationICAO;
                        if (!string.IsNullOrEmpty(sbDestinationRunway))
                        {
                            postFix += $"/{sbDestinationRunway}";
                        }

                        sbRoute += $" {postFix}";
                    }

                    if (string.IsNullOrEmpty(this.Route))
                    {
                        this.Route = sbRoute;
                    }
                    else if (!sbRoute.Equals(this.Route, StringComparison.InvariantCultureIgnoreCase))
                    {
                        answer = null;
                        this.DownloadSimBriefCommand.ReportProgress(
                            () =>
                            {
                                var messageBox = new OpenSkyMessageBox(
                                    "simBrief OFP",
                                    "Route doesn't match this flight plan. Do you want to use the values from simBrief?",
                                    MessageBoxButton.YesNo,
                                    ExtendedMessageBoxImage.Question);
                                messageBox.Closed += (_, _) => { answer = messageBox.Result; };
                                Main.ShowMessageBoxInSaveViewAs(this.ViewReference, messageBox);
                            });
                        while (answer == null && !SleepScheduler.IsShutdownInProgress)
                        {
                            Thread.Sleep(500);
                        }

                        if (answer == ExtendedMessageBoxResult.Yes)
                        {
                            this.Route = sbRoute;
                        }
                    }
                }

                // Alternate route
                var sbAlternateRoute = (string)ofp.Element("alternate")?.Element("route");
                if (!string.IsNullOrEmpty(sbAlternateRoute))
                {
                    // Add airport and runway to route
                    var sbAlternateRunway = (string)ofp.Element("alternate")?.Element("plan_rwy");
                    if (!string.IsNullOrEmpty(sbAlternateICAO))
                    {
                        var postFix = sbAlternateICAO;
                        if (!string.IsNullOrEmpty(sbAlternateRunway))
                        {
                            postFix += $"/{sbAlternateRunway}";
                        }

                        sbAlternateRoute += $" {postFix}";
                    }

                    if (string.IsNullOrEmpty(this.AlternateRoute))
                    {
                        this.AlternateRoute = sbAlternateRoute;
                    }
                    else if (!sbAlternateRoute.Equals(this.AlternateRoute, StringComparison.InvariantCultureIgnoreCase))
                    {
                        answer = null;
                        this.DownloadSimBriefCommand.ReportProgress(
                            () =>
                            {
                                var messageBox = new OpenSkyMessageBox(
                                    "simBrief OFP",
                                    "Alternate route doesn't match this flight plan. Do you want to use the values from simBrief?",
                                    MessageBoxButton.YesNo,
                                    ExtendedMessageBoxImage.Question);
                                messageBox.Closed += (_, _) => { answer = messageBox.Result; };
                                Main.ShowMessageBoxInSaveViewAs(this.ViewReference, messageBox);
                            });
                        while (answer == null && !SleepScheduler.IsShutdownInProgress)
                        {
                            Thread.Sleep(500);
                        }

                        if (answer == ExtendedMessageBoxResult.Yes)
                        {
                            this.AlternateRoute = sbAlternateRoute;
                        }
                    }
                }

                // Dispatcher remarks
                var sbRemarks = string.Empty;
                foreach (var dispatcherRemark in ofp.Element("general")?.Elements("dx_rmk"))
                {
                    if (!string.IsNullOrEmpty(dispatcherRemark.Value))
                    {
                        sbRemarks += $"{dispatcherRemark.Value}\r\n";
                    }
                }

                sbRemarks = sbRemarks.TrimEnd('\r', '\n');
                if (this.DispatcherRemarks == null || sbRemarks.Length > this.DispatcherRemarks.Length)
                {
                    this.DispatcherRemarks = sbRemarks;
                }

                // OFP html
                var sbOfpHtml = (string)ofp.Element("text")?.Element("plan_html");
                if (!string.IsNullOrEmpty(sbOfpHtml))
                {
                    // todo maybe can use this in the future if we find more performant html rendering control
                    //if (!sbOfpHtml.StartsWith("<html>"))
                    //{
                    //    const string style = "body { background-color: #29323c; color: #c2c2c2; margin: -1px; } div { margin-top: 10px; margin-left: 10px; margin-bottom: -10px; }";
                    //    sbOfpHtml = $"<html><head><style type=\"text/css\">{style}</style></head><body>{sbOfpHtml}</body></html>";
                    //}

                    // Remove comments
                    while (sbOfpHtml.Contains("<!--") && sbOfpHtml.Contains("-->"))
                    {
                        var start = sbOfpHtml.IndexOf("<!--", StringComparison.InvariantCultureIgnoreCase);
                        var end = sbOfpHtml.IndexOf("-->", start, StringComparison.InvariantCultureIgnoreCase);
                        if (start != -1 && end != -1)
                        {
                            sbOfpHtml = sbOfpHtml.Substring(0, start) + sbOfpHtml.Substring(end + 3);
                        }
                    }

                    // Replace page breaks
                    sbOfpHtml = sbOfpHtml.Replace("<h2 style=\"page-break-after: always;\"> </h2>", "\r\n\r\n");

                    // Remove html tags
                    while (sbOfpHtml.Contains("<") && sbOfpHtml.Contains(">"))
                    {
                        var start = sbOfpHtml.IndexOf("<", StringComparison.InvariantCultureIgnoreCase);
                        var end = sbOfpHtml.IndexOf(">", start, StringComparison.InvariantCultureIgnoreCase);
                        if (start != -1 && end != -1)
                        {
                            // Are we removing an image?
                            if (sbOfpHtml.Substring(start, 4) == "<img")
                            {
                                sbOfpHtml = sbOfpHtml.Substring(0, start) + "---Image removed---" + sbOfpHtml.Substring(end + 1);
                            }
                            else
                            {
                                sbOfpHtml = sbOfpHtml.Substring(0, start) + sbOfpHtml.Substring(end + 1);
                            }
                        }
                    }

                    this.DownloadSimBriefCommand.ReportProgress(() => this.OfpHtml = sbOfpHtml);
                }

                // Navlog fixes
                var originLat = double.Parse((string)ofp.Element("origin").Element("pos_lat"));
                var originLon = double.Parse((string)ofp.Element("origin").Element("pos_long"));
                this.navlogFixes.Clear();
                var fixNumber = 0;
                this.navlogFixes.Add(
                    new FlightNavlogFix
                    {
                        FlightID = this.ID,
                        FixNumber = fixNumber++,
                        Type = "apt",
                        Ident = this.OriginICAO,
                        Latitude = originLat,
                        Longitude = originLon
                    });

                this.DownloadSimBriefCommand.ReportProgress(
                    () =>
                    {
                        this.SimbriefRouteLocations.Clear();
                        this.SimbriefWaypointMarkers.Clear();

                        this.SimbriefRouteLocations.Add(new Location(originLat, originLon));
                    });
                var fixes = ofp.Element("navlog").Elements("fix");
                foreach (var fix in fixes)
                {
                    var ident = (string)fix.Element("ident");
                    var latitude = double.Parse((string)fix.Element("pos_lat"));
                    var longitude = double.Parse((string)fix.Element("pos_long"));
                    var type = (string)fix.Element("type");

                    this.navlogFixes.Add(
                        new FlightNavlogFix
                        {
                            FlightID = this.ID,
                            FixNumber = fixNumber++,
                            Type = type,
                            Ident = ident,
                            Latitude = latitude,
                            Longitude = longitude
                        });

                    if (this.SimbriefRouteLocations != null && this.SimbriefWaypointMarkers != null)
                    {
                        this.DownloadSimBriefCommand.ReportProgress(
                            () =>
                            {
                                this.SimbriefRouteLocations.Add(new Location(latitude, longitude));
                                if (type != "apt")
                                {
                                    var newMarker = new SimbriefWaypointMarker(latitude, longitude, ident, type);
                                    this.SimbriefWaypointMarkers.Add(newMarker);
                                }
                            });
                    }
                }
            }
            catch (WebException ex)
            {
                Debug.WriteLine("Web error received from simbrief api: " + ex);
                var responseStream = ex.Response.GetResponseStream();
                if (responseStream != null)
                {
                    var responseString = string.Empty;
                    var reader = new StreamReader(responseStream, Encoding.UTF8);
                    var buffer = new char[1024];
                    var count = reader.Read(buffer, 0, buffer.Length);
                    while (count > 0)
                    {
                        responseString += new string(buffer, 0, count);
                        count = reader.Read(buffer, 0, buffer.Length);
                    }

                    Debug.WriteLine(responseString);
                    if (responseString.Contains("<OFP>"))
                    {
                        var ofp = XElement.Parse(responseString);
                        var status = ofp.Element("fetch")?.Element("status")?.Value;
                        if (!string.IsNullOrWhiteSpace(status))
                        {
                            Debug.WriteLine("Error fetching flight plan from Simbrief: " + status);

                            if (status.ToLowerInvariant().Contains("no flight plan on file for the specified static id"))
                            {
                                this.DownloadSimBrief(true);
                                return;
                            }

                            this.DownloadSimBriefCommand.ReportProgress(
                                () =>
                                {
                                    var notification = new OpenSkyNotification("simBrief OFP", status, MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                                    notification.SetErrorColorStyle();
                                    Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                                });
                            return;
                        }
                    }
                }

                throw;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                this.DownloadSimBriefCommand.ReportProgress(
                    () =>
                    {
                        var notification = new OpenSkyNotification(new ErrorDetails { DetailedMessage = ex.Message, Exception = ex }, "simBrief OFP", "Error downloading simBrief OFP!", ExtendedMessageBoxImage.Error, 30);
                        Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                    });
            }
            finally
            {
                this.LoadingText = null;
            }
        }
    }
}
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FlightPlanViewModel.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Pages.Models
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Windows;
    using System.Xml.Linq;

    using OpenSky.Client.MVVM;
    using OpenSky.Client.Tools;

    using OpenSkyApi;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Flight plan view model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 28/10/2021.
    /// </remarks>
    /// <seealso cref="T:OpenSky.Client.MVVM.ViewModel"/>
    /// -------------------------------------------------------------------------------------------------
    public class FlightPlanViewModel : ViewModel
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The user's airline.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private UserAirline airline;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The alternate icao.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string alternateICAO;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The delete visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Visibility deleteVisibility = Visibility.Visible;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The departure hour.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private int departureHour;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The departure minute.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private int departureMinute;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The destination icao.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string destinationICAO;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The discard visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Visibility discardVisibility = Visibility.Collapsed;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Identifier for the dispatcher.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string dispatcherID;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The dispatcher remarks.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string dispatcherRemarks;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The flight number (1-9999).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private int flightNumber = 1;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The fuel gallons.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private double? fuelGallons;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The flight plan ID.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Guid id;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// True if is airline flight, false if not.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private bool isAirlineFlight;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// True if is new flight plan, false if not.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private bool isNewFlightPlan;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The loading text.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string loadingText;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The origin icao.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string originICAO;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The planned departure time.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private DateTime plannedDepartureTime;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The selected aircraft.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Aircraft selectedAircraft;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The UTC offset.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private double utcOffset;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The route.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string route;

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
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The alternate route.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string alternateRoute;

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
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="FlightPlanViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 29/10/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public FlightPlanViewModel()
        {
            // Initialize data structures
            this.Aircraft = new ObservableCollection<Aircraft>();
            this.UtcOffsets = new SortedSet<double>();

            // Populate UTC offsets from time zones
            foreach (var timeZone in TimeZoneInfo.GetSystemTimeZones())
            {
                this.UtcOffsets.Add(timeZone.BaseUtcOffset.TotalHours);
            }

            // Create commands
            this.RefreshAircraftCommand = new AsynchronousCommand(this.RefreshAircraft);
            this.ClearAircraftCommand = new Command(this.ClearAircraft);
            this.SaveCommand = new AsynchronousCommand(this.SaveFlightPlan);
            this.DiscardCommand = new Command(this.DiscardFlightPlan);
            this.DeleteCommand = new AsynchronousCommand(this.DeleteFlightPlan);
            this.RefreshAirlineCommand = new AsynchronousCommand(this.RefreshAirline);
            this.CreateSimBriefCommand = new Command(this.CreateSimBrief);
            this.DownloadSimBriefCommand = new AsynchronousCommand(this.DownloadSimBrief);

            // Fire off initial commands
            this.RefreshAirlineCommand.DoExecute(null);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Downloads the created simBrief OFP.
        /// </summary>
        /// <remarks>
        /// sushi.at, 03/11/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void DownloadSimBrief()
        {
            if (string.IsNullOrEmpty(UserSessionService.Instance.LinkedAccounts?.SimbriefUsername))
            {
                this.DownloadSimBriefCommand.ReportProgress(() => ModernWpf.MessageBox.Show("To use this feature, please configure your simBrief username in the settings!", "simBrief OFP", MessageBoxButton.OK, MessageBoxImage.Warning));
                return;
            }

            this.LoadingText = "Downloading simBrief OFP...";
            MessageBoxResult? answer;
            try
            {
                var ofpFetchUrl = $"https://www.simbrief.com/api/xml.fetcher.php?user={WebUtility.UrlEncode(UserSessionService.Instance.LinkedAccounts?.SimbriefUsername)}&static_id={this.ID}";
                Debug.WriteLine(ofpFetchUrl);

                using var client = new WebClient();
                var xml = client.DownloadString(ofpFetchUrl);
                var ofp = XElement.Parse(xml);

                // Origin and Destination
                var sbOriginICAO = (string)ofp.Element("origin")?.Element("icao_code");
                var sbDestinationICAO = (string)ofp.Element("destination")?.Element("icao_code");
                if (!string.IsNullOrEmpty(sbOriginICAO) && !string.IsNullOrEmpty(sbDestinationICAO) && (!sbOriginICAO.Equals(this.OriginICAO, StringComparison.InvariantCultureIgnoreCase) || !sbDestinationICAO.Equals(this.DestinationICAO, StringComparison.InvariantCultureIgnoreCase)))
                {
                    answer = MessageBoxResult.None;
                    this.DownloadSimBriefCommand.ReportProgress(
                        () =>
                        {
                            answer = ModernWpf.MessageBox.Show("Origin or destination ICAO don't match this flight plan. Do you want to use the values from simBrief?", "simBrief OFP", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        }, true);
                    if (answer == MessageBoxResult.Yes)
                    {
                        this.OriginICAO = sbOriginICAO;
                        this.DestinationICAO = sbDestinationICAO;
                    }
                }

                // Alternate
                var sbAlternateICAO = (string)ofp.Element("alternate")?.Element("icao_code");
                if (!string.IsNullOrEmpty(sbAlternateICAO))
                {
                    if (string.IsNullOrEmpty(this.AlternateICAO))
                    {
                        this.AlternateICAO = sbAlternateICAO;
                    }
                    else if (!sbAlternateICAO.Equals(this.AlternateICAO, StringComparison.InvariantCultureIgnoreCase))
                    {
                        answer = MessageBoxResult.None;
                        this.DownloadSimBriefCommand.ReportProgress(
                            () =>
                            {
                                answer = ModernWpf.MessageBox.Show("Alternate ICAO don't match this flight plan. Do you want to use the values from simBrief?", "simBrief OFP", MessageBoxButton.YesNo, MessageBoxImage.Question);
                            }, true);
                        if (answer == MessageBoxResult.Yes)
                        {
                            this.AlternateICAO = sbAlternateICAO;
                        }
                    }
                }

                // Fuel

                // ZFW sanity check?

                // Route
                var sbRoute = (string)ofp.Element("general")?.Element("route");
                if (!string.IsNullOrEmpty(sbRoute))
                {
                    if (string.IsNullOrEmpty(this.Route))
                    {
                        this.Route = sbRoute;
                    }
                    else if (!sbRoute.Equals(this.Route, StringComparison.InvariantCultureIgnoreCase))
                    {
                        answer = MessageBoxResult.None;
                        this.DownloadSimBriefCommand.ReportProgress(
                            () =>
                            {
                                answer = ModernWpf.MessageBox.Show("Route doesn't match this flight plan. Do you want to use the values from simBrief?", "simBrief OFP", MessageBoxButton.YesNo, MessageBoxImage.Question);
                            }, true);
                        if (answer == MessageBoxResult.Yes)
                        {
                            this.Route = sbRoute;
                        }
                    }
                }

                // Alternate route
                var sbAlternateRoute = (string)ofp.Element("alternate")?.Element("route");
                if (!string.IsNullOrEmpty(sbAlternateRoute))
                {
                    if (string.IsNullOrEmpty(this.AlternateRoute))
                    {
                        this.AlternateRoute = sbAlternateRoute;
                    }
                    else if (!sbAlternateRoute.Equals(this.AlternateRoute, StringComparison.InvariantCultureIgnoreCase))
                    {
                        answer = MessageBoxResult.None;
                        this.DownloadSimBriefCommand.ReportProgress(
                            () =>
                            {
                                answer = ModernWpf.MessageBox.Show("Alternate route doesn't match this flight plan. Do you want to use the values from simBrief?", "simBrief OFP", MessageBoxButton.YesNo, MessageBoxImage.Question);
                            }, true);
                        if (answer == MessageBoxResult.Yes)
                        {
                            this.alternateRoute = sbAlternateRoute;
                        }
                    }
                }

                // OFP html
                var sbOfpHtml = (string)ofp.Element("text")?.Element("plan_html");
                if (!string.IsNullOrEmpty(sbOfpHtml))
                {
                    if (!sbOfpHtml.StartsWith("<html>"))
                    {
                        sbOfpHtml = $"<html><body>{sbOfpHtml}</body></html>";
                    }

                    this.DownloadSimBriefCommand.ReportProgress(()=> this.OfpHtml = sbOfpHtml); 
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
                            this.DownloadSimBriefCommand.ReportProgress(() => ModernWpf.MessageBox.Show(status, "simBrief OFP", MessageBoxButton.OK, MessageBoxImage.Error));
                            return;
                        }
                    }
                }

                throw;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                this.DownloadSimBriefCommand.ReportProgress(() => ModernWpf.MessageBox.Show($"Error downloading simBrief OFP!\r\n{ex.Message}", "simBrief OFP", MessageBoxButton.OK, MessageBoxImage.Error));
            }
            finally
            {
                this.LoadingText = null;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The simBrief OFP HTML.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string ofpHtml;

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
                ModernWpf.MessageBox.Show("To use this feature, please configure your simBrief username in the settings!", "simBrief OFP", MessageBoxButton.OK, MessageBoxImage.Warning);
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

            if (!string.IsNullOrEmpty(this.AlternateICAO))
            {
                url += $"altn={this.AlternateICAO}&";
                url += $"altn_1_id={this.AlternateICAO}&";
            }

            url += $"date={this.PlannedDepartureTime:ddMMMyy}&";
            url += $"deph={this.DepartureHour:00}&";
            url += $"depm={this.DepartureMinute:00}&";

            // todo add route if we already have one?

            if (this.SelectedAircraft != null)
            {
                url += $"reg={this.SelectedAircraft.Registry}&";
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

            ModernWpf.MessageBox.Show(
                "If you are planning to use the fuel numbers from the simBrief OFP, please make sure you correctly set the passengers and cargo after selecting your aircraft type.",
                "simBrief OFP",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the create simulation brief command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command CreateSimBriefCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the refresh airline command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand RefreshAirlineCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Occurs when the view model wants to close the page.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public event EventHandler<object> ClosePage;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the aircraft list.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<Aircraft> Aircraft { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the user's airline.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public UserAirline Airline
        {
            get => this.airline;

            set
            {
                if (Equals(this.airline, value))
                {
                    return;
                }

                this.airline = value;
                this.NotifyPropertyChanged();
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
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the clear aircraft command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command ClearAircraftCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the delete command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand DeleteCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the delete visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Visibility DeleteVisibility
        {
            get => this.deleteVisibility;

            set
            {
                if (Equals(this.deleteVisibility, value))
                {
                    return;
                }

                this.deleteVisibility = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the departure hour.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Range(0, 23)]
        public int DepartureHour
        {
            get => this.departureHour;

            set
            {
                if (Equals(this.departureHour, value))
                {
                    return;
                }

                this.departureHour = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the departure minute.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Range(0, 59)]
        public int DepartureMinute
        {
            get => this.departureMinute;

            set
            {
                if (Equals(this.departureMinute, value))
                {
                    return;
                }

                this.departureMinute = value;
                this.NotifyPropertyChanged();
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
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the discard command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command DiscardCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the discard visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Visibility DiscardVisibility
        {
            get => this.discardVisibility;

            set
            {
                if (Equals(this.discardVisibility, value))
                {
                    return;
                }

                this.discardVisibility = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the identifier of the dispatcher.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string DispatcherID
        {
            get => this.dispatcherID;

            set
            {
                if (Equals(this.dispatcherID, value))
                {
                    return;
                }

                this.dispatcherID = value;
                this.NotifyPropertyChanged();
            }
        }

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
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the flight number  (1-9999).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Range(1, 9999)]
        public int FlightNumber
        {
            get => this.flightNumber;

            set
            {
                if (Equals(this.flightNumber, value))
                {
                    return;
                }

                this.flightNumber = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the fuel gallons.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double? FuelGallons
        {
            get => this.fuelGallons;

            set
            {
                if (Equals(this.fuelGallons, value))
                {
                    return;
                }

                if (value.HasValue && value > (this.SelectedAircraft?.Type.FuelTotalCapacity ?? 0))
                {
                    value = this.SelectedAircraft?.Type.FuelTotalCapacity ?? 0;
                }

                this.fuelGallons = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged(nameof(this.FuelWeight));
                this.NotifyPropertyChanged(nameof(this.GrossWeight));
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the fuel weight in lbs.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double FuelWeight
        {
            get
            {
                // TODO this should come from aircraft type?
                return (this.FuelGallons ?? 0) * 6;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the gross weight.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double GrossWeight => this.ZeroFuelWeight + this.FuelWeight;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the flight plan ID.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Guid ID
        {
            get => this.id;

            set
            {
                if (Equals(this.id, value))
                {
                    return;
                }

                this.id = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether this plan is for an airline flight or not.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool IsAirlineFlight
        {
            get => this.isAirlineFlight;

            set
            {
                if (Equals(this.isAirlineFlight, value))
                {
                    return;
                }

                this.isAirlineFlight = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether this object is new flight plan.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool IsNewFlightPlan
        {
            get => this.isNewFlightPlan;

            set
            {
                if (Equals(this.isNewFlightPlan, value))
                {
                    return;
                }

                this.isNewFlightPlan = value;
                this.NotifyPropertyChanged();
                this.DiscardVisibility = value ? Visibility.Visible : Visibility.Collapsed;
                this.DeleteVisibility = value ? Visibility.Collapsed : Visibility.Visible;
            }
        }

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
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the payload weight in lbs.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double PayloadWeight
        {
            get
            {
                // TODO Calculate the payload weight
                return 0;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the planned departure time.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public DateTime PlannedDepartureTime
        {
            get => this.plannedDepartureTime;

            set
            {
                if (Equals(this.plannedDepartureTime, value))
                {
                    return;
                }

                this.plannedDepartureTime = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the refresh aircraft command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand RefreshAircraftCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the save command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand SaveCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the selected aircraft.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Aircraft SelectedAircraft
        {
            get => this.selectedAircraft;

            set
            {
                if (Equals(this.selectedAircraft, value))
                {
                    return;
                }

                this.selectedAircraft = value;
                if (value != null)
                {
                    this.FuelGallons = value.Fuel;
                }
                else
                {
                    this.FuelGallons = null;
                }

                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged(nameof(this.ZeroFuelWeight));
                this.NotifyPropertyChanged(nameof(this.GrossWeight));
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the UTC offset.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double UtcOffset
        {
            get => this.utcOffset;

            set
            {
                if (Equals(this.utcOffset, value))
                {
                    return;
                }

                this.utcOffset = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the UTC offsets.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public SortedSet<double> UtcOffsets { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the zero fuel weight.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double ZeroFuelWeight => (this.SelectedAircraft?.Type.EmptyWeight ?? 0) + this.PayloadWeight;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Loads an existing flight plan.
        /// </summary>
        /// <remarks>
        /// sushi.at, 28/10/2021.
        /// </remarks>
        /// <param name="flightPlan">
        /// The flight plan.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public void LoadFlightPlan(FlightPlan flightPlan)
        {
            this.ID = flightPlan.Id;
            this.FlightNumber = flightPlan.FlightNumber;
            this.OriginICAO = flightPlan.OriginICAO;
            this.DestinationICAO = flightPlan.DestinationICAO;
            this.AlternateICAO = flightPlan.AlternateICAO;
            if (!string.IsNullOrEmpty(flightPlan.Aircraft?.Registry))
            {
                this.SelectedAircraft = flightPlan.Aircraft;
            }

            this.DispatcherID = flightPlan.DispatcherID;
            this.DispatcherRemarks = flightPlan.DispatcherRemarks;
            this.FuelGallons = flightPlan.FuelGallons;
            this.IsAirlineFlight = flightPlan.IsAirlineFlight;
            this.PlannedDepartureTime = flightPlan.PlannedDepartureTime.UtcDateTime;
            this.DepartureHour = flightPlan.PlannedDepartureTime.UtcDateTime.Hour;
            this.DepartureMinute = flightPlan.PlannedDepartureTime.UtcDateTime.Minute;
            this.UtcOffset = flightPlan.UtcOffset;
            this.Route = flightPlan.Route;
            this.AlternateRoute = flightPlan.AlternateRoute;
            this.OfpHtml = flightPlan.OfpHtml;

            this.IsNewFlightPlan = flightPlan.IsNewFlightPlan;

            this.RefreshAircraftCommand.DoExecute(null);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Clears the selected aircraft.
        /// </summary>
        /// <remarks>
        /// sushi.at, 31/10/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void ClearAircraft()
        {
            this.SelectedAircraft = null;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Deletes the flight plan.
        /// </summary>
        /// <remarks>
        /// sushi.at, 31/10/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void DeleteFlightPlan()
        {
            MessageBoxResult? answer = null;
            this.DeleteCommand.ReportProgress(
                () => { answer = ModernWpf.MessageBox.Show($"Are you sure you want to delete this flight plan?", "Delete flight plan?", MessageBoxButton.YesNo, MessageBoxImage.Question); },
                true);
            if (answer is not MessageBoxResult.Yes)
            {
                return;
            }

            this.LoadingText = "Deleting flight plan...";
            try
            {
                var result = OpenSkyService.Instance.DeleteFlightPlanAsync(this.ID).Result;
                if (!result.IsError)
                {
                    this.DeleteCommand.ReportProgress(() => this.ClosePage?.Invoke(this, null));
                }
                else
                {
                    this.DeleteCommand.ReportProgress(
                        () =>
                        {
                            Debug.WriteLine("Error deleting flight plan: " + result.Message);
                            if (!string.IsNullOrEmpty(result.ErrorDetails))
                            {
                                Debug.WriteLine(result.ErrorDetails);
                            }

                            ModernWpf.MessageBox.Show(result.Message, "Error deleting flight plan", MessageBoxButton.OK, MessageBoxImage.Error);
                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.DeleteCommand, "Error deleting flight plan");
            }
            finally
            {
                this.LoadingText = null;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Discards the flight plan (not yet saved to the server, so we only need to close the tab).
        /// </summary>
        /// <remarks>
        /// sushi.at, 31/10/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void DiscardFlightPlan()
        {
            this.ClosePage?.Invoke(this, null);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Refreshes the list of available aircraft.
        /// </summary>
        /// <remarks>
        /// sushi.at, 31/10/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void RefreshAircraft()
        {
            this.LoadingText = "Refreshing aircraft...";
            try
            {
                var currentSelection = this.SelectedAircraft;
                var result = OpenSkyService.Instance.GetMyAircraftAsync().Result;
                if (!result.IsError)
                {
                    this.RefreshAircraftCommand.ReportProgress(
                        () =>
                        {
                            this.Aircraft.Clear();
                            foreach (var aircraft in result.Data)
                            {
                                this.Aircraft.Add(aircraft);
                            }

                            if (currentSelection != null && this.Aircraft.Contains(currentSelection))
                            {
                                this.SelectedAircraft = currentSelection;
                            }
                        });
                }
                else
                {
                    this.RefreshAircraftCommand.ReportProgress(
                        () =>
                        {
                            Debug.WriteLine("Error refreshing aircraft: " + result.Message);
                            if (!string.IsNullOrEmpty(result.ErrorDetails))
                            {
                                Debug.WriteLine(result.ErrorDetails);
                            }

                            ModernWpf.MessageBox.Show(result.Message, "Error refreshing aircraft", MessageBoxButton.OK, MessageBoxImage.Error);
                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.RefreshAircraftCommand, "Error refreshing aircraft");
            }
            finally
            {
                this.LoadingText = null;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Refreshes the user's airline.
        /// </summary>
        /// <remarks>
        /// sushi.at, 31/10/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void RefreshAirline()
        {
            this.LoadingText = "Refreshing airline info...";
            try
            {
                var result = OpenSkyService.Instance.GetAirlineAsync().Result;
                if (!result.IsError)
                {
                    if (!string.IsNullOrEmpty(result.Data?.Icao))
                    {
                        this.Airline = result.Data;
                    }
                }
                else
                {
                    this.RefreshAirlineCommand.ReportProgress(
                        () =>
                        {
                            Debug.WriteLine("Error refreshing airline info: " + result.Message);
                            if (!string.IsNullOrEmpty(result.ErrorDetails))
                            {
                                Debug.WriteLine(result.ErrorDetails);
                            }

                            ModernWpf.MessageBox.Show(result.Message, "Error refreshing airline info", MessageBoxButton.OK, MessageBoxImage.Error);
                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.RefreshAirlineCommand, "Error refreshing airline info");
            }
            finally
            {
                this.LoadingText = null;
            }
        }


        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Saves the flight plan.
        /// </summary>
        /// <remarks>
        /// sushi.at, 31/10/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void SaveFlightPlan()
        {
            this.LoadingText = "Saving flight plan...";
            try
            {
                var flightPlan = new FlightPlan
                {
                    Id = this.ID,
                    FlightNumber = this.FlightNumber,
                    OriginICAO = this.OriginICAO,
                    DestinationICAO = this.DestinationICAO,
                    AlternateICAO = this.AlternateICAO,
                    Aircraft = this.SelectedAircraft,
                    DispatcherRemarks = this.DispatcherRemarks,
                    FuelGallons = this.FuelGallons,
                    IsAirlineFlight = this.IsAirlineFlight,
                    PlannedDepartureTime = this.PlannedDepartureTime.Date.AddHours(this.DepartureHour).AddMinutes(this.DepartureMinute),
                    UtcOffset = this.UtcOffset,
                    Route = this.Route,
                    AlternateRoute = this.AlternateRoute,
                    OfpHtml = this.OfpHtml
                };

                var result = OpenSkyService.Instance.SaveFlightPlanAsync(flightPlan).Result;
                if (!result.IsError)
                {
                    this.IsNewFlightPlan = false;
                }
                else
                {
                    this.SaveCommand.ReportProgress(
                        () =>
                        {
                            Debug.WriteLine("Error saving flight plan: " + result.Message);
                            if (!string.IsNullOrEmpty(result.ErrorDetails))
                            {
                                Debug.WriteLine(result.ErrorDetails);
                            }

                            ModernWpf.MessageBox.Show(result.Message, "Error saving flight plan", MessageBoxButton.OK, MessageBoxImage.Error);
                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.SaveCommand, "Error saving flight plan");
            }
            finally
            {
                this.LoadingText = null;
            }
        }
    }
}
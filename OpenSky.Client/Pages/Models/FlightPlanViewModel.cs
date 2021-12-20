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
    using System.Device.Location;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading;
    using System.Windows;
    using System.Windows.Media;
    using System.Xml.Linq;

    using Microsoft.Maps.MapControl.WPF;

    using OpenSky.Client.Controls.Models;
    using OpenSky.Client.MVVM;
    using OpenSky.Client.Tools;
    using OpenSky.Client.Views;
    using OpenSky.Client.Views.Models;

    using OpenSkyApi;

    using TomsToolbox.Essentials;

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
        /// The navlog fixes.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly List<FlightNavlogFix> navlogFixes = new();

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
        /// The alternate route.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string alternateRoute;

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
        /// Are there changes to the settings to be saved?
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private bool isDirty;

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
        /// The simBrief OFP HTML.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string ofpHtml;

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
        /// The route.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string route;

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
        /// The view reference.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Pages.FlightPlan viewReference;

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
            this.TrackingEventMarkers = new ObservableCollection<TrackingEventMarker>();
            this.SimbriefRouteLocations = new LocationCollection();
            this.SimbriefWaypointMarkers = new ObservableCollection<SimbriefWaypointMarker>();
            this.PayloadsOnBoard = new ObservableCollection<PlannablePayload>();
            this.PayloadsAtOrigin = new ObservableCollection<PlannablePayload>();
            this.PayloadsTowardsOrigin = new ObservableCollection<PlannablePayload>();
            this.OtherPayloads = new ObservableCollection<PlannablePayload>();
            this.Payloads = new ObservableCollection<Guid>();
            this.Payloads.CollectionChanged += (_, _) =>
            {
                this.NotifyPropertyChanged(nameof(this.PayloadWeight));
                this.NotifyPropertyChanged(nameof(this.ZeroFuelWeight));
                this.NotifyPropertyChanged(nameof(this.GrossWeight));
                this.IsDirty = true;
            };

            // Populate UTC offsets from time zones
            foreach (var timeZone in TimeZoneInfo.GetSystemTimeZones())
            {
                this.UtcOffsets.Add(timeZone.BaseUtcOffset.TotalHours);
            }

            // Default values
            // todo maybe can use this in the future if we find more performant html rendering control
            //const string style = "body { background-color: #29323c; color: #c2c2c2; margin: -1px; } div { margin-top: 10px; margin-left: 10px; margin-bottom: -10px; }";
            //this.OfpHtml = $"<html><head><style type=\"text/css\">{style}</style></head><body><div style=\"line-height:14px; font-size:13px; height: 1000px;\"><pre>--- None ---</pre></div></body></html>";
            this.OfpHtml = "--- None ---";

            // Create commands
            this.LoadFlightPlanCommand = new AsynchronousCommand(this.LoadFlightPlanInBackground);
            this.RefreshAircraftCommand = new AsynchronousCommand(this.RefreshAircraft);
            this.RefreshPayloadsCommand = new AsynchronousCommand(this.RefreshPayloads);
            this.ClearAircraftCommand = new Command(this.ClearAircraft);
            this.SaveCommand = new AsynchronousCommand(this.SaveFlightPlan, false);
            this.DiscardCommand = new Command(this.DiscardFlightPlan);
            this.DeleteCommand = new AsynchronousCommand(this.DeleteFlightPlan);
            this.RefreshAirlineCommand = new AsynchronousCommand(this.RefreshAirline);
            this.CreateSimBriefCommand = new Command(this.CreateSimBrief);
            this.DownloadSimBriefCommand = new AsynchronousCommand(this.DownloadSimBrief);
            this.StartFlightCommand = new AsynchronousCommand(this.StartFlight);

            // Fire off initial commands
            this.RefreshAirlineCommand.DoExecute(null);
            this.RefreshPayloadsCommand.DoExecute(null);
        }

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
        [CustomValidation(typeof(AirportPackageClientHandler), "ValidateAirportICAO")] // todo validation is called, but the textbox isn't showing it
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
                this.UpdateAirportMarkers();
            }
        }

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
        /// Gets the clear aircraft command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command ClearAircraftCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the create simulation brief command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command CreateSimBriefCommand { get; }

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
                this.IsDirty = true;
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
                this.IsDirty = true;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the destination icao.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [CustomValidation(typeof(AirportPackageClientHandler), "ValidateAirportICAO")] // todo validation is called, but the textbox isn't showing it
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
                this.UpdateAirportMarkers();
                this.IsDirty = true;
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
                this.IsDirty = true;
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
                this.IsDirty = true;
                this.NotifyPropertyChanged(nameof(this.FuelWeight));
                this.NotifyPropertyChanged(nameof(this.GrossWeight));
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the fuel weight in lbs.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double FuelWeight => (this.FuelGallons ?? 0) * (this.SelectedAircraft?.Type.FuelWeightPerGallon ?? 0);

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
                this.IsDirty = true;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether there are changes to the settings to be saved.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool IsDirty
        {
            get => this.isDirty;

            set
            {
                // This property notifies on every change to ensure all views/properties get updated correctly
                // and the user can save the plan in any case.
                this.isDirty = value;
                this.NotifyPropertyChanged();

                if (this.SaveCommand != null)
                {
                    UpdateGUIDelegate setCanExecute = () => this.SaveCommand.CanExecute = value;
                    Application.Current.Dispatcher.BeginInvoke(setCanExecute);
                }
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
        /// Gets the load flight plan command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand LoadFlightPlanCommand { get; }

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
        /// Gets or sets the origin icao.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [CustomValidation(typeof(AirportPackageClientHandler), "ValidateAirportICAO")] // todo validation is called, but the textbox isn't showing it
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
                this.UpdateAirportMarkers();

                UpdateGUIDelegate updateOriginRelated = () =>
                {
                    this.UpdateAircraftDistances();
                    this.UpdatePlannablePayloads();
                };
                Application.Current.Dispatcher.BeginInvoke(updateOriginRelated);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the other payloads.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<PlannablePayload> OtherPayloads { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the payloads planned for the flight.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<Guid> Payloads { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the plannable payloads.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<PlannablePayload> PayloadsAtOrigin { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the payloads towards origin.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<PlannablePayload> PayloadsTowardsOrigin { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the payload weight in lbs.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double PayloadWeight
        {
            get
            {
                var totalPayload = 0.0;
                foreach (var payloadID in this.Payloads)
                {
                    var atOriginPayload = this.PayloadsAtOrigin.SingleOrDefault(p => p.Id == payloadID);
                    if (atOriginPayload != null)
                    {
                        totalPayload += atOriginPayload.Weight;
                    }

                    var towardsOriginPayload = this.PayloadsTowardsOrigin.SingleOrDefault(p => p.Id == payloadID);
                    if (towardsOriginPayload != null)
                    {
                        totalPayload += towardsOriginPayload.Weight;
                    }

                    var otherPayload = this.OtherPayloads.SingleOrDefault(p => p.Id == payloadID);
                    if (otherPayload != null)
                    {
                        totalPayload += otherPayload.Weight;
                    }
                }

                return totalPayload;
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
                this.IsDirty = true;
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
        /// Gets the refresh airline command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand RefreshAirlineCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the refresh payloads command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand RefreshPayloadsCommand { get; }

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
                this.IsDirty = true;
                this.NotifyPropertyChanged(nameof(this.ZeroFuelWeight));
                this.NotifyPropertyChanged(nameof(this.GrossWeight));

                UpdateGUIDelegate updateAircraftRelated = this.UpdatePlannablePayloads;
                Application.Current.Dispatcher.BeginInvoke(updateAircraftRelated);
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
        /// Gets the start flight command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand StartFlightCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the tracking event markers.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<TrackingEventMarker> TrackingEventMarkers { get; }

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
                this.IsDirty = true;
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
        public double ZeroFuelWeight => (this.SelectedAircraft?.Type.EmptyWeight ?? 0) + this.PayloadWeight + this.CrewWeight;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the crew weight in lbs.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double CrewWeight
        {
            get
            {
                if (this.SelectedAircraft != null)
                {
                    var crewWeight = 190.0;
                    if (this.SelectedAircraft.Type.NeedsCoPilot)
                    {
                        crewWeight += 190;
                    }

                    if (this.SelectedAircraft.Type.NeedsFlightEngineer)
                    {
                        crewWeight += 190;
                    }

                    // todo add other non-flight crew later once implemented

                    return crewWeight;
                }

                return 0;
            }
        }

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
            this.LoadFlightPlanCommand.DoExecute(flightPlan);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Sets the view reference for this view model (to determine main window to open new tabs in, in
        /// case the user has multiple open windows)
        /// </summary>
        /// <remarks>
        /// sushi.at, 28/10/2021.
        /// </remarks>
        /// <param name="view">
        /// The view reference.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public void SetViewReference(Pages.FlightPlan view)
        {
            this.viewReference = view;
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

            if (!string.IsNullOrEmpty(this.Route))
            {
                url += $"route={WebUtility.UrlEncode(this.Route)}&";
            }

            if (!string.IsNullOrEmpty(this.AlternateICAO))
            {
                url += $"altn={this.AlternateICAO}&";

                // todo revisit alternate simbrief upload, seems to ignore all the advanced stuff and only accepts the primary
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
                this.DownloadSimBriefCommand.ReportProgress(() => ModernWpf.MessageBox.Show("To use this feature, please configure your simBrief username in the settings!", "simBrief OFP", MessageBoxButton.OK, MessageBoxImage.Warning));
                return;
            }

            this.LoadingText = "Downloading simBrief OFP...";
            MessageBoxResult? answer;
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
                        this.OriginICAO = sbOriginICAO;
                        this.DestinationICAO = sbDestinationICAO;
                    }
                    else
                    {
                        answer = MessageBoxResult.None;
                        this.DownloadSimBriefCommand.ReportProgress(
                            () => { answer = ModernWpf.MessageBox.Show("Origin or destination ICAO don't match this flight plan. Do you want to use the values from simBrief?", "simBrief OFP", MessageBoxButton.YesNo, MessageBoxImage.Question); },
                            true);
                        if (answer == MessageBoxResult.Yes)
                        {
                            this.OriginICAO = sbOriginICAO;
                            this.DestinationICAO = sbDestinationICAO;
                        }
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
                            () => { answer = ModernWpf.MessageBox.Show("Alternate ICAO don't match this flight plan. Do you want to use the values from simBrief?", "simBrief OFP", MessageBoxButton.YesNo, MessageBoxImage.Question); },
                            true);
                        if (answer == MessageBoxResult.Yes)
                        {
                            this.AlternateICAO = sbAlternateICAO;
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
                        answer = MessageBoxResult.None;
                        this.DownloadSimBriefCommand.ReportProgress(
                            () => answer = ModernWpf.MessageBox.Show("simBrief planned fuel is less than the currently selected value, do you want to use it anyway?", "simBrief OFP", MessageBoxButton.YesNo, MessageBoxImage.Question),
                            true);
                        if (answer == MessageBoxResult.Yes)
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
                    this.DownloadSimBriefCommand.ReportProgress(() => ModernWpf.MessageBox.Show("No aircraft selected, can't set planned fuel.", "simBrief OFP", MessageBoxButton.OK, MessageBoxImage.Information), true);
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
                        answer = MessageBoxResult.None;
                        this.DownloadSimBriefCommand.ReportProgress(
                            () => { answer = ModernWpf.MessageBox.Show("Route doesn't match this flight plan. Do you want to use the values from simBrief?", "simBrief OFP", MessageBoxButton.YesNo, MessageBoxImage.Question); },
                            true);
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
                        answer = MessageBoxResult.None;
                        this.DownloadSimBriefCommand.ReportProgress(
                            () => { answer = ModernWpf.MessageBox.Show("Alternate route doesn't match this flight plan. Do you want to use the values from simBrief?", "simBrief OFP", MessageBoxButton.YesNo, MessageBoxImage.Question); },
                            true);
                        if (answer == MessageBoxResult.Yes)
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
        /// Loads flight plan in background.
        /// </summary>
        /// <remarks>
        /// sushi.at, 05/11/2021.
        /// </remarks>
        /// <param name="parameter">
        /// The parameter (must be flight plan).
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void LoadFlightPlanInBackground(object parameter)
        {
            try
            {
                this.LoadingText = "Loading flight plan...";
                if (parameter is FlightPlan flightPlan)
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
                    this.Payloads.Clear();
                    if (flightPlan.Payloads != null)
                    {
                        this.Payloads.AddRange(flightPlan.Payloads.Select(p => p.PayloadID));
                    }

                    this.navlogFixes.Clear();
                    if (flightPlan.NavlogFixes != null)
                    {
                        this.navlogFixes.AddRange(flightPlan.NavlogFixes);
                    }

                    if (!string.IsNullOrEmpty(flightPlan.OfpHtml))
                    {
                        this.OfpHtml = flightPlan.OfpHtml;
                    }

                    this.IsNewFlightPlan = flightPlan.IsNewFlightPlan;

                    this.LoadFlightPlanCommand.ReportProgress(
                        () =>
                        {
                            foreach (var fix in this.navlogFixes)
                            {
                                this.SimbriefRouteLocations.Add(new Location(fix.Latitude, fix.Longitude));
                                if (fix.Type != "apt")
                                {
                                    var newMarker = new SimbriefWaypointMarker(fix.Latitude, fix.Longitude, fix.Ident, fix.Type);
                                    this.SimbriefWaypointMarkers.Add(newMarker);
                                }
                            }
                        });

                    this.LoadFlightPlanCommand.ReportProgress(() => this.RefreshAircraftCommand.DoExecute(null));
                }

                this.IsDirty = this.IsNewFlightPlan;
            }
            finally
            {
                this.LoadingText = null;
            }
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
                var currentFuel = this.FuelGallons;
                var result = OpenSkyService.Instance.GetMyAircraftAsync().Result;
                if (!result.IsError)
                {
                    this.RefreshAircraftCommand.ReportProgress(
                        () =>
                        {
                            this.Aircraft.Clear();
                            this.Aircraft.AddRange(result.Data.OrderBy(a => a.Registry));
                            this.UpdateAircraftDistances();

                            if (currentSelection != null && this.Aircraft.Contains(currentSelection))
                            {
                                this.SelectedAircraft = currentSelection;
                                this.FuelGallons = currentFuel;
                                this.IsDirty = false;
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
        /// Refreshes the list of plannable payloads.
        /// </summary>
        /// <remarks>
        /// sushi.at, 19/12/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void RefreshPayloads()
        {
            this.LoadingText = "Refreshing payloads...";
            try
            {
                var result = OpenSkyService.Instance.GetPlannablePayloadsAsync().Result;
                if (!result.IsError)
                {
                    this.RefreshPayloadsCommand.ReportProgress(
                        () =>
                        {
                            this.OtherPayloads.Clear();
                            this.OtherPayloads.AddRange(result.Data);
                            this.UpdatePlannablePayloads();
                        });
                }
                else
                {
                    this.RefreshPayloadsCommand.ReportProgress(
                        () =>
                        {
                            Debug.WriteLine("Error refreshing payloads: " + result.Message);
                            if (!string.IsNullOrEmpty(result.ErrorDetails))
                            {
                                Debug.WriteLine(result.ErrorDetails);
                            }

                            ModernWpf.MessageBox.Show(result.Message, "Error refreshing payloads", MessageBoxButton.OK, MessageBoxImage.Error);
                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.RefreshPayloadsCommand, "Error refreshing payloads");
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
                    OfpHtml = this.OfpHtml,
                    NavlogFixes = this.navlogFixes,
                    Payloads = new List<FlightPayload>()
                };

                foreach (var payloadID in this.Payloads)
                {
                    flightPlan.Payloads.Add(new FlightPayload { FlightID = this.ID, PayloadID = payloadID });
                }

                var result = OpenSkyService.Instance.SaveFlightPlanAsync(flightPlan).Result;
                if (!result.IsError)
                {
                    this.IsNewFlightPlan = false;
                    this.SaveCommand.ReportProgress(() => this.IsDirty = false);
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

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Start flying the flight plan.
        /// </summary>
        /// <remarks>
        /// sushi.at, 20/11/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void StartFlight()
        {
            if (this.IsDirty)
            {
                MessageBoxResult? answer = MessageBoxResult.None;
                this.StartFlightCommand.ReportProgress(
                    () => { answer = ModernWpf.MessageBox.Show("You first have to save your changes, do you want to save the flight plan now?", "Save flight plan?", MessageBoxButton.YesNo); },
                    true);

                if (answer != MessageBoxResult.Yes)
                {
                    return;
                }

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
                        OfpHtml = this.OfpHtml,
                        NavlogFixes = this.navlogFixes
                    };

                    var result = OpenSkyService.Instance.SaveFlightPlanAsync(flightPlan).Result;
                    if (!result.IsError)
                    {
                        this.IsNewFlightPlan = false;
                        this.SaveCommand.ReportProgress(() => this.IsDirty = false);
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
                    return;
                }
                finally
                {
                    this.LoadingText = null;
                }
            }

            try
            {
                this.LoadingText = $"Starting flight {this.FlightNumber}...";
                var result = OpenSkyService.Instance.StartFlightAsync(this.ID).Result;
                if (!result.IsError)
                {
                    this.StartFlightCommand.ReportProgress(
                        () =>
                        {
                            ModernWpf.MessageBox.Show(result.Message, "Start flight", MessageBoxButton.OK, MessageBoxImage.Information);
                            this.ClosePage?.Invoke(this, null);
                        });
                }
                else
                {
                    if (result.Data == "AircraftNotAtOrigin")
                    {
                        MessageBoxResult? answer = MessageBoxResult.None;
                        this.StartFlightCommand.ReportProgress(
                            () =>
                            {
                                answer = ModernWpf.MessageBox.Show(
                                    "The selected aircraft is not at the selected origin airport. Do you want to create another flight plan for the positioning flight?",
                                    "Aircraft not at Origin",
                                    MessageBoxButton.YesNo);
                            },
                            true);
                        if (answer == MessageBoxResult.Yes)
                        {
                            this.StartFlightCommand.ReportProgress(
                                () =>
                                {
                                    var posFlightNumber = new Random().Next(1, 9999);
                                    var navMenuItem = new NavMenuItem
                                    {
                                        Icon = "/Resources/plan16.png", PageType = typeof(Pages.FlightPlan), Name = $"New flight plan {posFlightNumber}",
                                        Parameter = new FlightPlan
                                        {
                                            Id = Guid.NewGuid(), FlightNumber = posFlightNumber, PlannedDepartureTime = DateTime.UtcNow.AddMinutes(30).RoundUp(TimeSpan.FromMinutes(5)), IsNewFlightPlan = true,
                                            OriginICAO = this.SelectedAircraft.AirportICAO, DestinationICAO = this.OriginICAO, Aircraft = this.SelectedAircraft, FuelGallons = this.SelectedAircraft.Fuel,
                                            DispatcherRemarks = $"REPOSITIONING FLIGHT FOR {this.SelectedAircraft.Registry} FLIGHT #{this.FlightNumber}",
                                            UtcOffset = Properties.Settings.Default.DefaultUTCOffset
                                        }
                                    };
                                    Main.ActivateNavMenuItemInSameViewAs(this.viewReference, navMenuItem);
                                });
                        }
                    }
                    else
                    {
                        this.StartFlightCommand.ReportProgress(
                            () =>
                            {
                                Debug.WriteLine("Error starting flight: " + result.Message);
                                if (!string.IsNullOrEmpty(result.ErrorDetails))
                                {
                                    Debug.WriteLine(result.ErrorDetails);
                                }

                                ModernWpf.MessageBox.Show(result.Message, "Error starting flight", MessageBoxButton.OK, MessageBoxImage.Error);
                            });
                    }
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.StartFlightCommand, "Error starting flight");
            }
            finally
            {
                this.LoadingText = null;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Update aircraft distances when origin airport changes.
        /// </summary>
        /// <remarks>
        /// sushi.at, 18/12/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void UpdateAircraftDistances()
        {
            var currentSelection = this.SelectedAircraft;
            var currentAircraft = new List<Aircraft>(this.Aircraft.Where(a => a.Registry != "----"));
            this.Aircraft.Clear();
            if (string.IsNullOrEmpty(this.OriginICAO))
            {
                foreach (var aircraft in currentAircraft.OrderBy(a => a.Registry))
                {
                    aircraft.Distance = 0;
                    this.Aircraft.Add(aircraft);
                }
            }
            else
            {
                // Origin is already set, put the aircraft at that airport first
                var airportPackage = AirportPackageClientHandler.GetPackage();
                var origin = airportPackage?.Airports.SingleOrDefault(a => a.ICAO == this.OriginICAO);
                if (origin != null)
                {
                    this.Aircraft.Add(new Aircraft { Registry = "----", Type = new AircraftType { Name = $" Aircraft at {this.OriginICAO} ----" } });
                }

                foreach (var aircraft in currentAircraft.Where(a => a.AirportICAO.Equals(this.OriginICAO, StringComparison.InvariantCultureIgnoreCase)).OrderBy(a => a.Registry))
                {
                    aircraft.Distance = 0;
                    this.Aircraft.Add(aircraft);
                }

                if (origin != null)
                {
                    this.Aircraft.Add(new Aircraft { Registry = "----", Type = new AircraftType { Name = " Aircraft at other airports ----" } });
                }

                var atOtherAirports = new List<Aircraft>();
                foreach (var aircraft in currentAircraft.Where(a => !a.AirportICAO.Equals(this.OriginICAO, StringComparison.InvariantCultureIgnoreCase)))
                {
                    aircraft.Distance = 0;
                    if (origin != null)
                    {
                        var aircraftAirport = airportPackage.Airports.SingleOrDefault(a => a.ICAO == aircraft.AirportICAO);
                        if (aircraftAirport != null)
                        {
                            aircraft.Distance = (int)(new GeoCoordinate(origin.Latitude, origin.Longitude).GetDistanceTo(new GeoCoordinate(aircraftAirport.Latitude, aircraftAirport.Longitude)) / 1852.0);
                        }
                    }

                    atOtherAirports.Add(aircraft);
                }

                this.Aircraft.AddRange(atOtherAirports.OrderBy(a => a.Distance).ThenBy(a => a.Registry));
            }

            if (currentSelection != null && this.Aircraft.Contains(currentSelection))
            {
                this.SelectedAircraft = currentSelection;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Update map airport markers.
        /// </summary>
        /// <remarks>
        /// sushi.at, 08/11/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void UpdateAirportMarkers()
        {
            new Thread(
                    () =>
                    {
                        try
                        {
                            var localAirportPackage = AirportPackageClientHandler.GetPackage();
                            if (this.TrackingEventMarkers.Count > 0)
                            {
                                UpdateGUIDelegate clearExisting = () => this.TrackingEventMarkers.Clear();
                                Application.Current.Dispatcher.Invoke(clearExisting);
                            }

                            // Origin
                            if (!string.IsNullOrEmpty(this.OriginICAO))
                            {
                                var airport = localAirportPackage?.Airports.SingleOrDefault(a => a.ICAO == this.OriginICAO);
                                if (airport != null)
                                {
                                    UpdateGUIDelegate addOrigin = () =>
                                    {
                                        var originMarker = new TrackingEventMarker(new GeoCoordinate(airport.Latitude, airport.Longitude), this.OriginICAO, OpenSkyColors.OpenSkyTeal, Colors.White);
                                        this.TrackingEventMarkers.Add(originMarker);

                                        var originDetailMarker = new TrackingEventMarker(airport, OpenSkyColors.OpenSkyTeal, Colors.White);
                                        this.TrackingEventMarkers.Add(originDetailMarker);

                                        foreach (var runway in airport.Runways)
                                        {
                                            var runwayMarker = new TrackingEventMarker(runway);
                                            this.TrackingEventMarkers.Add(runwayMarker);
                                        }
                                    };
                                    Application.Current.Dispatcher.BeginInvoke(addOrigin);
                                }
                            }

                            // Destination
                            if (!string.IsNullOrEmpty(this.DestinationICAO))
                            {
                                var airport = localAirportPackage?.Airports.SingleOrDefault(a => a.ICAO == this.DestinationICAO);
                                if (airport != null)
                                {
                                    UpdateGUIDelegate addDestination = () =>
                                    {
                                        var destinationMarker = new TrackingEventMarker(new GeoCoordinate(airport.Latitude, airport.Longitude), this.DestinationICAO, OpenSkyColors.OpenSkyTeal, Colors.White);
                                        this.TrackingEventMarkers.Add(destinationMarker);

                                        var destinationDetailMarker = new TrackingEventMarker(airport, OpenSkyColors.OpenSkyTeal, Colors.White);
                                        this.TrackingEventMarkers.Add(destinationDetailMarker);

                                        foreach (var runway in airport.Runways)
                                        {
                                            var runwayMarker = new TrackingEventMarker(runway);
                                            this.TrackingEventMarkers.Add(runwayMarker);
                                        }
                                    };
                                    Application.Current.Dispatcher.BeginInvoke(addDestination);
                                }
                            }

                            // Alternate
                            if (!string.IsNullOrEmpty(this.AlternateICAO))
                            {
                                var airport = localAirportPackage?.Airports.SingleOrDefault(a => a.ICAO == this.AlternateICAO);
                                if (airport != null)
                                {
                                    UpdateGUIDelegate addAlternate = () =>
                                    {
                                        var alternateMarker = new TrackingEventMarker(new GeoCoordinate(airport.Latitude, airport.Longitude), this.AlternateICAO, OpenSkyColors.OpenSkyWarningOrange, Colors.Black);
                                        this.TrackingEventMarkers.Add(alternateMarker);

                                        var alternateDetailMarker = new TrackingEventMarker(airport, OpenSkyColors.OpenSkyWarningOrange, Colors.Black);
                                        this.TrackingEventMarkers.Add(alternateDetailMarker);

                                        foreach (var runway in airport.Runways)
                                        {
                                            var runwayMarker = new TrackingEventMarker(runway);
                                            this.TrackingEventMarkers.Add(runwayMarker);
                                        }
                                    };
                                    Application.Current.Dispatcher.BeginInvoke(addAlternate);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex);
                            UpdateGUIDelegate showError = () => ModernWpf.MessageBox.Show(ex.Message, "Update airports", MessageBoxButton.OK, MessageBoxImage.Error);
                            Application.Current.Dispatcher.BeginInvoke(showError);
                        }
                    })
            { Name = "FlightPlanViewModel.UpdateAirportMarkers" }.Start();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the payloads on board.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<PlannablePayload> PayloadsOnBoard { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Update plannable payloads distribution depending on selected Origin.
        /// </summary>
        /// <remarks>
        /// sushi.at, 19/12/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void UpdatePlannablePayloads()
        {
            var payloads = new List<PlannablePayload>(this.PayloadsOnBoard);
            payloads.AddRange(this.PayloadsAtOrigin);
            payloads.AddRange(this.PayloadsTowardsOrigin);
            payloads.AddRange(this.OtherPayloads);
            this.PayloadsOnBoard.Clear();
            this.PayloadsAtOrigin.Clear();
            this.PayloadsTowardsOrigin.Clear();
            this.OtherPayloads.Clear();

            if (string.IsNullOrEmpty(this.OriginICAO) && this.SelectedAircraft == null)
            {
                // No origin, no aircraft
                this.OtherPayloads.AddRange(payloads);
            }
            else
            {
                if (this.SelectedAircraft == null)
                {
                    // Origin selected, but no aircraft
                    this.PayloadsAtOrigin.AddRange(payloads.Where(p => p.CurrentLocation == this.OriginICAO));
                    this.PayloadsTowardsOrigin.AddRange(payloads.Where(p => p.CurrentLocation != this.OriginICAO && p.Destinations.Contains(this.OriginICAO)));
                    this.OtherPayloads.AddRange(payloads.Where(p => p.CurrentLocation != this.OriginICAO && !p.Destinations.Contains(this.OriginICAO)));
                }
                else if (string.IsNullOrEmpty(this.OriginICAO))
                {
                    // Aircraft selected, but no origin
                    this.PayloadsOnBoard.AddRange(payloads.Where(p => p.CurrentLocation == this.SelectedAircraft.Registry));
                    this.OtherPayloads.AddRange(payloads.Where(p => p.CurrentLocation != this.SelectedAircraft.Registry));
                }
                else
                {
                    // Both origin and aircraft selected
                    this.PayloadsOnBoard.AddRange(payloads.Where(p => p.CurrentLocation == this.SelectedAircraft.Registry));
                    this.PayloadsAtOrigin.AddRange(payloads.Where(p => p.CurrentLocation == this.OriginICAO));
                    this.PayloadsTowardsOrigin.AddRange(payloads.Where(p => p.CurrentLocation != this.OriginICAO && p.Destinations.Contains(this.OriginICAO)));
                    this.OtherPayloads.AddRange(payloads.Where(p => p.CurrentLocation != this.OriginICAO && p.CurrentLocation != this.SelectedAircraft.Registry && !p.Destinations.Contains(this.OriginICAO)));
                }
            }
        }
    }
}
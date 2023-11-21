// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FlightPlanViewModel.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Pages.Models
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Windows;

    using Microsoft.Maps.MapControl.WPF;

    using OpenSky.Client.Controls;
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
    public partial class FlightPlanViewModel : ViewModel
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// List of start of flight states the user chose to override.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly List<StartFlightStatus> overriddenStates = new();

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The account balances.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private AccountBalances accountBalances;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The user's airline.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private UserAirline airline;

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
        /// The flight number (1-9999).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private int flightNumber = 1;

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
        /// The planned departure time.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private DateTime plannedDepartureTime;

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
            this.TrackingEventMarkers = new ObservableCollection<TrackingEventMarker>();
            this.SimbriefRouteLocations = new LocationCollection();
            this.SimbriefWaypointMarkers = new ObservableCollection<SimbriefWaypointMarker>();
            this.PayloadsOnBoard = new ObservableCollection<PlannablePayload>();
            this.PayloadsAtOrigin = new ObservableCollection<PlannablePayload>();
            this.PayloadsTowardsOrigin = new ObservableCollection<PlannablePayload>();
            this.OtherPayloads = new ObservableCollection<PlannablePayload>();
            this.RouteTrailLocations = new LocationCollection();
            this.Payloads = new ObservableCollection<Guid>();
            this.OriginAirports = new ObservableCollection<string>();
            this.DestinationAirports = new ObservableCollection<string>();
            this.AlternateAirports = new ObservableCollection<string>();
            this.Payloads.CollectionChanged += (_, _) =>
            {
                this.NotifyPropertyChanged(nameof(this.PayloadWeight));
                this.NotifyPropertyChanged(nameof(this.ZeroFuelWeight));
                this.NotifyPropertyChanged(nameof(this.GrossWeight));
                this.IsDirty = true;
            };

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
            this.UpdateAirportsCommand = new AsynchronousCommand(this.UpdateAirports);
            this.RefreshBalancesCommand = new AsynchronousCommand(this.RefreshBalances);

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
        /// Occurs when the map was updated, tell the mapviewer to zoom to contained child elements.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public event EventHandler MapUpdated;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the account balances.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AccountBalances AccountBalances
        {
            get => this.accountBalances;

            set
            {
                if (Equals(this.accountBalances, value))
                {
                    return;
                }

                this.accountBalances = value;
                this.NotifyPropertyChanged();
            }
        }

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
        /// Gets the refresh airline command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand RefreshAirlineCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the refresh balances command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand RefreshBalancesCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the save command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand SaveCommand { get; }

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
        /// Gets the zero fuel weight.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double ZeroFuelWeight => (this.SelectedAircraft?.Type.EmptyWeight ?? 0) + this.PayloadWeight + this.CrewWeight;

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
        /// Deletes the flight plan.
        /// </summary>
        /// <remarks>
        /// sushi.at, 31/10/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void DeleteFlightPlan()
        {
            ExtendedMessageBoxResult? answer = null;
            this.DeleteCommand.ReportProgress(
                () =>
                {
                    var messageBox = new OpenSkyMessageBox(
                        "Delete flight plan?",
                        "Are you sure you want to delete this flight plan?",
                        MessageBoxButton.YesNo,
                        ExtendedMessageBoxImage.Question);
                    messageBox.Closed += (_, _) => { answer = messageBox.Result; };
                    Main.ShowMessageBoxInSaveViewAs(this.ViewReference, messageBox);
                });
            while (answer == null && !SleepScheduler.IsShutdownInProgress)
            {
                Thread.Sleep(500);
            }

            if (answer != ExtendedMessageBoxResult.Yes)
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

                            var notification = new OpenSkyNotification("Error deleting flight plan", result.Message, MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                            notification.SetErrorColorStyle();
                            Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.ViewReference, this.DeleteCommand, "Error deleting flight plan");
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
                    this.LoadFlightPlanCommand.ReportProgress(
                        () =>
                        {
                            this.OriginICAO = flightPlan.OriginICAO;
                            this.DestinationICAO = flightPlan.DestinationICAO;
                            this.AlternateICAO = flightPlan.AlternateICAO;
                        }, true);
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

                            var notification = new OpenSkyNotification("Error refreshing airline info", result.Message, MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                            notification.SetErrorColorStyle();
                            Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.ViewReference, this.RefreshAirlineCommand, "Error refreshing airline info");
            }
            finally
            {
                this.LoadingText = null;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Refresh account balances.
        /// </summary>
        /// <remarks>
        /// sushi.at, 26/01/2022.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void RefreshBalances()
        {
            this.LoadingText = "Refreshing account balances...";
            try
            {
                var result = OpenSkyService.Instance.GetAccountBalancesAsync().Result;
                if (!result.IsError)
                {
                    this.AccountBalances = result.Data;
                }
                else
                {
                    this.RefreshBalancesCommand.ReportProgress(
                        () =>
                        {
                            Debug.WriteLine("Error refreshing account balances: " + result.Message);
                            if (!string.IsNullOrEmpty(result.ErrorDetails))
                            {
                                Debug.WriteLine(result.ErrorDetails);
                            }

                            var notification = new OpenSkyNotification("Error refreshing account balances", result.Message, MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                            notification.SetErrorColorStyle();
                            Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.ViewReference, this.RefreshBalancesCommand, "Error refreshing account balances");
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
                    this.SaveCommand.ReportProgress(
                        () =>
                        {
                            var notification = new OpenSkyNotification("Flight plan", result.Message, MessageBoxButton.OK, ExtendedMessageBoxImage.Check, 10);
                            Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                        });

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

                            var notification = new OpenSkyNotification("Error saving flight plan", result.Message, MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                            notification.SetErrorColorStyle();
                            Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.ViewReference, this.SaveCommand, "Error saving flight plan");
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
                ExtendedMessageBoxResult? answer = null;
                this.StartFlightCommand.ReportProgress(
                    () =>
                    {
                        var messageBox = new OpenSkyMessageBox(
                            "Save flight plan?",
                            "You first have to save your changes, do you want to save the flight plan now?",
                            MessageBoxButton.YesNo,
                            ExtendedMessageBoxImage.Question);
                        messageBox.Closed += (_, _) => { answer = messageBox.Result; };
                        Main.ShowMessageBoxInSaveViewAs(this.ViewReference, messageBox);
                    });
                while (answer == null && !SleepScheduler.IsShutdownInProgress)
                {
                    Thread.Sleep(500);
                }

                if (answer != ExtendedMessageBoxResult.Yes)
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

                                var notification = new OpenSkyNotification("Error saving flight plan", result.Message, MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                                notification.SetErrorColorStyle();
                                Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                            });
                    }
                }
                catch (Exception ex)
                {
                    ex.HandleApiCallException(this.ViewReference, this.SaveCommand, "Error saving flight plan");
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
                var result = OpenSkyService.Instance.StartFlightAsync(
                    new StartFlight
                    {
                        FlightID = this.ID,
                        OverrideStates = this.overriddenStates
                    }).Result;
                if (!result.IsError)
                {
                    if (result.Data == StartFlightStatus.Started)
                    {
                        this.StartFlightCommand.ReportProgress(
                            () =>
                            {
                                var messageBox = new OpenSkyMessageBox("Start flight", result.Message, MessageBoxButton.OK, ExtendedMessageBoxImage.Check, 10);
                                Main.ShowMessageBoxInSaveViewAs(this.ViewReference, messageBox);
                                this.ClosePage?.Invoke(this, null);
                            });

                        AgentAutoLauncher.AutoLaunchAgent();
                    }

                    if (result.Data == StartFlightStatus.AircraftNotAtOrigin)
                    {
                        ExtendedMessageBoxResult? answer = null;
                        this.StartFlightCommand.ReportProgress(
                            () =>
                            {
                                var messageBox = new OpenSkyMessageBox(
                                    "Aircraft not at Origin",
                                    "The selected aircraft is not at the selected origin airport. Do you want to create another flight plan for the positioning flight?",
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
                                            DispatcherRemarks = $"REPOSITIONING FLIGHT FOR {this.SelectedAircraft.Registry} FLIGHT #{this.FlightNumber}"
                                        }
                                    };
                                    Main.ActivateNavMenuItemInSameViewAs(this.ViewReference, navMenuItem);
                                });
                        }
                    }

                    if (result.Data == StartFlightStatus.OriginDoesntSellAvGas)
                    {
                        ExtendedMessageBoxResult? answer = null;
                        this.StartFlightCommand.ReportProgress(
                            () =>
                            {
                                var messageBox = new OpenSkyMessageBox(
                                    "No AV gas",
                                    "The origin airport does not sell AV gas, do you want to start the flight with the fuel already on board the aircraft?"
                                    + "\r\n\r\nWARNING: You may not enough fuel to reach your destination!",
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
                            this.overriddenStates.Add(StartFlightStatus.OriginDoesntSellAvGas);
                            new Thread(
                                () =>
                                {
                                    Thread.Sleep(500);
                                    UpdateGUIDelegate tryAgain = () => this.StartFlightCommand.DoExecute(null);
                                    Application.Current.Dispatcher.BeginInvoke(tryAgain);
                                }).Start();
                        }
                    }

                    if (result.Data == StartFlightStatus.OriginDoesntSellJetFuel)
                    {
                        ExtendedMessageBoxResult? answer = null;
                        this.StartFlightCommand.ReportProgress(
                            () =>
                            {
                                var messageBox = new OpenSkyMessageBox(
                                    "No jet fuel",
                                    "The origin airport does not sell jet fuel, do you want to start the flight with the fuel already on board the aircraft?"
                                    + "\r\n\r\nWARNING: You may not enough fuel to reach your destination!",
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
                            this.overriddenStates.Add(StartFlightStatus.OriginDoesntSellJetFuel);
                            new Thread(
                                () =>
                                {
                                    Thread.Sleep(500);
                                    UpdateGUIDelegate tryAgain = () => this.StartFlightCommand.DoExecute(null);
                                    Application.Current.Dispatcher.BeginInvoke(tryAgain);
                                }).Start();
                        }
                    }

                    if (result.Data == StartFlightStatus.NonFlightPlanPayloadsFound)
                    {
                        ExtendedMessageBoxResult? answer = null;
                        this.StartFlightCommand.ReportProgress(
                            () =>
                            {
                                var messageBox = new OpenSkyMessageBox(
                                    "Non flight payload",
                                    "At least one payload is currently on board the aircraft that is not part of the flight plan, do you want to start the flight anyway?"
                                    + "\r\n\r\nWARNING: Your weight and balance values will not match your flight plan!",
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
                            this.overriddenStates.Add(StartFlightStatus.OriginDoesntSellJetFuel);
                            new Thread(
                                () =>
                                {
                                    Thread.Sleep(500);
                                    UpdateGUIDelegate tryAgain = () => this.StartFlightCommand.DoExecute(null);
                                    Application.Current.Dispatcher.BeginInvoke(tryAgain);
                                }).Start();
                        }
                    }

                    if (result.Data == StartFlightStatus.Error)
                    {
                        this.StartFlightCommand.ReportProgress(
                            () =>
                            {
                                Debug.WriteLine("Error starting flight: " + result.Message);
                                if (!string.IsNullOrEmpty(result.ErrorDetails))
                                {
                                    Debug.WriteLine(result.ErrorDetails);
                                }

                                var notification = new OpenSkyNotification("Error starting flight", result.Message, MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                                notification.SetErrorColorStyle();
                                Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
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

                            var notification = new OpenSkyNotification("Error starting flight", result.Message, MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                            notification.SetErrorColorStyle();
                            Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.ViewReference, this.StartFlightCommand, "Error starting flight");
            }
            finally
            {
                this.LoadingText = null;
            }
        }
    }
}
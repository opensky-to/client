// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MyFleetViewModel.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Pages.Models
{
    using System;
    using System.Collections.ObjectModel;
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
    /// My fleet view model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 29/07/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class MyFleetViewModel : ViewModel
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The account balances.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private AccountBalances accountBalances;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The edit aircraft variants visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Visibility editAircraftVariantsVisibility = Visibility.Collapsed;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The edit aircraft visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Visibility editAircraftVisibility = Visibility.Collapsed;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// True if the edited aircraft is available for purchase.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private bool editedAircraftForPurchase;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// True if the edited aircraft is available for rent.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private bool editedAircraftForRent;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Name of the edited aircraft.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string editedAircraftName;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The edited aircraft original variant ID.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Guid? editedAircraftOriginalVariant;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The edited aircraft purchase price.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private int editedAircraftPurchasePrice;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The edited aircraft registry.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string editedAircraftRegistry;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The edited aircraft rental price.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private int editedAircraftRentPrice;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The ground operations.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private GroundOperations groundOperations;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The ground operations aircraft.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Aircraft groundOperationsAircraft;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The ground operations airport.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Airport groundOperationsAirport;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The ground operations maximum fuel.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private double groundOperationsMaxFuel;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The ground operations maximum fuel weight.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private double groundOperationsMaxFuelWeight;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The ground operations visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Visibility groundOperationsVisibility = Visibility.Collapsed;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The loading text.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string loadingText;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The selected aircraft.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Aircraft selectedAircraft;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The selected edit variant.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private AircraftType selectedEditVariant;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="MyFleetViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 29/07/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public MyFleetViewModel()
        {
            // Initialize data structures
            this.Aircraft = new ObservableCollection<Aircraft>();
            this.EditAircraftVariants = new ObservableCollection<AircraftType>();
            this.Payloads = new ObservableCollection<Guid>();
            this.AirportPayloads = new ObservableCollection<PlannablePayload>();
            this.AircraftPayloads = new ObservableCollection<Payload>();
            this.AircraftPositions = new ObservableCollection<AircraftPosition>();

            // Create commands
            this.RefreshFleetCommand = new AsynchronousCommand(this.RefreshFleet);
            this.RefreshBalancesCommand = new AsynchronousCommand(this.RefreshBalances);
            this.StartEditAircraftCommand = new Command(this.StartEditAircraft, false);
            this.CancelEditAircraftCommand = new Command(this.CancelEditAircraft, false);
            this.SaveEditedAircraftCommand = new AsynchronousCommand(this.SaveEditedAircraft, false);
            this.PlanFlightCommand = new Command(this.PlanFlight, false);
            this.GetEditAircraftVariantsCommand = new AsynchronousCommand(this.GetEditAircraftVariants);
            this.StartGroundOperationsCommand = new AsynchronousCommand(this.StartGroundOperations, false);
            this.CancelGroundOperationsCommand = new Command(this.CancelGroundOperations, false);
            this.GetPlannablePayloadsCommand = new AsynchronousCommand(this.GetPlannablePayloads);
            this.SubmitGroundOperationsCommand = new AsynchronousCommand(this.SubmitGroundOperations, false);
            this.FindJobCommand = new Command(this.FindJob, false);
            this.SellAircraftNowCommand = new AsynchronousCommand(this.SellAircraftNow, false);
        }

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
        /// Gets the aircraft list.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<Aircraft> Aircraft { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the aircraft payloads.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<Payload> AircraftPayloads { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the aircraft positions.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<AircraftPosition> AircraftPositions { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the airport payloads.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<PlannablePayload> AirportPayloads { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the cancel edit aircraft command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command CancelEditAircraftCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the cancel ground operations command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command CancelGroundOperationsCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the edit aircraft variants.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<AircraftType> EditAircraftVariants { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the edit aircraft variants visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Visibility EditAircraftVariantsVisibility
        {
            get => this.editAircraftVariantsVisibility;

            set
            {
                if (Equals(this.editAircraftVariantsVisibility, value))
                {
                    return;
                }

                this.editAircraftVariantsVisibility = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the edit aircraft visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Visibility EditAircraftVisibility
        {
            get => this.editAircraftVisibility;

            set
            {
                if (Equals(this.editAircraftVisibility, value))
                {
                    return;
                }

                this.editAircraftVisibility = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether the edited aircraft is available for purchase.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool EditedAircraftForPurchase
        {
            get => this.editedAircraftForPurchase;

            set
            {
                if (Equals(this.editedAircraftForPurchase, value))
                {
                    return;
                }

                this.editedAircraftForPurchase = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether the edited aircraft is available for rent.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool EditedAircraftForRent
        {
            get => this.editedAircraftForRent;

            set
            {
                if (Equals(this.editedAircraftForRent, value))
                {
                    return;
                }

                this.editedAircraftForRent = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the name of the edited aircraft.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string EditedAircraftName
        {
            get => this.editedAircraftName;

            set
            {
                if (Equals(this.editedAircraftName, value))
                {
                    return;
                }

                this.editedAircraftName = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the edited aircraft purchase price.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int EditedAircraftPurchasePrice
        {
            get => this.editedAircraftPurchasePrice;

            set
            {
                if (Equals(this.editedAircraftPurchasePrice, value))
                {
                    return;
                }

                this.editedAircraftPurchasePrice = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the edited aircraft registry.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string EditedAircraftRegistry
        {
            get => this.editedAircraftRegistry;

            private set
            {
                if (Equals(this.editedAircraftRegistry, value))
                {
                    return;
                }

                this.editedAircraftRegistry = value;
                this.NotifyPropertyChanged();
                this.CancelEditAircraftCommand.CanExecute = value != null;
                this.SaveEditedAircraftCommand.CanExecute = value != null;
                this.EditAircraftVisibility = value != null ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the edited aircraft rental price.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int EditedAircraftRentPrice
        {
            get => this.editedAircraftRentPrice;

            set
            {
                if (Equals(this.editedAircraftRentPrice, value))
                {
                    return;
                }

                this.editedAircraftRentPrice = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the find job command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command FindJobCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the get edit aircraft variants command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand GetEditAircraftVariantsCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the get plannable payloads command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand GetPlannablePayloadsCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the ground operations.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public GroundOperations GroundOperations
        {
            get => this.groundOperations;

            set
            {
                if (Equals(this.groundOperations, value))
                {
                    return;
                }

                this.groundOperations = value;
                this.NotifyPropertyChanged();
                this.CancelGroundOperationsCommand.CanExecute = value != null;
                this.SubmitGroundOperationsCommand.CanExecute = value != null;
                this.GroundOperationsVisibility = value != null ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the ground operations aircraft.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Aircraft GroundOperationsAircraft
        {
            get => this.groundOperationsAircraft;

            set
            {
                if (Equals(this.groundOperationsAircraft, value))
                {
                    return;
                }

                this.groundOperationsAircraft = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the ground operations airport.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Airport GroundOperationsAirport
        {
            get => this.groundOperationsAirport;

            set
            {
                if (Equals(this.groundOperationsAirport, value))
                {
                    return;
                }

                this.groundOperationsAirport = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the ground operations maximum fuel.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double GroundOperationsMaxFuel
        {
            get => this.groundOperationsMaxFuel;

            set
            {
                if (Equals(this.groundOperationsMaxFuel, value))
                {
                    return;
                }

                this.groundOperationsMaxFuel = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the ground operations maximum fuel weight.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double GroundOperationsMaxFuelWeight
        {
            get => this.groundOperationsMaxFuelWeight;

            set
            {
                if (Equals(this.groundOperationsMaxFuelWeight, value))
                {
                    return;
                }

                this.groundOperationsMaxFuelWeight = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the ground operations visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Visibility GroundOperationsVisibility
        {
            get => this.groundOperationsVisibility;

            set
            {
                if (Equals(this.groundOperationsVisibility, value))
                {
                    return;
                }

                this.groundOperationsVisibility = value;
                this.NotifyPropertyChanged();
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
        /// Gets the payloads the player wants on the aircraft.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<Guid> Payloads { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the plan flight command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command PlanFlightCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the refresh balances command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand RefreshBalancesCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the refresh fleet command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand RefreshFleetCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the save edited aircraft command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand SaveEditedAircraftCommand { get; }

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
                this.NotifyPropertyChanged();
                this.StartEditAircraftCommand.CanExecute = value is { CanStartFlight: true };
                this.StartGroundOperationsCommand.CanExecute = value is { CanStartFlight: true };
                this.PlanFlightCommand.CanExecute = value != null;
                this.FindJobCommand.CanExecute = value != null;
                this.SellAircraftNowCommand.CanExecute = value != null;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the selected edit variant.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AircraftType SelectedEditVariant
        {
            get => this.selectedEditVariant;

            set
            {
                if (Equals(this.selectedEditVariant, value))
                {
                    return;
                }

                this.selectedEditVariant = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the sell aircraft now command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand SellAircraftNowCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the start edit aircraft command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command StartEditAircraftCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the start ground operations command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand StartGroundOperationsCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the submit ground operations command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand SubmitGroundOperationsCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Cancel edit aircraft.
        /// </summary>
        /// <remarks>
        /// sushi.at, 17/09/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void CancelEditAircraft()
        {
            this.EditedAircraftRegistry = null;
            this.EditedAircraftName = null;
            this.EditedAircraftForPurchase = false;
            this.EditedAircraftPurchasePrice = 0;
            this.EditedAircraftForRent = false;
            this.EditedAircraftRentPrice = 0;
            this.editedAircraftOriginalVariant = null;
            this.EditAircraftVariantsVisibility = Visibility.Collapsed;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Cancel ground operations.
        /// </summary>
        /// <remarks>
        /// sushi.at, 20/12/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void CancelGroundOperations()
        {
            this.GroundOperations = null;
            this.GroundOperationsMaxFuel = 0;
            this.GroundOperationsMaxFuelWeight = 0;
            this.GroundOperationsAircraft = null;
            this.GroundOperationsAirport = null;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Find a job for the selected aircraft.
        /// </summary>
        /// <remarks>
        /// sushi.at, 23/12/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void FindJob()
        {
            if (this.SelectedAircraft != null)
            {
                var navMenuItem = new NavMenuItem
                {
                    Icon = "/Resources/market16.png", PageType = typeof(JobMarket), Name = "Job market", Parameter = this.SelectedAircraft
                };
                Main.ActivateNavMenuItemInSameViewAs(this.ViewReference, navMenuItem);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the available variants of the aircraft the player is currently editing.
        /// </summary>
        /// <remarks>
        /// sushi.at, 24/11/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void GetEditAircraftVariants()
        {
            if (!this.editedAircraftOriginalVariant.HasValue)
            {
                return;
            }

            this.LoadingText = "Retrieving aircraft variants...";
            try
            {
                var result = OpenSkyService.Instance.GetVariantsOfTypeAsync(this.editedAircraftOriginalVariant.Value).Result;
                if (!result.IsError)
                {
                    this.GetEditAircraftVariantsCommand.ReportProgress(
                        () =>
                        {
                            this.EditAircraftVariants.Clear();
                            this.EditAircraftVariants.AddRange(result.Data);

                            this.EditAircraftVariantsVisibility = this.EditAircraftVariants.Count > 1 ? Visibility.Visible : Visibility.Collapsed;

                            if (this.EditAircraftVariants.Count > 0)
                            {
                                // Select the current variant
                                this.SelectedEditVariant = this.EditAircraftVariants.SingleOrDefault(v => v.Id == this.editedAircraftOriginalVariant.Value);
                            }
                        });
                }
                else
                {
                    this.GetEditAircraftVariantsCommand.ReportProgress(
                        () =>
                        {
                            Debug.WriteLine("Error retrieving variants for aircraft: " + result.Message);
                            if (!string.IsNullOrEmpty(result.ErrorDetails))
                            {
                                Debug.WriteLine(result.ErrorDetails);
                            }

                            var notification = new OpenSkyNotification("Error retrieving variants for aircraft", result.Message, MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                            notification.SetErrorColorStyle();
                            Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.ViewReference, this.GetEditAircraftVariantsCommand, "Error retrieving variants for aircraft.");
            }
            finally
            {
                this.LoadingText = null;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the plannable payloads once the player opens the ground operations editor for an
        /// aircraft.
        /// </summary>
        /// <remarks>
        /// sushi.at, 20/12/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void GetPlannablePayloads()
        {
            this.LoadingText = "Retrieving payloads...";
            try
            {
                var result = OpenSkyService.Instance.GetPlannablePayloadsAsync().Result;
                if (!result.IsError)
                {
                    this.GetPlannablePayloadsCommand.ReportProgress(
                        () =>
                        {
                            this.AirportPayloads.Clear();
                            this.AirportPayloads.AddRange(result.Data.Where(p => p.CurrentLocation == this.SelectedAircraft.AirportICAO));
                        });
                }
                else
                {
                    this.GetPlannablePayloadsCommand.ReportProgress(
                        () =>
                        {
                            Debug.WriteLine("Error retrieving payloads: " + result.Message);
                            if (!string.IsNullOrEmpty(result.ErrorDetails))
                            {
                                Debug.WriteLine(result.ErrorDetails);
                            }

                            var notification = new OpenSkyNotification("Error retrieving payloads", result.Message, MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                            notification.SetErrorColorStyle();
                            Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.ViewReference, this.GetPlannablePayloadsCommand, "Error retrieving payloads.");
            }
            finally
            {
                this.LoadingText = null;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Create a flight plan for the selected aircraft.
        /// </summary>
        /// <remarks>
        /// sushi.at, 10/11/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void PlanFlight()
        {
            if (this.SelectedAircraft != null)
            {
                var flightNumber = new Random().Next(1, 9999);
                var navMenuItem = new NavMenuItem
                {
                    Icon = "/Resources/plan16.png", PageType = typeof(Pages.FlightPlan), Name = $"New flight plan {flightNumber}",
                    Parameter = new FlightPlan
                    {
                        Id = Guid.NewGuid(), FlightNumber = flightNumber, PlannedDepartureTime = DateTime.UtcNow.AddMinutes(30).RoundUp(TimeSpan.FromMinutes(5)), IsNewFlightPlan = true, OriginICAO = this.SelectedAircraft.AirportICAO,
                        Aircraft = this.SelectedAircraft, FuelGallons = this.SelectedAircraft.Fuel, UtcOffset = Properties.Settings.Default.DefaultUTCOffset
                    }
                };
                Main.ActivateNavMenuItemInSameViewAs(this.ViewReference, navMenuItem);
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
        /// Refreshes the fleet list.
        /// </summary>
        /// <remarks>
        /// sushi.at, 29/07/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void RefreshFleet()
        {
            this.LoadingText = "Refreshing your fleet...";
            try
            {
                var result = OpenSkyService.Instance.GetMyAircraftAsync().Result;
                if (!result.IsError)
                {
                    this.RefreshFleetCommand.ReportProgress(
                        () =>
                        {
                            this.Aircraft.Clear();
                            this.AircraftPositions.Clear();
                            foreach (var aircraft in result.Data)
                            {
                                this.Aircraft.Add(aircraft);
                                this.AircraftPositions.Add(
                                    new AircraftPosition
                                    {
                                        Heading = aircraft.Heading,
                                        Location = new Location(aircraft.Latitude, aircraft.Longitude),
                                        Registry = aircraft.Registry,
                                        ToolTip = aircraft.Registry
                                    });
                            }
                        });
                }
                else
                {
                    this.RefreshFleetCommand.ReportProgress(
                        () =>
                        {
                            Debug.WriteLine("Error refreshing your fleet: " + result.Message);
                            if (!string.IsNullOrEmpty(result.ErrorDetails))
                            {
                                Debug.WriteLine(result.ErrorDetails);
                            }

                            var notification = new OpenSkyNotification("Error refreshing your fleet", result.Message, MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                            notification.SetErrorColorStyle();
                            Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.ViewReference, this.RefreshFleetCommand, "Error refreshing your fleet");
            }
            finally
            {
                this.LoadingText = null;
            }

            this.RefreshFleetCommand.ReportProgress(() => this.RefreshBalancesCommand.DoExecute(null));
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Save the edited aircraft.
        /// </summary>
        /// <remarks>
        /// sushi.at, 17/09/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void SaveEditedAircraft()
        {
            if (string.IsNullOrEmpty(this.EditedAircraftRegistry))
            {
                return;
            }

            var updateAircraft = new UpdateAircraft
            {
                Registry = this.EditedAircraftRegistry,
                Name = this.EditedAircraftName,
                VariantID = this.SelectedEditVariant?.Id ?? Guid.Empty
            };

            if (this.EditedAircraftForPurchase)
            {
                updateAircraft.PurchasePrice = this.EditedAircraftPurchasePrice;
            }

            if (this.EditedAircraftForRent)
            {
                updateAircraft.RentPrice = this.EditedAircraftRentPrice;
            }

            this.LoadingText = "Saving changed aircraft";
            try
            {
                var result = OpenSkyService.Instance.UpdateAircraftAsync(updateAircraft).Result;
                if (!result.IsError)
                {
                    this.SaveEditedAircraftCommand.ReportProgress(
                        () =>
                        {
                            var notification = new OpenSkyNotification("Update aircraft", result.Message, MessageBoxButton.OK, ExtendedMessageBoxImage.Check, 10);
                            Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                            this.CancelEditAircraft(); // This resets the input form and hides the groupbox
                            this.RefreshFleetCommand.DoExecute(null);
                        });
                }
                else
                {
                    this.SaveEditedAircraftCommand.ReportProgress(
                        () =>
                        {
                            Debug.WriteLine("Error saving changed aircraft: " + result.Message);
                            if (!string.IsNullOrEmpty(result.ErrorDetails))
                            {
                                Debug.WriteLine(result.ErrorDetails);
                            }

                            var notification = new OpenSkyNotification("Error saving changed aircraft", result.Message, MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                            notification.SetErrorColorStyle();
                            Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.ViewReference, this.SaveEditedAircraftCommand, "Error saving changed aircraft");
            }
            finally
            {
                this.LoadingText = null;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Sell the selected aircraft to the system NOW.
        /// </summary>
        /// <remarks>
        /// sushi.at, 23/12/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void SellAircraftNow()
        {
            if (this.SelectedAircraft == null)
            {
                return;
            }

            ExtendedMessageBoxResult? answer = null;
            this.SellAircraftNowCommand.ReportProgress(
                () =>
                {
                    var messageBox = new OpenSkyMessageBox(
                        "Sell aircraft?",
                        $"Are you sure you want to sell the aircraft {this.SelectedAircraft.Registry.RemoveSimPrefix()}?\r\n\r\nYou will only receive 70 % of the market value of the aircraft.",
                        MessageBoxButton.YesNo,
                        ExtendedMessageBoxImage.Hand);
                    messageBox.SetWarningColorStyle();
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

            this.LoadingText = "Selling aircraft...";
            try
            {
                var result = OpenSkyService.Instance.SellAircraftToSystemAsync(this.SelectedAircraft.Registry).Result;
                if (!result.IsError)
                {
                    this.SellAircraftNowCommand.ReportProgress(
                        () =>
                        {
                            var notification = new OpenSkyNotification("Sell aircraft", result.Message, MessageBoxButton.OK, ExtendedMessageBoxImage.Check, 10);
                            Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                            this.RefreshFleetCommand.DoExecute(null);
                        });
                }
                else
                {
                    this.SellAircraftNowCommand.ReportProgress(
                        () =>
                        {
                            Debug.WriteLine("Error selling aircraft: " + result.Message);
                            if (!string.IsNullOrEmpty(result.ErrorDetails))
                            {
                                Debug.WriteLine(result.ErrorDetails);
                            }

                            var notification = new OpenSkyNotification("Error selling aircraft", result.Message, MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                            notification.SetErrorColorStyle();
                            Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.ViewReference, this.SellAircraftNowCommand, "Error selling aircraft");
            }
            finally
            {
                this.LoadingText = null;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Starts editing the selected aircraft.
        /// </summary>
        /// <remarks>
        /// sushi.at, 17/09/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void StartEditAircraft()
        {
            if (this.SelectedAircraft == null)
            {
                var notification = new OpenSkyNotification("Error", "Please select the aircraft to edit!", MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                notification.SetErrorColorStyle();
                Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                return;
            }

            this.EditedAircraftRegistry = this.SelectedAircraft.Registry;
            this.EditedAircraftName = this.SelectedAircraft.Name;
            this.EditedAircraftForPurchase = this.SelectedAircraft.PurchasePrice.HasValue;
            this.EditedAircraftPurchasePrice = this.SelectedAircraft.PurchasePrice ?? 0;
            this.EditedAircraftForRent = this.SelectedAircraft.RentPrice.HasValue;
            this.EditedAircraftRentPrice = this.SelectedAircraft.RentPrice ?? 0;
            this.editedAircraftOriginalVariant = this.SelectedAircraft.TypeID;
            this.GetEditAircraftVariantsCommand.DoExecute(null);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Starts ground operations.
        /// </summary>
        /// <remarks>
        /// sushi.at, 20/12/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void StartGroundOperations()
        {
            if (this.SelectedAircraft == null)
            {
                this.StartGroundOperationsCommand.ReportProgress(
                    () =>
                    {
                        var notification = new OpenSkyNotification("Error", "Please select the aircraft to perform ground operations for!", MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                        notification.SetErrorColorStyle();
                        Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                    });
                return;
            }

            this.LoadingText = "Retrieving airport information...";
            try
            {
                var result = OpenSkyService.Instance.GetAirportAsync(this.SelectedAircraft.AirportICAO).Result;
                if (!result.IsError)
                {
                    this.GroundOperationsAirport = result.Data;
                }
                else
                {
                    this.StartGroundOperationsCommand.ReportProgress(
                        () =>
                        {
                            Debug.WriteLine("Error retrieving airport information: " + result.Message);
                            if (!string.IsNullOrEmpty(result.ErrorDetails))
                            {
                                Debug.WriteLine(result.ErrorDetails);
                            }

                            var notification = new OpenSkyNotification("Error retrieving airport information", result.Message, MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                            notification.SetErrorColorStyle();
                            Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.ViewReference, this.StartGroundOperationsCommand, "Error retrieving airport information");
            }
            finally
            {
                this.LoadingText = null;
            }

            this.StartGroundOperationsCommand.ReportProgress(
                () =>
                {
                    this.GroundOperations = new GroundOperations
                    {
                        Registry = this.SelectedAircraft.Registry,
                        SelectedFuel = this.SelectedAircraft.Fuel,
                        Fuel = this.SelectedAircraft.Fuel,
                        FuelWeightPerGallon = this.SelectedAircraft.Type.FuelWeightPerGallon,
                        FuelPricePerGallon = this.SelectedAircraft.Type.FuelType switch
                        {
                            FuelType.AvGas => this.GroundOperationsAirport?.AvGasPrice ?? 0,
                            FuelType.JetFuel => this.GroundOperationsAirport?.JetFuelPrice ?? 0,
                            _ => 0
                        }
                    };

                    this.GroundOperationsMaxFuel = this.SelectedAircraft.Type.FuelTotalCapacity;
                    this.GroundOperationsMaxFuelWeight = this.SelectedAircraft.Type.FuelWeightPerGallon * this.SelectedAircraft.Type.FuelTotalCapacity;
                    this.GroundOperationsAircraft = this.SelectedAircraft;
                    this.Payloads.Clear();
                    this.Payloads.AddRange(this.SelectedAircraft.Payloads.Select(p => p.Id));
                    this.AircraftPayloads.Clear();
                    this.AircraftPayloads.AddRange(this.SelectedAircraft.Payloads);
                    this.GetPlannablePayloadsCommand.DoExecute(null);
                });
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Submit ground operations.
        /// </summary>
        /// <remarks>
        /// sushi.at, 20/12/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void SubmitGroundOperations()
        {
            if (this.GroundOperations == null)
            {
                return;
            }

            this.GroundOperations.Payloads = this.Payloads.ToList();
            this.GroundOperations.Fuel = this.GroundOperations.SelectedFuel;

            this.LoadingText = "Starting ground operations...";
            try
            {
                var result = OpenSkyService.Instance.PerformGroundOperationsAsync(this.GroundOperations).Result;
                if (!result.IsError)
                {
                    this.SubmitGroundOperationsCommand.ReportProgress(
                        () =>
                        {
                            var notification = new OpenSkyNotification("Ground operations", result.Message, MessageBoxButton.OK, ExtendedMessageBoxImage.Check, 10);
                            Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                            this.CancelGroundOperations(); // This resets the input form and hides the groupbox
                            this.RefreshFleetCommand.DoExecute(null);
                        });
                }
                else
                {
                    this.SubmitGroundOperationsCommand.ReportProgress(
                        () =>
                        {
                            Debug.WriteLine("Error starting ground operations: " + result.Message);
                            if (!string.IsNullOrEmpty(result.ErrorDetails))
                            {
                                Debug.WriteLine(result.ErrorDetails);
                            }

                            var notification = new OpenSkyNotification("Error starting ground operations", result.Message, MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                            notification.SetErrorColorStyle();
                            Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.ViewReference, this.SubmitGroundOperationsCommand, "Error starting ground operations");
            }
            finally
            {
                this.LoadingText = null;
            }
        }
    }
}
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MyFleetViewModel.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Pages.Models
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Windows;

    using OpenSky.Client.Extensions;
    using OpenSky.Client.MVVM;
    using OpenSky.Client.Tools;
    using OpenSky.Client.Views;
    using OpenSky.Client.Views.Models;

    using OpenSkyApi;

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
        /// The view reference.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private MyFleet viewReference;

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

            // Create commands
            this.RefreshFleetCommand = new AsynchronousCommand(this.RefreshFleet);
            this.StartEditAircraftCommand = new Command(this.StartEditAircraft, false);
            this.CancelEditAircraftCommand = new Command(this.CancelEditAircraft, false);
            this.SaveEditedAircraftCommand = new AsynchronousCommand(this.SaveEditedAircraft, false);
            this.PlanFlightCommand = new Command(this.PlanFlight, false);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the aircraft list.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<Aircraft> Aircraft { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the cancel edit aircraft command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command CancelEditAircraftCommand { get; }

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
        /// Gets the plan flight command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command PlanFlightCommand { get; }

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
                this.StartEditAircraftCommand.CanExecute = value != null;
                this.PlanFlightCommand.CanExecute = value != null;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the start edit aircraft command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command StartEditAircraftCommand { get; }

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
        public void SetViewReference(MyFleet view)
        {
            this.viewReference = view;
        }

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
                        Id = Guid.NewGuid(), FlightNumber = flightNumber, PlannedDepartureTime = DateTime.UtcNow.AddMinutes(45).RoundUp(TimeSpan.FromMinutes(5)), IsNewFlightPlan = true, OriginICAO = this.SelectedAircraft.AirportICAO,
                        Aircraft = this.SelectedAircraft, FuelGallons = this.SelectedAircraft.Fuel
                    }
                };
                Main.ActivateNavMenuItemInSameViewAs(this.viewReference, navMenuItem);
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
                            foreach (var aircraft in result.Data)
                            {
                                this.Aircraft.Add(aircraft);
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

                            ModernWpf.MessageBox.Show(result.Message, "Error refreshing your fleet", MessageBoxButton.OK, MessageBoxImage.Error);
                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.RefreshFleetCommand, "Error refreshing your fleet");
            }
            finally
            {
                this.LoadingText = null;
            }
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
                Name = this.EditedAircraftName
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
                            ModernWpf.MessageBox.Show(result.Message, "Update aircraft", MessageBoxButton.OK, MessageBoxImage.Information);
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

                            ModernWpf.MessageBox.Show(result.Message, "Error saving changed aircraft", MessageBoxButton.OK, MessageBoxImage.Error);
                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.SaveEditedAircraftCommand, "Error saving changed aircraft");
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
                ModernWpf.MessageBox.Show("Please select the aircraft to edit!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            this.EditedAircraftRegistry = this.SelectedAircraft.Registry;
            this.EditedAircraftName = this.SelectedAircraft.Name;
            this.EditedAircraftForPurchase = this.SelectedAircraft.PurchasePrice.HasValue;
            this.EditedAircraftPurchasePrice = this.SelectedAircraft.PurchasePrice ?? 0;
            this.EditedAircraftForRent = this.SelectedAircraft.RentPrice.HasValue;
            this.EditedAircraftRentPrice = this.SelectedAircraft.RentPrice ?? 0;
        }
    }
}
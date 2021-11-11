// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FlightPlansViewModel.cs" company="OpenSky">
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
    /// Flight plans view model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 28/10/2021.
    /// </remarks>
    /// <seealso cref="T:OpenSky.Client.MVVM.ViewModel"/>
    /// -------------------------------------------------------------------------------------------------
    public class FlightPlansViewModel : ViewModel
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The loading text.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string loadingText;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The selected flight plan.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private FlightPlan selectedFlightPlan;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The view reference.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private FlightPlans viewReference;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="FlightPlansViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 28/10/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public FlightPlansViewModel()
        {
            // Initialize data structures
            this.Plans = new ObservableCollection<FlightPlan>();

            // Create commands
            this.RefreshPlansCommand = new AsynchronousCommand(this.RefreshPlans);
            this.NewPlanCommand = new Command(this.NewPlan);
            this.EditPlanCommand = new Command(this.EditPlan);
            this.StartFlightCommand = new AsynchronousCommand(this.StartFlight, false);
            this.DeletePlanCommand = new AsynchronousCommand(this.DeletePlan, false);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the delete plan command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand DeletePlanCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the edit plan command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command EditPlanCommand { get; }

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
        /// Gets the new plan command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command NewPlanCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the flight plans.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<FlightPlan> Plans { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the refresh plans command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand RefreshPlansCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the selected flight plan.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public FlightPlan SelectedFlightPlan
        {
            get => this.selectedFlightPlan;

            set
            {
                if (Equals(this.selectedFlightPlan, value))
                {
                    return;
                }

                this.selectedFlightPlan = value;
                this.NotifyPropertyChanged();
                this.DeletePlanCommand.CanExecute = this.SelectedFlightPlan != null;
                this.StartFlightCommand.CanExecute = this.SelectedFlightPlan != null;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the start flight command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand StartFlightCommand { get; }

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
        public void SetViewReference(FlightPlans view)
        {
            this.viewReference = view;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Deletes the selected flight plan.
        /// </summary>
        /// <remarks>
        /// sushi.at, 31/10/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void DeletePlan()
        {
            if (this.SelectedFlightPlan == null)
            {
                return;
            }

            MessageBoxResult? answer = null;
            this.DeletePlanCommand.ReportProgress(
                () => { answer = ModernWpf.MessageBox.Show($"Are you sure you want to delete the flight plan {this.SelectedFlightPlan.FullFlightNumber}?", "Delete flight plan?", MessageBoxButton.YesNo, MessageBoxImage.Question); },
                true);
            if (answer is not MessageBoxResult.Yes)
            {
                return;
            }

            this.LoadingText = $"Deleting flight plan {this.SelectedFlightPlan.FullFlightNumber}...";
            try
            {
                var result = OpenSkyService.Instance.DeleteFlightPlanAsync(this.SelectedFlightPlan.Id).Result;
                if (!result.IsError)
                {
                    this.DeletePlanCommand.ReportProgress(() => this.RefreshPlansCommand.DoExecute(null));
                }
                else
                {
                    this.DeletePlanCommand.ReportProgress(
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
                ex.HandleApiCallException(this.DeletePlanCommand, "Error deleting flight plan");
            }
            finally
            {
                this.LoadingText = null;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Edit the selected flight plan.
        /// </summary>
        /// <remarks>
        /// sushi.at, 31/10/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void EditPlan()
        {
            if (this.SelectedFlightPlan != null)
            {
                var navMenuItem = new NavMenuItem { Icon = "/Resources/plan16.png", PageType = typeof(Pages.FlightPlan), Name = $"Flight plan  {this.SelectedFlightPlan.FullFlightNumber}", Parameter = this.SelectedFlightPlan };
                Main.ActivateNavMenuItemInSameViewAs(this.viewReference, navMenuItem);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Creates a new flight plan and opens the editing view for the user.
        /// </summary>
        /// <remarks>
        /// sushi.at, 28/10/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void NewPlan()
        {
            var flightNumber = new Random().Next(1, 9999);
            var navMenuItem = new NavMenuItem
            {
                Icon = "/Resources/plan16.png", PageType = typeof(Pages.FlightPlan), Name = $"New flight plan {flightNumber}",
                Parameter = new FlightPlan { Id = Guid.NewGuid(), FlightNumber = flightNumber, PlannedDepartureTime = DateTime.UtcNow.AddMinutes(30).RoundUp(TimeSpan.FromMinutes(5)), IsNewFlightPlan = true }
            };
            Main.ActivateNavMenuItemInSameViewAs(this.viewReference, navMenuItem);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Refreshes the flight plans.
        /// </summary>
        /// <remarks>
        /// sushi.at, 28/10/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void RefreshPlans()
        {
            this.LoadingText = "Refreshing flight plans...";
            try
            {
                var result = OpenSkyService.Instance.GetFlightPlansAsync().Result;
                if (!result.IsError)
                {
                    this.RefreshPlansCommand.ReportProgress(
                        () =>
                        {
                            this.Plans.Clear();
                            foreach (var flightPlan in result.Data)
                            {
                                this.Plans.Add(flightPlan);
                            }
                        });
                }
                else
                {
                    this.RefreshPlansCommand.ReportProgress(
                        () =>
                        {
                            Debug.WriteLine("Error refreshing flight plans: " + result.Message);
                            if (!string.IsNullOrEmpty(result.ErrorDetails))
                            {
                                Debug.WriteLine(result.ErrorDetails);
                            }

                            ModernWpf.MessageBox.Show(result.Message, "Error refreshing flight plans", MessageBoxButton.OK, MessageBoxImage.Error);
                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.RefreshPlansCommand, "Error refreshing flight plans");
            }
            finally
            {
                this.LoadingText = null;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Start the selected flight.
        /// </summary>
        /// <remarks>
        /// sushi.at, 10/11/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void StartFlight()
        {
            if (this.SelectedFlightPlan == null)
            {
                return;
            }

            this.LoadingText = $"Starting flight {this.SelectedFlightPlan.FullFlightNumber}...";
            try
            {
                var result = OpenSkyService.Instance.StartFlightAsync(this.SelectedFlightPlan.Id).Result;
                if (!result.IsError)
                {
                    this.StartFlightCommand.ReportProgress(() => ModernWpf.MessageBox.Show(result.Message, "Start flight", MessageBoxButton.OK, MessageBoxImage.Information));
                }
                else
                {
                    if (result.Data == "AircraftNotAtOrigin")
                    {
                        // todo ask user if he/she wants to create a ferry flight plan to get the plane to the departure airport
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
    }
}
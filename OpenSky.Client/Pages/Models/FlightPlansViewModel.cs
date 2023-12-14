// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FlightPlansViewModel.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Pages.Models
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Windows;

    using OpenSky.Client.Controls;
    using OpenSky.Client.Controls.Models;
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
        /// (Immutable) Dictionary for each flight containing list of start of flight states the user
        /// chose to override.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly Dictionary<Guid, List<StartFlightStatus>> overriddenStates = new();

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The loading text.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string loadingText;

        private IList selectedPlans;

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
            this.EditPlanCommand = new AsynchronousCommand(this.EditPlan, false);
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
        public AsynchronousCommand EditPlanCommand { get; }

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
        /// Gets the start flight command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand StartFlightCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Flight plan selection changed.
        /// </summary>
        /// <remarks>
        /// sushi.at, 14/12/2023.
        /// </remarks>
        /// <param name="nowSelectedPlans">
        /// The now selected plans.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public void FlightPlanSelectionChanged(IList nowSelectedPlans)
        {
            this.selectedPlans = nowSelectedPlans;
            this.EditPlanCommand.CanExecute = this.selectedPlans?.Count > 0;
            this.DeletePlanCommand.CanExecute = this.selectedPlans?.Count > 0;
            this.StartFlightCommand.CanExecute = this.selectedPlans?.Count == 1;
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
            if (this.selectedPlans?.Count > 0)
            {
                if (this.selectedPlans.Count == 1)
                {
                    if (this.selectedPlans[0] is not FlightPlan flightPlan)
                    {
                        return;
                    }

                    ExtendedMessageBoxResult? answer = null;
                    this.DeletePlanCommand.ReportProgress(
                        () =>
                        {
                            var messageBox = new OpenSkyMessageBox(
                                "Delete flight plan?",
                                $"Are you sure you want to delete the flight plan {flightPlan.FullFlightNumber}?",
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
                }
                else
                {
                    ExtendedMessageBoxResult? answer = null;
                    this.DeletePlanCommand.ReportProgress(
                        () =>
                        {
                            var messageBox = new OpenSkyMessageBox(
                                "Delete flight plans?",
                                $"Are you sure you want to delete all the selected flight plans?",
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
                }

                try
                {
                    foreach (var plan in this.selectedPlans.OfType<FlightPlan>())
                    {
                        this.LoadingText = $"Deleting flight plan {plan.FullFlightNumber}...";

                        var result = OpenSkyService.Instance.DeleteFlightPlanAsync(plan.Id).Result;
                        if(result.IsError)
                        {
                            this.DeletePlanCommand.ReportProgress(
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

                    this.DeletePlanCommand.ReportProgress(() => this.RefreshPlansCommand.DoExecute(null));
                }
                catch (Exception ex)
                {
                    ex.HandleApiCallException(this.ViewReference, this.DeletePlanCommand, "Error deleting flight plan");
                }
                finally
                {
                    this.LoadingText = null;
                }
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
            if (this.selectedPlans?.Count > 0)
            {
                foreach (var plan in this.selectedPlans.OfType<FlightPlan>())
                {
                    this.EditPlanCommand.ReportProgress(
                        () =>
                        {
                            var navMenuItem = new NavMenuItem { Icon = "/Resources/plan16.png", PageType = typeof(Pages.FlightPlan), Name = $"Flight plan  {plan.FullFlightNumber}", Parameter = plan };
                            Main.ActivateNavMenuItemInSameViewAs(this.ViewReference, navMenuItem);
                        });
                    Thread.Sleep(200);
                }
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
            Main.ActivateNavMenuItemInSameViewAs(this.ViewReference, navMenuItem);
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

                            var notification = new OpenSkyNotification("Error refreshing flight plans", result.Message, MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                            notification.SetErrorColorStyle();
                            Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.ViewReference, this.RefreshPlansCommand, "Error refreshing flight plans");
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
            if (this.selectedPlans?.Count != 1)
            {
                return;
            }

            if (this.selectedPlans[0] is not FlightPlan flightPlan)
            {
                return;
            }

            this.LoadingText = $"Starting flight {flightPlan.FullFlightNumber}...";
            try
            {
                var flightID = flightPlan.Id;
                if (!this.overriddenStates.ContainsKey(flightID))
                {
                    this.overriddenStates.Add(flightID, new List<StartFlightStatus>());
                }

                var result = OpenSkyService.Instance.StartFlightAsync(
                    new StartFlight
                    {
                        FlightID = flightID,
                        OverrideStates = this.overriddenStates[flightID]
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
                                this.RefreshPlansCommand.DoExecute(null);
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
                                            OriginICAO = flightPlan.Aircraft.AirportICAO, DestinationICAO = flightPlan.OriginICAO, Aircraft = flightPlan.Aircraft,
                                            FuelGallons = flightPlan.Aircraft.Fuel, DispatcherRemarks = $"REPOSITIONING FLIGHT FOR {flightPlan.Aircraft.Registry} FLIGHT #{flightPlan.FlightNumber}"
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
                                    "The origin airport does not sell AV gas, do you want to truck the fuel in from outside the airport?"
                                    + "\r\n\r\nWARNING: You will be charged at greatly increased rates and it will take 30 minutes for the truck to arrive!",
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
                            this.overriddenStates[flightID].Add(StartFlightStatus.OriginDoesntSellAvGas);
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
                                    "The origin airport does not sell jet fuel, do you want to truck the fuel in from outside the airport?"
                                    + "\r\n\r\nWARNING: You will be charged at greatly increased rates and it will take 30 minutes for the truck to arrive!",
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
                            this.overriddenStates[flightID].Add(StartFlightStatus.OriginDoesntSellJetFuel);
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
                            this.overriddenStates[flightID].Add(StartFlightStatus.OriginDoesntSellJetFuel);
                            new Thread(
                                () =>
                                {
                                    Thread.Sleep(500);
                                    UpdateGUIDelegate tryAgain = () => this.StartFlightCommand.DoExecute(null);
                                    Application.Current.Dispatcher.BeginInvoke(tryAgain);
                                }).Start();
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
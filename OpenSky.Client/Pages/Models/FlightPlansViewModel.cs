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

    using OpenSky.Client.Controls;
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

            // Fire off initial commands
            this.RefreshPlansCommand.DoExecute(null);
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
            var navMenuItem = new NavMenuItem { Icon = "/Resources/plan16.png", PageType = typeof(Pages.FlightPlan), Name = "New flight plan", Parameter = new FlightPlan { Id = Guid.NewGuid() } };
            Main mainWindow = null;
            foreach (var instance in Main.Instances)
            {
                foreach (var dockItem in instance.DockingAdapter.ItemsSource)
                {
                    Debug.WriteLine(dockItem);
                    if (dockItem is DockItemEx itemEx)
                    {
                        Debug.WriteLine(itemEx.Content);
                        if (itemEx.Content == this.viewReference)
                        {
                            mainWindow = instance;
                        }
                    }
                }
            }

            mainWindow ??= Main.Instances[0];
            if (mainWindow.DataContext is MainViewModel viewModel)
            {
                viewModel.NavigationItemInvoked(navMenuItem, true, false, true);
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
            }
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
        /// The view reference.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private FlightPlans viewReference;
    }
}
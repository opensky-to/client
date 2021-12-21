// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FlightPlan.xaml.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Pages
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Controls;

    using OpenSky.Client.Pages.Models;
    using OpenSky.Client.Tools;

    using Syncfusion.Windows.Tools.Controls;

    /// -------------------------------------------------------------------------------------------------
    /// <content>
    /// Flight plan page.
    /// </content>
    /// -------------------------------------------------------------------------------------------------
    public partial class FlightPlan
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="FlightPlan"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 28/10/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public FlightPlan()
        {
            this.InitializeComponent();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Page tab/document close button was clicked, ask the page if that is ok right now, set
        /// e.Cancel=true to abort closing the page.
        /// </summary>
        /// <remarks>
        /// sushi.at, 08/11/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Close button event information.
        /// </param>
        /// <seealso cref="M:OpenSky.Client.Controls.OpenSkyPage.CloseButtonClick(object,CloseButtonEventArgs)"/>
        /// -------------------------------------------------------------------------------------------------
        public override void CloseButtonClick(object sender, CloseButtonEventArgs e)
        {
            if (this.DataContext is FlightPlanViewModel { IsDirty: true } viewModel)
            {
                var answer = ModernWpf.MessageBox.Show("Flight plan has unsaved changes, do you want to save them now?", "Save flight plan?", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                if (answer == MessageBoxResult.Yes)
                {
                    viewModel.SaveCommand.DoExecute(null);
                }

                if (answer == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Method that receives an optional page parameter when the page is opened.
        /// </summary>
        /// <remarks>
        /// sushi.at, 28/10/2021.
        /// </remarks>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        /// <seealso cref="M:OpenSky.Client.Controls.OpenSkyPage.PassPageParameter(object)"/>
        /// -------------------------------------------------------------------------------------------------
        public override void PassPageParameter(object parameter)
        {
            if (this.DataContext is FlightPlanViewModel viewModel && parameter is OpenSkyApi.FlightPlan flightPlan)
            {
                viewModel.LoadFlightPlan(flightPlan);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Flight plan on loaded.
        /// </summary>
        /// <remarks>
        /// sushi.at, 31/10/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Routed event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void FlightPlanOnLoaded(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is FlightPlanViewModel viewModel)
            {
                viewModel.SetViewReference(this);
                viewModel.PropertyChanged += this.ViewModelPropertyChanged;
                this.ViewModelPropertyChanged(this, new PropertyChangedEventArgs("IsDirty"));
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Flight plan view model on close page.
        /// </summary>
        /// <remarks>
        /// sushi.at, 31/10/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// An object to process.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void FlightPlanViewModelOnClosePage(object sender, object e)
        {
            if (this.Parent is ContentControl control)
            {
                Debug.WriteLine(control);
                if (control.Parent is DockingManager dockingManager)
                {
                    Debug.WriteLine(dockingManager);
                    dockingManager.Children.Remove(control);
                }
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// View model on map updated.
        /// </summary>
        /// <remarks>
        /// sushi.at, 20/12/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void ViewModelOnMapUpdated(object sender, EventArgs e)
        {
            this.MapView.AnimateAircraftTrail(2);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// View model property changed.
        /// </summary>
        /// <remarks>
        /// sushi.at, 31/10/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Property changed event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void ViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            UpdateGUIDelegate updateTabText = () =>
            {
                if (this.DockItem != null && e.PropertyName is nameof(FlightPlanViewModel.FlightNumber) or nameof(FlightPlanViewModel.IsAirlineFlight) or nameof(FlightPlanViewModel.IsNewFlightPlan) or nameof(FlightPlanViewModel.IsDirty) &&
                    this.DataContext is FlightPlanViewModel viewModel)
                {
                    var documentHeaderText = viewModel.IsNewFlightPlan ? "New flight plan " : "Flight plan ";
                    if (!string.IsNullOrEmpty(viewModel.Airline?.Iata))
                    {
                        documentHeaderText += viewModel.Airline.Iata;
                    }
                    else if (!string.IsNullOrEmpty(viewModel.Airline?.Icao))
                    {
                        documentHeaderText += viewModel.Airline.Icao;
                    }

                    documentHeaderText += $"{viewModel.FlightNumber}";
                    if (viewModel.IsDirty)
                    {
                        documentHeaderText += "*";
                    }

                    this.DockItem.DocumentHeader.Text = documentHeaderText;
                }
            };

            this.Dispatcher.BeginInvoke(updateTabText);
        }
    }
}
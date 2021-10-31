// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FlightPlan.xaml.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Pages
{
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
                viewModel.PropertyChanged += this.ViewModelPropertyChanged;
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
            Debug.WriteLine(this.DockItem);

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
                if (this.DockItem != null && e.PropertyName is nameof(FlightPlanViewModel.FlightNumber) or nameof(FlightPlanViewModel.IsAirlineFlight) or nameof(FlightPlanViewModel.IsNewFlightPlan) &&
                    this.DataContext is FlightPlanViewModel viewmodel)
                {
                    var documentHeaderText = viewmodel.IsNewFlightPlan ? "New flight plan " : "Flight plan ";
                    if (!string.IsNullOrEmpty(viewmodel.Airline?.Iata))
                    {
                        documentHeaderText += viewmodel.Airline.Iata;
                    }
                    else if (!string.IsNullOrEmpty(viewmodel.Airline?.Icao))
                    {
                        documentHeaderText += viewmodel.Airline.Icao;
                    }

                    documentHeaderText += $"{viewmodel.FlightNumber}";

                    this.DockItem.DocumentHeader.Text = documentHeaderText;
                }
            };

            this.Dispatcher.BeginInvoke(updateTabText);
        }
    }
}
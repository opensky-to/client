// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FlightLog.xaml.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Pages
{
    using System;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Input;

    using OpenSky.Client.Controls.Models;
    using OpenSky.Client.Pages.Models;

    using Syncfusion.Windows.Tools.Controls;

    /// -------------------------------------------------------------------------------------------------
    /// <content>
    /// Flight log page.
    /// </content>
    /// -------------------------------------------------------------------------------------------------
    public partial class FlightLog
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="FlightLog"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 17/11/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public FlightLog()
        {
            this.InitializeComponent();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Page tab/document close button was clicked, ask the page if that is ok right now, set
        /// e.Cancel=true to abort closing the page.
        /// </summary>
        /// <remarks>
        /// sushi.at, 17/11/2021.
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
            // Don't care
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Method that receives an optional page parameter when the page is opened.
        /// </summary>
        /// <remarks>
        /// sushi.at, 17/11/2021.
        /// </remarks>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        /// <seealso cref="M:OpenSky.Client.Controls.OpenSkyPage.PassPageParameter(object)"/>
        /// -------------------------------------------------------------------------------------------------
        public override void PassPageParameter(object parameter)
        {
            if (parameter is OpenSkyApi.FlightLog flightLog && this.DataContext is FlightLogViewModel viewModel)
            {
                viewModel.LoadFlightLogCommand.DoExecute(flightLog);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Event log mouse double click.
        /// </summary>
        /// <remarks>
        /// sushi.at, 20/11/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Mouse button event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void EventLogMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (this.EventLog.SelectedItem is TrackingEventLogEntry selectedEventLog)
            {
                Debug.WriteLine($"User double clicked event log entry: {selectedEventLog.LogMessage}");
                this.MapView.Center(selectedEventLog.Location, true);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Flight log on loaded.
        /// </summary>
        /// <remarks>
        /// sushi.at, 14/01/2022.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Routed event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void FlightLogOnLoaded(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is FlightLogViewModel viewModel)
            {
                viewModel.ViewReference = this;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Map view on size changed.
        /// </summary>
        /// <remarks>
        /// sushi.at, 20/11/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Size changed event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void MapViewOnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.MapView.ShowAllMarkers();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// View model on map updated.
        /// </summary>
        /// <remarks>
        /// sushi.at, 20/11/2021.
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
            this.MapView.ShowAllMarkers(true);
            this.MapView.AnimateAircraftTrail();
        }
    }
}
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FlightPlan.xaml.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Pages
{
    using System;
    using System.ComponentModel;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Input;

    using ModernWpf.Controls;

    using OpenSky.Client.Controls;
    using OpenSky.Client.Controls.Models;
    using OpenSky.Client.Pages.Models;
    using OpenSky.Client.Tools;
    using OpenSky.Client.Views;

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
                var messageBox = new OpenSkyMessageBox("Save flight plan?", "Flight plan has unsaved changes, do you want to save them now?", MessageBoxButton.YesNoCancel, ExtendedMessageBoxImage.Question);
                messageBox.Closed += (_, _) =>
                {
                    if (messageBox.Result == ExtendedMessageBoxResult.Yes)
                    {
                        viewModel.SaveCommand.DoExecute(null);
                        this.ClosePage();
                    }

                    if (messageBox.Result == ExtendedMessageBoxResult.No)
                    {
                        this.ClosePage();
                    }
                };

                Main.ShowMessageBoxInSaveViewAs(this, messageBox);
                e.Cancel = true;
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
        /// Airport auto suggest box lost focus, check if we need to uppercase.
        /// </summary>
        /// <remarks>
        /// sushi.at, 08/12/2023.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Routed event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void AirportAutoSuggestLostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is AutoSuggestBox box)
            {
                if (!string.Equals(box.Text, box.Text?.ToUpperInvariant()))
                {
                    box.Text = box.Text?.ToUpperInvariant();
                }
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// An auto-suggestion box submitted a query (aka the user pressed enter or clicked an entry)
        /// </summary>
        /// <remarks>
        /// sushi.at, 08/12/2023.
        /// </remarks>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="args">
        /// Automatic suggest box query submitted event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void AutoSuggestionsQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion != null)
            {
                sender.Text = args.ChosenSuggestion.ToString().Split(':')[0];
            }

            sender.IsSuggestionListOpen = false;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Auto suggest box preview key down.
        /// </summary>
        /// <remarks>
        /// sushi.at, 08/12/2023.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Key event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void AutoSuggestPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (sender is AutoSuggestBox box)
            {
                if (e.Key == Key.PageDown)
                {
                    var method = typeof(AutoSuggestBox).GetMethod("SelectedIndexIncrement", BindingFlags.Instance | BindingFlags.NonPublic);
                    for (var i = 0; i < 5; i++)
                    {
                        method?.Invoke(box, Array.Empty<object>());
                    }
                }

                if (e.Key == Key.PageUp)
                {
                    var method = typeof(AutoSuggestBox).GetMethod("SelectedIndexDecrement", BindingFlags.Instance | BindingFlags.NonPublic);
                    for (var i = 0; i < 5; i++)
                    {
                        method?.Invoke(box, Array.Empty<object>());
                    }
                }
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
                viewModel.ViewReference = this;
                viewModel.RefreshBalancesCommand.DoExecute(null);
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
            this.ClosePage();
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
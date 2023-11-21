// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NewAircraft.xaml.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Pages
{
    using System;
    using System.Windows;

    using ModernWpf.Controls;

    using OpenSky.Client.OpenAPIs.ModelExtensions;
    using OpenSky.Client.Pages.Models;

    using OpenSkyApi;

    using Syncfusion.Windows.Tools.Controls;

    /// -------------------------------------------------------------------------------------------------
    /// <content>
    /// New aircraft page.
    /// </content>
    /// -------------------------------------------------------------------------------------------------
    public partial class NewAircraft
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="NewAircraft"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 14/02/2022.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public NewAircraft()
        {
            this.InitializeComponent();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Page tab/document close button was clicked, ask the page if that is ok right now, set
        /// e.Cancel=true to abort closing the page.
        /// </summary>
        /// <remarks>
        /// sushi.at, 14/02/2022.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Close button event information.
        /// </param>
        /// <seealso cref="OpenSky.Client.Controls.OpenSkyPage.CloseButtonClick(object,CloseButtonEventArgs)"/>
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
        /// sushi.at, 14/02/2022.
        /// </remarks>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        /// <seealso cref="OpenSky.Client.Controls.OpenSkyPage.PassPageParameter(object)"/>
        /// -------------------------------------------------------------------------------------------------
        public override void PassPageParameter(object parameter)
        {
            // None yet
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Automatic suggest box on suggestion chosen.
        /// </summary>
        /// <remarks>
        /// sushi.at, 16/02/2022.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="args">
        /// Automatic suggest box suggestion chosen event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void AircraftTypeAutoSuggestBoxOnSuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            var selection = args.SelectedItem.ToString();
            if (selection.Contains(" [v"))
            {
                selection = selection.Substring(0, selection.IndexOf(" [", StringComparison.Ordinal));
            }

            sender.Text = selection;
            sender.IsSuggestionListOpen = false;

            if (this.DataContext is NewAircraftViewModel viewModel && args.SelectedItem is AircraftType type)
            {
                viewModel.SelectedAircraftType = type;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Auto suggest box suggestion chosen by user.
        /// </summary>
        /// <remarks>
        /// sushi.at, 05/05/2022.
        /// </remarks>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="args">
        /// Automatic suggest box suggestion chosen event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void AutoSuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            sender.Text = args.SelectedItem.ToString();
            sender.IsSuggestionListOpen = false;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// A auto suggestion box submitted a query (aka the find button was clicked)
        /// </summary>
        /// <remarks>
        /// sushi.at, 05/05/2022.
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
            sender.IsSuggestionListOpen = true;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Country automatic suggest box on suggestion chosen.
        /// </summary>
        /// <remarks>
        /// sushi.at, 20/02/2022.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="args">
        /// Automatic suggest box suggestion chosen event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void CountryAutoSuggestBoxOnSuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            sender.Text = args.SelectedItem.ToString();
            sender.IsSuggestionListOpen = false;

            if (this.DataContext is NewAircraftViewModel viewModel && args.SelectedItem is CountryComboItem country)
            {
                viewModel.RegistrationCountry = country.Country;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Creates a new aircraft on loaded.
        /// </summary>
        /// <remarks>
        /// sushi.at, 14/02/2022.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Routed event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void NewAircraftOnLoaded(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is NewAircraftViewModel viewModel)
            {
                viewModel.ViewReference = this;
                viewModel.RefreshBalancesCommand.DoExecute(null);
            }
        }
    }
}
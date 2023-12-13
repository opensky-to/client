// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NewAircraft.xaml.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Pages
{
    using System;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Input;

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
        private void AircraftTypeAutoSuggestionsQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion != null)
            {
                var selection = args.ChosenSuggestion.ToString();
                if (selection.Contains(" [v"))
                {
                    selection = selection.Substring(0, selection.IndexOf(" [", StringComparison.Ordinal));
                }

                sender.Text = selection;
                if (this.DataContext is NewAircraftViewModel viewModel && args.ChosenSuggestion is AircraftType type)
                {
                    viewModel.SelectedAircraftType = type;
                }
            }

            sender.IsSuggestionListOpen = false;
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
        private void AirportAutoSuggestionsQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion != null)
            {
                sender.Text = args.ChosenSuggestion.ToString();
            }

            sender.IsSuggestionListOpen = false;
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
        private void CountryAutoSuggestionsQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion != null)
            {
                sender.Text = args.ChosenSuggestion.ToString();

                if (this.DataContext is NewAircraftViewModel viewModel && args.ChosenSuggestion is CountryComboItem country)
                {
                    viewModel.RegistrationCountry = country.Country;
                }
            }

            sender.IsSuggestionListOpen = false;
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
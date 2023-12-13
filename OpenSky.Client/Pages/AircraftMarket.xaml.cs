// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AircraftMarket.xaml.cs" company="OpenSky">
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

    using OpenSky.Client.Pages.Models;

    using Syncfusion.Windows.Tools.Controls;

    /// -------------------------------------------------------------------------------------------------
    /// <content>
    /// Aircraft market page.
    /// </content>
    /// -------------------------------------------------------------------------------------------------
    public partial class AircraftMarket
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="AircraftMarket"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 19/07/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public AircraftMarket()
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
            // Don't care
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
            // No parameters supported
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Aircraft market on loaded.
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
        private void AircraftMarketOnLoaded(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is AircraftMarketViewModel viewModel)
            {
                viewModel.ViewReference = this;
                viewModel.RefreshBalancesCommand.DoExecute(null);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// An auto-suggestion box submitted a query (aka the user pressed enter or clicked an entry)
        /// </summary>
        /// <remarks>
        /// sushi.at, 26/07/2021.
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
                var selection = args.ChosenSuggestion.ToString();
                if (selection.Contains(" [v"))
                {
                    selection = selection.Substring(0, selection.IndexOf(" [", StringComparison.Ordinal));
                }

                sender.Text = selection;
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
    }
}
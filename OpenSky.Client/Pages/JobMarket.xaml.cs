// --------------------------------------------------------------------------------------------------------------------
// <copyright file="JobMarket.xaml.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Pages
{
    using System.Linq;
    using System.Windows;

    using ModernWpf.Controls;

    using OpenSky.Client.Pages.Models;

    using OpenSkyApi;

    using Syncfusion.Windows.Tools.Controls;

    /// -------------------------------------------------------------------------------------------------
    /// <content>
    /// Job market page.
    /// </content>
    /// -------------------------------------------------------------------------------------------------
    public partial class JobMarket
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="JobMarket"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 13/12/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public JobMarket()
        {
            this.InitializeComponent();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Page tab/document close button was clicked, ask the page if that is ok right now, set
        /// e.Cancel=true to abort closing the page.
        /// </summary>
        /// <remarks>
        /// sushi.at, 13/12/2021.
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
        /// sushi.at, 13/12/2021.
        /// </remarks>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        /// <seealso cref="M:OpenSky.Client.Controls.OpenSkyPage.PassPageParameter(object)"/>
        /// -------------------------------------------------------------------------------------------------
        public override void PassPageParameter(object parameter)
        {
            if (this.DataContext is JobMarketViewModel viewModel && parameter is Aircraft aircraft)
            {
                viewModel.AirportICAO = aircraft.AirportICAO;
                viewModel.AircraftTypeCategory = viewModel.TypeCategories.SingleOrDefault(c => c.AircraftTypeCategory == aircraft.Type.Category);
                viewModel.SearchJobsCommand.DoExecute(null);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Job market on loaded.
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
        private void JobMarketOnLoaded(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is JobMarketViewModel viewModel)
            {
                viewModel.ViewReference = this;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Auto suggest box suggestion chosen by user.
        /// </summary>
        /// <remarks>
        /// sushi.at, 26/07/2021.
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
            sender.Text = args.SelectedItem.ToString().Split(':')[0];
            sender.IsSuggestionListOpen = false;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// A auto suggestion box submitted a query (aka the find button was clicked)
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
            sender.IsSuggestionListOpen = true;
        }
    }
}
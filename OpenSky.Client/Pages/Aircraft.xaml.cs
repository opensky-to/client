// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Aircraft.xaml.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Pages
{
    using ModernWpf.Controls;

    /// -------------------------------------------------------------------------------------------------
    /// <content>
    /// Aircraft page.
    /// </content>
    /// -------------------------------------------------------------------------------------------------
    public partial class Aircraft
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="Aircraft"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 19/07/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public Aircraft()
        {
            this.InitializeComponent();
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
            sender.Text = args.SelectedItem.ToString();
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
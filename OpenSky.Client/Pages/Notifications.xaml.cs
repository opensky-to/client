// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Notifications.xaml.cs" company="OpenSky">
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
    /// Notifications page.
    /// </content>
    /// -------------------------------------------------------------------------------------------------
    public partial class Notifications
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="Notifications"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 18/12/2023.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public Notifications()
        {
            this.InitializeComponent();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Page tab/document close button was clicked, ask the page if that is ok right now, set
        /// e.Cancel=true to abort closing the page.
        /// </summary>
        /// <remarks>
        /// sushi.at, 18/12/2023.
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
        /// sushi.at, 18/12/2023.
        /// </remarks>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        /// <seealso cref="OpenSky.Client.Controls.OpenSkyPage.PassPageParameter(object)"/>
        /// -------------------------------------------------------------------------------------------------
        public override void PassPageParameter(object parameter)
        {
            // No parameters supported
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Automatic suggestions query submitted.
        /// </summary>
        /// <remarks>
        /// sushi.at, 18/12/2023.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="args">
        /// Automatic suggest box query submitted event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void AutoSuggestionsQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion != null)
            {
                sender.Text = args.ChosenSuggestion.ToString();
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
        /// Notifications on loaded.
        /// </summary>
        /// <remarks>
        /// sushi.at, 18/12/2023.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Routed event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void NotificationsOnLoaded(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is NotificationsViewModel viewModel)
            {
                viewModel.ViewReference = this;
                viewModel.RefreshNotificationsCommand.DoExecute(null);
            }
        }
    }
}
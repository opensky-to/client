// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Settings.xaml.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Pages
{
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Navigation;

    using OpenSky.Client.Controls;
    using OpenSky.Client.Controls.Models;
    using OpenSky.Client.Pages.Models;
    using OpenSky.Client.Tools;
    using OpenSky.Client.Views;

    using Syncfusion.Windows.Tools.Controls;

    /// -------------------------------------------------------------------------------------------------
    /// <content>
    /// Settings page.
    /// </content>
    /// -------------------------------------------------------------------------------------------------
    public partial class Settings
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="Settings"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 17/06/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public Settings()
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
            if (this.DataContext is SettingsViewModel { IsDirty: true } viewModel)
            {
                var messageBox = new OpenSkyMessageBox("Save settings?", "There are unsaved setting changes, do you want to save them now?", MessageBoxButton.YesNoCancel, ExtendedMessageBoxImage.Question);
                messageBox.Closed += (_, _) =>
                {
                    if (messageBox.Result == ExtendedMessageBoxResult.Yes)
                    {
                        viewModel.SaveSettingsCommand.DoExecute(null);
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
            // No parameters supported
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Hyperlink on request navigate.
        /// </summary>
        /// <remarks>
        /// sushi.at, 03/11/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Request navigate event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void HyperlinkOnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.ToString());
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Profile image on mouse enter.
        /// </summary>
        /// <remarks>
        /// sushi.at, 25/11/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Mouse event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void ProfileImageOnMouseEnter(object sender, MouseEventArgs e)
        {
            this.CameraCanvas.Visibility = this.ProfileImage.IsMouseOver || this.CameraCanvas.IsMouseOver ? Visibility.Visible : Visibility.Collapsed;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Profile image on mouse leave.
        /// </summary>
        /// <remarks>
        /// sushi.at, 25/11/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Mouse event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void ProfileImageOnMouseLeave(object sender, MouseEventArgs e)
        {
            this.CameraCanvas.Visibility = this.ProfileImage.IsMouseOver || this.CameraCanvas.IsMouseOver ? Visibility.Visible : Visibility.Collapsed;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Settings on loaded.
        /// </summary>
        /// <remarks>
        /// sushi.at, 08/11/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Routed event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void SettingsOnLoaded(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is SettingsViewModel viewModel)
            {
                viewModel.ViewReference = this;
                viewModel.PropertyChanged += this.ViewModelPropertyChanged;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// View model property changed.
        /// </summary>
        /// <remarks>
        /// sushi.at, 08/11/2021.
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
                if (this.DockItem != null && e.PropertyName is nameof(SettingsViewModel.IsDirty) && this.DataContext is SettingsViewModel viewModel)
                {
                    var documentHeaderText = "Settings";
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
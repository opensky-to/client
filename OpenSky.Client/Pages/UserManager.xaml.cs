// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UserManager.xaml.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Pages
{
    using System.Windows;

    using OpenSky.Client.Pages.Models;

    using Syncfusion.Windows.Tools.Controls;

    /// -------------------------------------------------------------------------------------------------
    /// <content>
    /// User manager page.
    /// </content>
    /// -------------------------------------------------------------------------------------------------
    public partial class UserManager
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="UserManager"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 21/11/2023.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public UserManager()
        {
            this.InitializeComponent();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Page tab/document close button was clicked, ask the page if that is ok right now, set
        /// e.Cancel=true to abort closing the page.
        /// </summary>
        /// <remarks>
        /// sushi.at, 21/11/2023.
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
        /// sushi.at, 21/11/2023.
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

        private void UserManagerOnLoaded(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is UserManagerViewModel viewModel)
            {
                viewModel.ViewReference = this;
                viewModel.RefreshUsersCommand.DoExecute(null);
            }
        }
    }
}
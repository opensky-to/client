﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Settings.xaml.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Pages
{
    using System.Diagnostics;
    using System.Windows.Navigation;

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
    }
}
﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MyFleet.xaml.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Pages
{
    using System.Windows;

    using OpenSky.Client.Pages.Models;

    using Syncfusion.Windows.Tools.Controls;

    /// -------------------------------------------------------------------------------------------------
    /// <content>
    /// My fleet page.
    /// </content>
    /// -------------------------------------------------------------------------------------------------
    public partial class MyFleet
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="MyFleet"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 29/07/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public MyFleet()
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
        /// My fleet on loaded.
        /// </summary>
        /// <remarks>
        /// sushi.at, 10/11/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Routed event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void MyFleetOnLoaded(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is MyFleetViewModel viewModel)
            {
                viewModel.SetViewReference(this);
                viewModel.RefreshFleetCommand.DoExecute(null);
            }
        }
    }
}
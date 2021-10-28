// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FlightPlan.xaml.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Pages
{
    using OpenSky.Client.Pages.Models;

    /// -------------------------------------------------------------------------------------------------
    /// <content>
    /// Flight plan page.
    /// </content>
    /// -------------------------------------------------------------------------------------------------
    public partial class FlightPlan
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="FlightPlan"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 28/10/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public FlightPlan()
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
            if (this.DataContext is FlightPlanViewModel viewModel && parameter is OpenSkyApi.FlightPlan flightPlan)
            {
                viewModel.LoadFlightPlan(flightPlan);
            }
        }
    }
}
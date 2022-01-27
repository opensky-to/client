// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AutoUpdate.xaml.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Views
{
    using System;

    /// -------------------------------------------------------------------------------------------------
    /// <content>
    /// Auto update window.
    /// </content>
    /// -------------------------------------------------------------------------------------------------
    public partial class AutoUpdate
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the currently active instance.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static AutoUpdate Instance { get; private set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="AutoUpdate"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 25/11/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public AutoUpdate()
        {
            Instance = this;
            this.InitializeComponent();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The viewmodel wants to close the window.
        /// </summary>
        /// <remarks>
        /// sushi.at, 24/11/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void AutoUpdateViewModelOnClose(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
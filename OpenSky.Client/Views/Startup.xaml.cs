// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Startup.xaml.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Views
{
    using System;
    using System.Windows.Media.Animation;

    using JetBrains.Annotations;

    /// -------------------------------------------------------------------------------------------------
    /// <content>
    /// Startup window
    /// </content>
    /// -------------------------------------------------------------------------------------------------
    public partial class Startup
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 11/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public Startup()
        {
            if (!StartupFailed)
            {
                if (Instance != null)
                {
                    throw new Exception("Only one instance of the startup window may be created!");
                }

                Instance = this;
                this.InitializeComponent();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the single instance of the startup window.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [CanBeNull]
        public static Startup Instance { get; private set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether our application parent report a failed startup.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static bool StartupFailed { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The view model is done initializing the application and wants to close the splash screen
        /// window.
        /// </summary>
        /// <remarks>
        /// sushi.at, 15/06/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void StartupViewModelOnCloseWindow(object sender, EventArgs e)
        {
            if (this.Resources["HideWindow"] is Storyboard storyboard)
            {
                storyboard.Begin();
            }
        }
    }
}
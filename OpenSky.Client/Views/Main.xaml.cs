// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Main.xaml.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Views
{
    using System;
    using System.Collections.Generic;
    using System.Windows;

    using OpenSky.Client.Tools;
    using OpenSky.Client.Views.Models;

    /// -------------------------------------------------------------------------------------------------
    /// <content>
    /// Main OpenSky application window.
    /// </content>
    /// -------------------------------------------------------------------------------------------------
    public partial class Main
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="Main"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 15/06/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public Main()
        {
            Instances.Add(this);
            this.InitializeComponent();

            if (Instances.Count == 1)
            {
                // We are the first window, tell the viewmodel to add the welcome view
                if (this.DataContext is MainViewModel viewModel)
                {
                    viewModel.ShowWelcomePage();
                }
            }
            else
            {
                this.NavigationView.IsPaneOpen = false;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The single instance of this view.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static List<Main> Instances { get; } = new();

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Main on closed event.
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
        private void MainOnClosed(object sender, EventArgs e)
        {
            Instances.Remove(this);

            if (Instances.Count == 0)
            {
                UpdateGUIDelegate cleanUp = SleepScheduler.Shutdown;
                ((App)Application.Current).RequestShutdown(cleanUp);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Tabablz control on is dragging window changed.
        /// </summary>
        /// <remarks>
        /// sushi.at, 17/06/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// A RoutedPropertyChangedEventArgs&lt;bool&gt; to process.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void TabablzControlOnIsDraggingWindowChanged(object sender, RoutedPropertyChangedEventArgs<bool> e)
        {
            this.Opacity = e.NewValue ? 0.3 : 1;
        }
    }
}
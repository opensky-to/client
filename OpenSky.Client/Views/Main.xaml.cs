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

    using ModernWpf.Controls;

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
        /// The last navigation item invoked Date/Time.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private DateTime lastNavigationItemInvoked = DateTime.MinValue;

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

            if (!App.IsDesignMode)
            {
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

                    // Add our docking manager as a valid target to the others and vice versa
                    foreach (var instance in Instances)
                    {
                        if (instance != this)
                        {
                            instance.DockingAdapter.PART_DockingManager.AddToTargetManagersList(this.DockingAdapter.PART_DockingManager);
                            this.DockingAdapter.PART_DockingManager.AddToTargetManagersList(instance.DockingAdapter.PART_DockingManager);
                        }
                    }
                }
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
        /// Navigation view on item invoked.
        /// </summary>
        /// <remarks>
        /// sushi.at, 22/06/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="args">
        /// Navigation view item invoked event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void NavigationView_OnItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (this.DataContext is MainViewModel viewModel && (DateTime.Now - this.lastNavigationItemInvoked).TotalMilliseconds > 500)
            {
                viewModel.NavigationItemInvoked(args.InvokedItemContainer?.DataContext as NavMenuItem);
                this.lastNavigationItemInvoked = DateTime.Now;
            }
        }
    }
}
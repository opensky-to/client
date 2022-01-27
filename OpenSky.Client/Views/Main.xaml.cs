// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Main.xaml.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Views
{
    using System;
    using System.Collections.Generic;
    using System.Windows;

    using ModernWpf.Controls;

    using OpenSky.Client.Controls;
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
        /// Activates the specified navigation menu item in the same main window that contains the
        /// specified source view.
        /// </summary>
        /// <remarks>
        /// sushi.at, 31/10/2021.
        /// </remarks>
        /// <param name="sourceView">
        /// Source view.
        /// </param>
        /// <param name="navMenuItem">
        /// The navigation menu item.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public static void ActivateNavMenuItemInSameViewAs(FrameworkElement sourceView, NavMenuItem navMenuItem)
        {
            var mainWindow = FindMainForFrameworkElement(sourceView);
            if (mainWindow.DataContext is MainViewModel viewModel)
            {
                viewModel.NavigationItemInvoked(navMenuItem, true, false, true);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Shows the message box in save view as the specified framework element (typically OpenSkyPage).
        /// </summary>
        /// <remarks>
        /// sushi.at, 13/01/2022.
        /// </remarks>
        /// <param name="sourceView">
        /// Source view.
        /// </param>
        /// <param name="messageBox">
        /// The message box.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public static void ShowMessageBoxInSaveViewAs(FrameworkElement sourceView, OpenSkyMessageBox messageBox)
        {
            var mainWindow = FindMainForFrameworkElement(sourceView);
            mainWindow.ShowMessageBox(messageBox);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Shows the notification in same view as the specified framework element (typically OpenSkyPage).
        /// </summary>
        /// <remarks>
        /// sushi.at, 13/01/2022.
        /// </remarks>
        /// <param name="sourceView">
        /// Source view.
        /// </param>
        /// <param name="notification">
        /// The notification.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public static void ShowNotificationInSameViewAs(FrameworkElement sourceView, OpenSkyNotification notification)
        {
            var mainWindow = FindMainForFrameworkElement(sourceView);
            mainWindow.ShowNotification(notification);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Find the main window containing the specified framework element inside it's docking adapter.
        /// </summary>
        /// <remarks>
        /// sushi.at, 13/01/2022.
        /// </remarks>
        /// <param name="element">
        /// The element (typically an OpenSkyPage).
        /// </param>
        /// <returns>
        /// The found main for framework element.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        private static Main FindMainForFrameworkElement(FrameworkElement element)
        {
            Main mainWindow = null;
            foreach (var instance in Instances)
            {
                foreach (var dockItem in instance.DockingAdapter.ItemsSource)
                {
                    if (dockItem is DockItemEx itemEx)
                    {
                        if (itemEx.Content == element)
                        {
                            mainWindow = instance;
                        }
                    }
                }
            }

            mainWindow ??= Instances[0];
            return mainWindow;
        }

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
            if (args.InvokedItemContainer?.DataContext is NavMenuItem menuItem && this.DataContext is MainViewModel viewModel)
            {
                viewModel.NavigationItemInvoked(menuItem);
            }
        }
    }
}
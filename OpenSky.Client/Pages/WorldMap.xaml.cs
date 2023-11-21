// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WorldMap.xaml.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Pages
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Windows;

    using OpenSky.Client.Controls;
    using OpenSky.Client.Controls.Models;
    using OpenSky.Client.Pages.Models;
    using OpenSky.Client.Tools;
    using OpenSky.Client.Views;

    using Syncfusion.Windows.Tools.Controls;

    /// -------------------------------------------------------------------------------------------------
    /// <content>
    /// World map page.
    /// </content>
    /// -------------------------------------------------------------------------------------------------
    public partial class WorldMap
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The update thread mutex.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly Mutex updateThreadMutex = new(false);

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// True to keep updating the map in the background thread.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private bool updateMap;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="WorldMap"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 18/11/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public WorldMap()
        {
            this.InitializeComponent();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Page tab/document close button was clicked, ask the page if that is ok right now, set
        /// e.Cancel=true to abort closing the page.
        /// </summary>
        /// <remarks>
        /// sushi.at, 18/11/2021.
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
        /// sushi.at, 18/11/2021.
        /// </remarks>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        /// <seealso cref="M:OpenSky.Client.Controls.OpenSkyPage.PassPageParameter(object)"/>
        /// -------------------------------------------------------------------------------------------------
        public override void PassPageParameter(object parameter)
        {
            // None so far
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// World map on loaded.
        /// </summary>
        /// <remarks>
        /// sushi.at, 18/11/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Routed event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void WorldMapOnLoaded(object sender, RoutedEventArgs e)
        {
            this.updateMap = true;
            if (this.DataContext is WorldMapViewModel viewModel)
            {
                viewModel.ViewReference = this;
                new Thread(
                        () =>
                        {
                            try
                            {
                                if (this.updateThreadMutex.WaitOne(500))
                                {
                                    while (this.updateMap && !SleepScheduler.IsShutdownInProgress)
                                    {
                                        UpdateGUIDelegate refresh = () => viewModel.RefreshCommand.DoExecute(null);
                                        this.Dispatcher.BeginInvoke(refresh);
                                        SleepScheduler.SleepFor(TimeSpan.FromSeconds(30));
                                    }

                                    Debug.WriteLine("World map updater thread finished...");
                                }
                            }
                            catch (AbandonedMutexException)
                            {
                                // Ignore
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"Error updating world map: {ex}");
                                UpdateGUIDelegate showNotification = () =>
                                {
                                    var notification = new OpenSkyNotification("Error updating world map", ex.Message, MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                                    notification.SetErrorColorStyle();
                                    Main.ShowNotificationInSameViewAs(this, notification);
                                };
                                this.Dispatcher.BeginInvoke(showNotification);
                            }
                            finally
                            {
                                try
                                {
                                    this.updateThreadMutex.ReleaseMutex();
                                }
                                catch
                                {
                                    // Ignore
                                }
                            }
                        })

                    { Name = "OpenSky.WorldMap.Update" }.Start();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// World map on unloaded.
        /// </summary>
        /// <remarks>
        /// sushi.at, 18/11/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Routed event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void WorldMapOnUnloaded(object sender, RoutedEventArgs e)
        {
            this.updateMap = false;
        }
    }
}
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataImport.xaml.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Pages
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;

    using DataGridExtensions;

    using OpenSky.Client.Controls;
    using OpenSky.Client.Controls.Models;
    using OpenSky.Client.Pages.Models;
    using OpenSky.Client.Tools;
    using OpenSky.Client.Views;

    using Syncfusion.Windows.Tools.Controls;

    /// -------------------------------------------------------------------------------------------------
    /// <content>
    /// Data import page.
    /// </content>
    /// -------------------------------------------------------------------------------------------------
    public partial class DataImport
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="DataImport"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 17/06/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public DataImport()
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
            // todo check when we overhaul this
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
        /// Clears all filters on click.
        /// </summary>
        /// <remarks>
        /// sushi.at, 02/07/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Routed event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void ClearAllFiltersOnClick(object sender, RoutedEventArgs e)
        {
            this.DataImportsGrid.GetFilter().Clear();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Data import on loaded.
        /// </summary>
        /// <remarks>
        /// sushi.at, 14/01/2022.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Routed event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void DataImportOnLoaded(object sender, RoutedEventArgs e)
        {
            this.updateImportStatus = true;
            if (this.DataContext is DataImportViewModel viewModel)
            {
                viewModel.ViewReference = this;
                new Thread(
                        () =>
                        {
                            try
                            {
                                if (this.updateThreadMutex.WaitOne(500))
                                {
                                    while (this.updateImportStatus && !SleepScheduler.IsShutdownInProgress)
                                    {
                                        UpdateGUIDelegate refresh = () => viewModel.RefreshDataImportStatusCommand.DoExecute(null);
                                        this.Dispatcher.BeginInvoke(refresh);
                                        SleepScheduler.SleepFor(TimeSpan.FromSeconds(5));
                                    }
                                }
                            }
                            catch (AbandonedMutexException)
                            {
                                // Ignore
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"Error updating data import status: {ex}");
                                UpdateGUIDelegate showNotification = () =>
                                {
                                    var notification = new OpenSkyNotification("Error updating data import status", ex.Message, MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
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
                    { Name = "OpenSky.DataImport.Update" }.Start();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Data import on unloaded.
        /// </summary>
        /// <remarks>
        /// sushi.at, 27/01/2022.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Routed event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void DataImportOnUnloaded(object sender, RoutedEventArgs e)
        {
            this.updateImportStatus = false;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The update thread mutex.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly Mutex updateThreadMutex = new(false);

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// True to keep updating the import status of the selected entry in the background thread.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private bool updateImportStatus;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Textbox text changed (auto scrolls to the end)
        /// </summary>
        /// <remarks>
        /// sushi.at, 02/07/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Text changed event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void TextBoxBaseOnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBoxBase textBox)
            {
                textBox.ScrollToEnd();
            }
        }

        
    }
}
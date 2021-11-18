// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FlightLogsViewModel.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Pages.Models
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Windows;

    using OpenSky.Client.MVVM;
    using OpenSky.Client.Tools;
    using OpenSky.Client.Views;
    using OpenSky.Client.Views.Models;

    using OpenSkyApi;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Flight logs view model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 15/11/2021.
    /// </remarks>
    /// <seealso cref="T:OpenSky.Client.MVVM.ViewModel"/>
    /// -------------------------------------------------------------------------------------------------
    public class FlightLogsViewModel : ViewModel
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The loading text.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string loadingText;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The selected flight log.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private OpenSkyApi.FlightLog selectedFlightLog;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The view reference.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private FlightLogs viewReference;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="FlightLogsViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 15/11/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public FlightLogsViewModel()
        {
            // Initialize data structures
            this.FlightLogs = new ObservableCollection<OpenSkyApi.FlightLog>();

            // Create commands
            this.RefreshFlightLogsCommand = new AsynchronousCommand(this.RefreshFlightLogs);
            this.OpenLogCommand = new Command(this.OpenLog, false);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the flight logs.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<OpenSkyApi.FlightLog> FlightLogs { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the loading text.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string LoadingText
        {
            get => this.loadingText;

            set
            {
                if (Equals(this.loadingText, value))
                {
                    return;
                }

                this.loadingText = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the open log command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command OpenLogCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the refresh flight logs command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand RefreshFlightLogsCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the selected flight log.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public OpenSkyApi.FlightLog SelectedFlightLog
        {
            get => this.selectedFlightLog;

            set
            {
                if (Equals(this.selectedFlightLog, value))
                {
                    return;
                }

                this.selectedFlightLog = value;
                this.NotifyPropertyChanged();
                this.OpenLogCommand.CanExecute = value != null;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Sets the view reference for this view model (to determine main window to open new tabs in, in
        /// case the user has multiple open windows)
        /// </summary>
        /// <remarks>
        /// sushi.at, 28/10/2021.
        /// </remarks>
        /// <param name="view">
        /// The view reference.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public void SetViewReference(FlightLogs view)
        {
            this.viewReference = view;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Open the selected flight log.
        /// </summary>
        /// <remarks>
        /// sushi.at, 17/11/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void OpenLog()
        {
            if (this.SelectedFlightLog != null)
            {
                var navMenuItem = new NavMenuItem { Icon = "/Resources/book16.png", PageType = typeof(Pages.FlightLog), Name = $"Flight log {this.SelectedFlightLog.FullFlightNumber}", Parameter = this.SelectedFlightLog };
                Main.ActivateNavMenuItemInSameViewAs(this.viewReference, navMenuItem);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Refresh flight logs.
        /// </summary>
        /// <remarks>
        /// sushi.at, 15/11/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void RefreshFlightLogs()
        {
            this.LoadingText = "Refreshing flight logs...";
            try
            {
                var result = OpenSkyService.Instance.GetMyFlightLogsAsync().Result;
                if (!result.IsError)
                {
                    this.RefreshFlightLogsCommand.ReportProgress(
                        () =>
                        {
                            this.FlightLogs.Clear();
                            foreach (var flightLog in result.Data)
                            {
                                this.FlightLogs.Add(flightLog);
                            }
                        });
                }
                else
                {
                    this.RefreshFlightLogsCommand.ReportProgress(
                        () =>
                        {
                            Debug.WriteLine("Error refreshing flight logs: " + result.Message);
                            if (!string.IsNullOrEmpty(result.ErrorDetails))
                            {
                                Debug.WriteLine(result.ErrorDetails);
                            }

                            ModernWpf.MessageBox.Show(result.Message, "Error refreshing flight logs", MessageBoxButton.OK, MessageBoxImage.Error);
                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.RefreshFlightLogsCommand, "Error refreshing flight logs");
            }
            finally
            {
                this.LoadingText = null;
            }
        }
    }
}
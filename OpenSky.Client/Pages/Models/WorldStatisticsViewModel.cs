﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WorldStatisticsViewModel.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Pages.Models
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Windows;

    using OpenSky.Client.Controls;
    using OpenSky.Client.Controls.Models;
    using OpenSky.Client.MVVM;
    using OpenSky.Client.Tools;
    using OpenSky.Client.Views;

    using OpenSkyApi;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// World population view model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 02/07/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class WorldStatisticsViewModel : ViewModel
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The loading text.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string loadingText;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The world population overview.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private WorldStatisticsOverview overview;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The manual populate ICAO string.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string populateICAO;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The populate airport result.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string populateAirportResult;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The selected failed airport.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Airport selectedFailedAirport;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The populate airport visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Visibility populateAirportAirportVisibility = Visibility.Collapsed;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the toggle populate airport command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command TogglePopulateAirportCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the populate airport visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Visibility PopulateAirportVisibility
        {
            get => this.populateAirportAirportVisibility;
        
            set
            {
                if(Equals(this.populateAirportAirportVisibility, value))
                {
                   return;
                }
        
                this.populateAirportAirportVisibility = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged(nameof(this.PopulateAirportCommandText));
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the populate airport command text.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string PopulateAirportCommandText => this.PopulateAirportVisibility == Visibility.Collapsed ? "Show populate aircraft" : "Hide populate aircraft";

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="WorldStatisticsViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 02/07/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public WorldStatisticsViewModel()
        {
            // Initialize data structures
            this.UnprocessedAirports = new ObservableCollection<Airport>();
            this.FailedAirports = new ObservableCollection<Airport>();

            // Create commands
            this.RefreshViewCommand = new AsynchronousCommand(this.RefreshView);
            this.TogglePopulateAirportCommand = new Command(this.TogglePopulateAirport);
            this.PopulateSelectedFailedCommand = new Command(this.PopulateSelectedFailed, false);
            this.PopulateAirportCommand = new AsynchronousCommand(this.PopulateAirport, false);

            this.RefreshViewCommand.DoExecute(null);
        }

        private void TogglePopulateAirport()
        {
            this.PopulateAirportVisibility = this.PopulateAirportVisibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the failed airports.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<Airport> FailedAirports { get; }

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
        /// Gets or sets the world statistics overview.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public WorldStatisticsOverview Overview
        {
            get => this.overview;

            set
            {
                if (Equals(this.overview, value))
                {
                    return;
                }

                this.overview = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the populate airport command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand PopulateAirportCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the ICAO identifier to manually populate.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string PopulateICAO
        {
            get => this.populateICAO;

            set
            {
                if (Equals(this.populateICAO, value))
                {
                    return;
                }

                this.populateICAO = value;
                this.NotifyPropertyChanged();
                this.PopulateAirportCommand.CanExecute = !string.IsNullOrEmpty(value);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the populate airport result.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string PopulateAirportResult
        {
            get => this.populateAirportResult;

            set
            {
                if (Equals(this.populateAirportResult, value))
                {
                    return;
                }

                this.populateAirportResult = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the populate selected failed airport command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command PopulateSelectedFailedCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the refresh view command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand RefreshViewCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the selected failed airport.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Airport SelectedFailedAirport
        {
            get => this.selectedFailedAirport;

            set
            {
                if (Equals(this.selectedFailedAirport, value))
                {
                    return;
                }

                this.selectedFailedAirport = value;
                this.NotifyPropertyChanged();
                this.PopulateSelectedFailedCommand.CanExecute = value != null;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the unprocessed airports.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<Airport> UnprocessedAirports { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Populate airport specified by ICAO.
        /// </summary>
        /// <remarks>
        /// sushi.at, 05/07/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void PopulateAirport()
        {
            this.LoadingText = $"Populating airport {this.PopulateICAO}...";
            try
            {
                this.PopulateAirportResult = string.Empty;
                var result = OpenSkyService.Instance.PopulateAirportWithAircraftAsync(this.PopulateICAO).Result;
                if (!result.IsError)
                {
                    this.PopulateAirportResult = result.Data;
                }
                else
                {
                    this.PopulateAirportCommand.ReportProgress(
                        () =>
                        {
                            Debug.WriteLine("Error populating airport: " + result.Message);
                            if (!string.IsNullOrEmpty(result.ErrorDetails))
                            {
                                Debug.WriteLine(result.ErrorDetails);
                            }

                            var notification = new OpenSkyNotification("Error populating airport", result.Message, MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                            notification.SetErrorColorStyle();
                            Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.ViewReference, this.RefreshViewCommand, "Error populating airport");
            }
            finally
            {
                this.LoadingText = null;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Populate selected failed airport.
        /// </summary>
        /// <remarks>
        /// sushi.at, 05/07/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void PopulateSelectedFailed()
        {
            this.PopulateICAO = this.SelectedFailedAirport.Icao;
            this.PopulateAirportCommand.DoExecute(null);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Refreshes the world statistics view.
        /// </summary>
        /// <remarks>
        /// sushi.at, 02/07/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void RefreshView()
        {
            this.LoadingText = "Refreshing world statistics";
            try
            {
                var overviewResult = OpenSkyService.Instance.GetWorldStatisticsOverviewAsync().Result;
                if (!overviewResult.IsError)
                {
                    this.Overview = overviewResult.Data;
                }
                else
                {
                    this.RefreshViewCommand.ReportProgress(
                        () =>
                        {
                            Debug.WriteLine("Error refreshing world statistics overview: " + overviewResult.Message);
                            if (!string.IsNullOrEmpty(overviewResult.ErrorDetails))
                            {
                                Debug.WriteLine(overviewResult.ErrorDetails);
                            }

                            var notification = new OpenSkyNotification("Error refreshing world statistics overview", overviewResult.Message, MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                            notification.SetErrorColorStyle();
                            Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                        });
                }

                var failedAirportsResult = OpenSkyService.Instance.GetAirportsWithPopulationStatusAsync(ProcessingStatus.Failed, 50).Result;
                if (!failedAirportsResult.IsError)
                {
                    this.RefreshViewCommand.ReportProgress(
                        () =>
                        {
                            this.FailedAirports.Clear();
                            foreach (var airport in failedAirportsResult.Data)
                            {
                                this.FailedAirports.Add(airport);
                            }
                        });
                }
                else
                {
                    this.RefreshViewCommand.ReportProgress(
                        () =>
                        {
                            Debug.WriteLine("Error refreshing failed airports: " + failedAirportsResult.Message);
                            if (!string.IsNullOrEmpty(failedAirportsResult.ErrorDetails))
                            {
                                Debug.WriteLine(failedAirportsResult.ErrorDetails);
                            }

                            var notification = new OpenSkyNotification("Error refreshing failed airports", failedAirportsResult.Message, MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                            notification.SetErrorColorStyle();
                            Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                        });
                }

                var unprocessedAirportsResult = OpenSkyService.Instance.GetAirportsWithPopulationStatusAsync(ProcessingStatus.NeedsHandling, 50).Result;
                if (!unprocessedAirportsResult.IsError)
                {
                    this.RefreshViewCommand.ReportProgress(
                        () =>
                        {
                            this.UnprocessedAirports.Clear();
                            foreach (var airport in unprocessedAirportsResult.Data)
                            {
                                this.UnprocessedAirports.Add(airport);
                            }
                        });
                }
                else
                {
                    this.RefreshViewCommand.ReportProgress(
                        () =>
                        {
                            Debug.WriteLine("Error refreshing unprocessed airports: " + unprocessedAirportsResult.Message);
                            if (!string.IsNullOrEmpty(unprocessedAirportsResult.ErrorDetails))
                            {
                                Debug.WriteLine(unprocessedAirportsResult.ErrorDetails);
                            }

                            var notification = new OpenSkyNotification("Error refreshing unprocessed airports", unprocessedAirportsResult.Message, MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                            notification.SetErrorColorStyle();
                            Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.ViewReference, this.RefreshViewCommand, "Error refreshing world statistics");
            }
            finally
            {
                this.LoadingText = null;
            }
        }
    }
}
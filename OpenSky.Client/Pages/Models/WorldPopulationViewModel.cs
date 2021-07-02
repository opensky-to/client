// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WorldPopulationViewModel.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Pages.Models
{
    using System;
    using System.Diagnostics;
    using System.Windows;

    using OpenSky.Client.MVVM;
    using OpenSky.Client.Tools;

    using OpenSkyApi;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// World population view model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 02/07/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class WorldPopulationViewModel : ViewModel
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="WorldPopulationViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 02/07/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public WorldPopulationViewModel()
        {
            this.RefreshOverviewCommand = new AsynchronousCommand(this.RefreshOverview);

            this.RefreshOverviewCommand.DoExecute(null);
        }
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The loading text.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string loadingText;
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
        /// Refreshes the world population overview.
        /// </summary>
        /// <remarks>
        /// sushi.at, 02/07/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void RefreshOverview()
        {
            this.LoadingText = "Refreshing world population overview";
            try
            {
                var result = OpenSkyService.Instance.GetWorldPopulationOverviewAsync().Result;
                if (!result.IsError)
                {
                    this.Overview = result.Data;
                }
                else
                {
                    this.RefreshOverviewCommand.ReportProgress(
                        () =>
                        {
                            Debug.WriteLine("Error refreshing world population overview: " + result.Message);
                            if (!string.IsNullOrEmpty(result.ErrorDetails))
                            {
                                Debug.WriteLine(result.ErrorDetails);
                            }

                            ModernWpf.MessageBox.Show(result.Message, "Error refreshing world population overview", MessageBoxButton.OK, MessageBoxImage.Error);
                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.RefreshOverviewCommand, "Error refreshing world population overview");
            }
            finally
            {
                this.LoadingText = null;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the refresh overview command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand RefreshOverviewCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The world population overview.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private WorldPopulationOverview overview;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the world population overview.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public WorldPopulationOverview Overview
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
    }
}
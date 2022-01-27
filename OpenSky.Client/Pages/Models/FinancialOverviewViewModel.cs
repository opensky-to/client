// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FinancialOverviewViewModel.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Pages.Models
{
    using System;
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
    /// Financial overview view model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 26/01/2022.
    /// </remarks>
    /// <seealso cref="T:OpenSky.Client.MVVM.ViewModel"/>
    /// -------------------------------------------------------------------------------------------------
    public class FinancialOverviewViewModel : ViewModel
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The loading text.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string loadingText;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The overview.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private FinancialOverview overview;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="FinancialOverviewViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 26/01/2022.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public FinancialOverviewViewModel()
        {
            // Create commands
            this.RefreshCommand = new AsynchronousCommand(this.Refresh);
        }

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
        /// Gets or sets the overview.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public FinancialOverview Overview
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
        /// Gets the refresh command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand RefreshCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Refresh the financial overview.
        /// </summary>
        /// <remarks>
        /// sushi.at, 26/01/2022.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void Refresh()
        {
            this.LoadingText = "Refreshing your fleet...";
            try
            {
                var result = OpenSkyService.Instance.GetFinancialOverviewAsync().Result;
                if (!result.IsError)
                {
                    this.Overview = result.Data;
                }
                else
                {
                    this.RefreshCommand.ReportProgress(
                        () =>
                        {
                            Debug.WriteLine("Error refreshing financial overview: " + result.Message);
                            if (!string.IsNullOrEmpty(result.ErrorDetails))
                            {
                                Debug.WriteLine(result.ErrorDetails);
                            }

                            var notification = new OpenSkyNotification("Error refreshing financial overview", result.Message, MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                            notification.SetErrorColorStyle();
                            Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.ViewReference, this.RefreshCommand, "Error refreshing financial overview");
            }
            finally
            {
                this.LoadingText = null;
            }
        }
    }
}
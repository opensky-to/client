// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MyFlightsViewModel.cs" company="OpenSky">
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

    using OpenSkyApi;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// My flights view model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 10/11/2021.
    /// </remarks>
    /// <seealso cref="T:OpenSky.Client.MVVM.ViewModel"/>
    /// -------------------------------------------------------------------------------------------------
    public class MyFlightsViewModel : ViewModel
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The loading text.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string loadingText;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The selected flight.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Flight selectedFlight;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="MyFlightsViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 10/11/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public MyFlightsViewModel()
        {
            // Initialize data structures
            this.Flights = new ObservableCollection<Flight>();

            // Create commands
            this.RefreshFlightsCommand = new AsynchronousCommand(this.RefreshFlights);
            this.ResumeFlightCommand = new AsynchronousCommand(this.ResumeFlight, false);
            this.AbortFlightCommand = new AsynchronousCommand(this.AbortFlight, false);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the abort flight command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand AbortFlightCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the flights.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<Flight> Flights { get; }

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
        /// Gets the refresh flights command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand RefreshFlightsCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the resume flight command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand ResumeFlightCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the selected flight.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Flight SelectedFlight
        {
            get => this.selectedFlight;

            set
            {
                if (Equals(this.selectedFlight, value))
                {
                    return;
                }

                this.selectedFlight = value;
                this.NotifyPropertyChanged();
                this.ResumeFlightCommand.CanExecute = value is { Paused: { } };
                this.AbortFlightCommand.CanExecute = value != null;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Abort the selected flight.
        /// </summary>
        /// <remarks>
        /// sushi.at, 15/11/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void AbortFlight()
        {
            if (this.SelectedFlight == null)
            {
                return;
            }

            MessageBoxResult? answer = MessageBoxResult.None;
            this.AbortFlightCommand.ReportProgress(
                () =>
                {
                    answer = ModernWpf.MessageBox.Show(
                        $"Are you sure you want to abort flight {this.SelectedFlight.FullFlightNumber}?\r\n\r\nAll progress will be lost and the flight be reverted back to the planning phase.",
                        "Abort flight?",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Hand);
                },
                true);

            if (answer != MessageBoxResult.Yes)
            {
                return;
            }

            this.LoadingText = $"Aborting flight {this.SelectedFlight.FullFlightNumber}...";
            try
            {
                var result = OpenSkyService.Instance.AbortFlightAsync(this.SelectedFlight.Id).Result;
                if (!result.IsError)
                {
                    this.AbortFlightCommand.ReportProgress(() => this.RefreshFlightsCommand.DoExecute(null));
                }
                else
                {
                    this.AbortFlightCommand.ReportProgress(
                        () =>
                        {
                            Debug.WriteLine("Error aborting flight: " + result.Message);
                            if (!string.IsNullOrEmpty(result.ErrorDetails))
                            {
                                Debug.WriteLine(result.ErrorDetails);
                            }

                            ModernWpf.MessageBox.Show(result.Message, "Error resuming flight", MessageBoxButton.OK, MessageBoxImage.Error);
                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.AbortFlightCommand, "Error aborting flight");
            }
            finally
            {
                this.LoadingText = null;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Refresh flights.
        /// </summary>
        /// <remarks>
        /// sushi.at, 10/11/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void RefreshFlights()
        {
            this.LoadingText = "Refreshing active flights...";
            try
            {
                var result = OpenSkyService.Instance.GetMyFlightsAsync().Result;
                if (!result.IsError)
                {
                    this.RefreshFlightsCommand.ReportProgress(
                        () =>
                        {
                            this.Flights.Clear();
                            foreach (var flight in result.Data)
                            {
                                this.Flights.Add(flight);
                            }
                        });
                }
                else
                {
                    this.RefreshFlightsCommand.ReportProgress(
                        () =>
                        {
                            Debug.WriteLine("Error refreshing active flights: " + result.Message);
                            if (!string.IsNullOrEmpty(result.ErrorDetails))
                            {
                                Debug.WriteLine(result.ErrorDetails);
                            }

                            ModernWpf.MessageBox.Show(result.Message, "Error refreshing active flights", MessageBoxButton.OK, MessageBoxImage.Error);
                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.RefreshFlightsCommand, "Error refreshing active flights");
            }
            finally
            {
                this.LoadingText = null;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Resume the selected flight.
        /// </summary>
        /// <remarks>
        /// sushi.at, 15/11/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void ResumeFlight()
        {
            if (this.SelectedFlight == null)
            {
                return;
            }

            this.LoadingText = $"Resuming flight {this.SelectedFlight.FullFlightNumber}...";
            try
            {
                var result = OpenSkyService.Instance.ResumeFlightAsync(this.SelectedFlight.Id).Result;
                if (!result.IsError)
                {
                    this.ResumeFlightCommand.ReportProgress(() => this.RefreshFlightsCommand.DoExecute(null));
                }
                else
                {
                    this.ResumeFlightCommand.ReportProgress(
                        () =>
                        {
                            Debug.WriteLine("Error resuming flight: " + result.Message);
                            if (!string.IsNullOrEmpty(result.ErrorDetails))
                            {
                                Debug.WriteLine(result.ErrorDetails);
                            }

                            ModernWpf.MessageBox.Show(result.Message, "Error resuming flight", MessageBoxButton.OK, MessageBoxImage.Error);
                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.ResumeFlightCommand, "Error resuming flight");
            }
            finally
            {
                this.LoadingText = null;
            }
        }
    }
}
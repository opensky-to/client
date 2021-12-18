// --------------------------------------------------------------------------------------------------------------------
// <copyright file="JobMarketViewModel.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Pages.Models
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Device.Location;
    using System.Diagnostics;
    using System.Linq;
    using System.Windows;
    using System.Windows.Media;

    using Microsoft.Maps.MapControl.WPF;

    using OpenSky.Client.Controls.Models;
    using OpenSky.Client.MVVM;
    using OpenSky.Client.Tools;

    using OpenSkyApi;

    using TomsToolbox.Essentials;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Job market view model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 13/12/2021.
    /// </remarks>
    /// <seealso cref="T:OpenSky.Client.MVVM.ViewModel"/>
    /// -------------------------------------------------------------------------------------------------
    public class JobMarketViewModel : ViewModel
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The airport icao.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string airportICAO;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The loading text.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string loadingText;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The selected job.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Job selectedJob;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="JobMarketViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 13/12/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public JobMarketViewModel()
        {
            this.Jobs = new ObservableCollection<Job>();
            this.AirportMarkers = new ObservableCollection<TrackingEventMarker>();
            this.JobTrails = new ObservableCollection<MapPolyline>();

            this.SearchJobsCommand = new AsynchronousCommand(this.SearchJobs, false);
            this.ClearSelectionCommand = new Command(this.ClearSelection);
            this.AcceptJobCommand = new AsynchronousCommand(this.AcceptJob, false);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the accept job command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand AcceptJobCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the airport icao.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string AirportICAO
        {
            get => this.airportICAO;

            set
            {
                if (Equals(this.airportICAO, value))
                {
                    return;
                }

                this.airportICAO = value;
                this.NotifyPropertyChanged();
                this.SearchJobsCommand.CanExecute = !string.IsNullOrEmpty(value);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the tracking event markers.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<TrackingEventMarker> AirportMarkers { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the clear selection command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command ClearSelectionCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the jobs.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<Job> Jobs { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the job trails.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<MapPolyline> JobTrails { get; }

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
        /// Gets the search jobs command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand SearchJobsCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the selected job.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Job SelectedJob
        {
            get => this.selectedJob;

            set
            {
                if (Equals(this.selectedJob, value) && (this.Jobs.Count == 0 || value != null))
                {
                    return;
                }

                this.selectedJob = value;
                this.NotifyPropertyChanged();
                this.AcceptJobCommand.CanExecute = value != null;

                this.JobTrails.RemoveRange(this.JobTrails.ToList());
                this.AirportMarkers.RemoveRange(this.AirportMarkers.ToList());

                var airportPackage = AirportPackageClientHandler.GetPackage();
                if (airportPackage != null)
                {
                    if (value != null)
                    {
                        // Show selected
                        var origin = airportPackage.Airports.SingleOrDefault(a => a.ICAO == value.OriginICAO);
                        var destinations = airportPackage.Airports.Where(a => value.Payloads.Select(p => p.DestinationICAO).Contains(a.ICAO));

                        if (origin != null)
                        {
                            this.AirportMarkers.Add(new TrackingEventMarker(new GeoCoordinate(origin.Latitude, origin.Longitude), origin.ICAO, Colors.DarkGray, Colors.Black));
                            this.AirportMarkers.Add(new TrackingEventMarker(origin, Colors.DarkGray, Colors.Black));
                            foreach (var runway in origin.Runways)
                            {
                                this.AirportMarkers.Add(new TrackingEventMarker(runway));
                            }
                        }

                        var destinationsAdded = new List<string>();
                        foreach (var destination in destinations)
                        {
                            if (!destinationsAdded.Contains(destination.ICAO))
                            {
                                destinationsAdded.Add(destination.ICAO);
                                this.AirportMarkers.Add(new TrackingEventMarker(new GeoCoordinate(destination.Latitude, destination.Longitude), destination.ICAO, OpenSkyColors.OpenSkyTeal, Colors.White));
                                this.AirportMarkers.Add(new TrackingEventMarker(destination, OpenSkyColors.OpenSkyTeal, Colors.White));
                                foreach (var runway in destination.Runways)
                                {
                                    this.AirportMarkers.Add(new TrackingEventMarker(runway));
                                }

                                if (origin != null)
                                {
                                    this.JobTrails.Add(
                                        new MapPolyline
                                        {
                                            Stroke = new SolidColorBrush(OpenSkyColors.OpenSkyTeal),
                                            StrokeThickness = 4,
                                            Locations = new LocationCollection
                                            {
                                                new(origin.Latitude, origin.Longitude),
                                                new(destination.Latitude, destination.Longitude)
                                            }
                                        });
                                }
                            }
                        }
                    }
                    else
                    {
                        // Show all
                        var originsAdded = new Dictionary<string, List<TrackingEventMarker>>();
                        var destinationsAdded = new Dictionary<string, List<TrackingEventMarker>>();
                        foreach (var job in this.Jobs)
                        {
                            var origin = airportPackage.Airports.SingleOrDefault(a => a.ICAO == job.OriginICAO);
                            var destinations = airportPackage.Airports.Where(a => job.Payloads.Select(p => p.DestinationICAO).Contains(a.ICAO));

                            if (origin != null && !originsAdded.ContainsKey(origin.ICAO))
                            {
                                var originMarkers = new List<TrackingEventMarker>
                                {
                                    new(new GeoCoordinate(origin.Latitude, origin.Longitude), origin.ICAO, Colors.DarkGray, Colors.Black),
                                    new(origin, Colors.DarkGray, Colors.Black)
                                };
                                originMarkers.AddRange(origin.Runways.Select(runway => new TrackingEventMarker(runway)));
                                originsAdded.Add(origin.ICAO, originMarkers);
                            }

                            foreach (var destination in destinations)
                            {
                                if (!destinationsAdded.ContainsKey(destination.ICAO))
                                {
                                    var destinationMarkers = new List<TrackingEventMarker>
                                    {
                                        new(new GeoCoordinate(destination.Latitude, destination.Longitude), destination.ICAO, OpenSkyColors.OpenSkyTeal, Colors.White),
                                        new(destination, OpenSkyColors.OpenSkyTeal, Colors.White)
                                    };
                                    destinationMarkers.AddRange(destination.Runways.Select(runway => new TrackingEventMarker(runway)));
                                    destinationsAdded.Add(destination.ICAO, destinationMarkers);

                                    if (origin != null)
                                    {
                                        this.JobTrails.Add(
                                            new MapPolyline
                                            {
                                                Stroke = new SolidColorBrush(OpenSkyColors.OpenSkyTeal),
                                                StrokeThickness = 4,
                                                Locations = new LocationCollection
                                                {
                                                    new(origin.Latitude, origin.Longitude),
                                                    new(destination.Latitude, destination.Longitude)
                                                }
                                            });
                                    }
                                }
                            }
                        }

                        foreach (var destination in destinationsAdded)
                        {
                            foreach (var marker in destination.Value)
                            {
                                this.AirportMarkers.Add(marker);
                            }
                        }

                        foreach (var origin in originsAdded.Where(origin => !destinationsAdded.ContainsKey(origin.Key)))
                        {
                            foreach (var marker in origin.Value)
                            {
                                this.AirportMarkers.Add(marker);
                            }
                        }
                    }
                }
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Accept the selected job.
        /// </summary>
        /// <remarks>
        /// sushi.at, 18/12/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void AcceptJob()
        {
            if (this.SelectedJob == null)
            {
                return;
            }

            this.LoadingText = "Accepting job...";
            try
            {
                var result = OpenSkyService.Instance.AcceptJobAsync(this.SelectedJob.Id, false).Result;
                if (!result.IsError)
                {
                    this.AcceptJobCommand.ReportProgress(()=>ModernWpf.MessageBox.Show(result.Message));
                }
                else
                {
                    this.AcceptJobCommand.ReportProgress(
                        () =>
                        {
                            Debug.WriteLine("Error accepting job: " + result.Message);
                            if (!string.IsNullOrEmpty(result.ErrorDetails))
                            {
                                Debug.WriteLine(result.ErrorDetails);
                            }

                            ModernWpf.MessageBox.Show(result.Message, "Error accepting job", MessageBoxButton.OK, MessageBoxImage.Error);
                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.AcceptJobCommand, "Error accepting job");
            }
            finally
            {
                this.LoadingText = null;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Clears the job selection.
        /// </summary>
        /// <remarks>
        /// sushi.at, 14/12/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void ClearSelection()
        {
            this.SelectedJob = null;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Searches for jobs.
        /// </summary>
        /// <remarks>
        /// sushi.at, 13/12/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void SearchJobs()
        {
            this.LoadingText = "Searching for jobs...";
            try
            {
                var result = OpenSkyService.Instance.GetJobsAtAirportAsync(this.AirportICAO).Result;
                if (!result.IsError)
                {
                    this.SearchJobsCommand.ReportProgress(
                        () =>
                        {
                            this.Jobs.Clear();
                            foreach (var job in result.Data)
                            {
                                this.Jobs.Add(job);
                            }

                            this.SelectedJob = null;
                        });
                }
                else
                {
                    this.SearchJobsCommand.ReportProgress(
                        () =>
                        {
                            Debug.WriteLine("Error searching for jobs: " + result.Message);
                            if (!string.IsNullOrEmpty(result.ErrorDetails))
                            {
                                Debug.WriteLine(result.ErrorDetails);
                            }

                            ModernWpf.MessageBox.Show(result.Message, "Error searching for jobs", MessageBoxButton.OK, MessageBoxImage.Error);
                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.SearchJobsCommand, "Error searching for jobs");
            }
            finally
            {
                this.LoadingText = null;
            }
        }
    }
}
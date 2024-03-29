﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MyJobsViewModel.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Pages.Models
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Device.Location;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Windows;
    using System.Windows.Media;

    using Microsoft.Maps.MapControl.WPF;

    using OpenSky.Client.Controls;
    using OpenSky.Client.Controls.Models;
    using OpenSky.Client.Converters;
    using OpenSky.Client.MVVM;
    using OpenSky.Client.Tools;
    using OpenSky.Client.Views;
    using OpenSky.Client.Views.Models;

    using OpenSkyApi;

    using TomsToolbox.Essentials;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// My jobs view model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 18/12/2021.
    /// </remarks>
    /// <seealso cref="T:OpenSky.Client.MVVM.ViewModel"/>
    /// -------------------------------------------------------------------------------------------------
    public class MyJobsViewModel : ViewModel
    {
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
        /// Initializes a new instance of the <see cref="MyJobsViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 18/12/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public MyJobsViewModel()
        {
            // Initialize data structures
            this.Jobs = new ObservableCollection<Job>();
            this.AirportMarkers = new ObservableCollection<TrackingEventMarker>();
            this.JobTrails = new ObservableCollection<MapPolyline>();

            // Create commands
            this.RefreshJobsCommand = new AsynchronousCommand(this.RefreshJobs);
            this.AbortJobCommand = new AsynchronousCommand(this.AbortJob, false);
            this.PlanFlightCommand = new Command(this.PlanFlight, false);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the abort job command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand AbortJobCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the tracking event markers.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<TrackingEventMarker> AirportMarkers { get; }

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
        /// Gets the plan flight command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command PlanFlightCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the refresh jobs command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand RefreshJobsCommand { get; }

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
                this.AbortJobCommand.CanExecute = value != null;
                this.PlanFlightCommand.CanExecute = value != null;

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
        /// Abort the selected job.
        /// </summary>
        /// <remarks>
        /// sushi.at, 18/12/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void AbortJob()
        {
            if (this.SelectedJob == null)
            {
                return;
            }

            var jobTypeConverter = new JobTypeConverter();
            ExtendedMessageBoxResult? answer = null;
            this.AbortJobCommand.ReportProgress(
                () =>
                {
                    var messageBox = new OpenSkyMessageBox(
                        "Abort job?",
                        $"Are you sure you want to abort the {jobTypeConverter.Convert(this.SelectedJob.Type, typeof(string), null, CultureInfo.CurrentCulture)} job from {this.SelectedJob.OriginICAO}?\r\n\r\nYou will be charged a 30 % penalty for doing so.",
                        MessageBoxButton.YesNo,
                        ExtendedMessageBoxImage.Hand);
                    messageBox.SetWarningColorStyle();
                    messageBox.Closed += (_, _) => { answer = messageBox.Result; };
                    Main.ShowMessageBoxInSaveViewAs(this.ViewReference, messageBox);
                });
            while (answer == null && !SleepScheduler.IsShutdownInProgress)
            {
                Thread.Sleep(500);
            }

            if (answer != ExtendedMessageBoxResult.Yes)
            {
                return;
            }

            this.LoadingText = "Aborting job...";
            try
            {
                var result = OpenSkyService.Instance.AbortJobAsync(this.SelectedJob.Id).Result;
                if (!result.IsError)
                {
                    this.AbortJobCommand.ReportProgress(() => this.RefreshJobsCommand.DoExecute(null));
                }
                else
                {
                    this.AbortJobCommand.ReportProgress(
                        () =>
                        {
                            Debug.WriteLine("Error aborting job: " + result.Message);
                            if (!string.IsNullOrEmpty(result.ErrorDetails))
                            {
                                Debug.WriteLine(result.ErrorDetails);
                            }

                            var notification = new OpenSkyNotification("Error aborting job", result.Message, MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                            notification.SetErrorColorStyle();
                            Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.ViewReference, this.AbortJobCommand, "Error aborting job");
            }
            finally
            {
                this.LoadingText = null;
            }
        }

        // Create a flight plan for the selected job.
        private void PlanFlight()
        {
            if (this.SelectedJob != null)
            {
                var flightNumber = new Random().Next(1, 9999);
                var destination = !string.IsNullOrEmpty(this.SelectedJob.Destinations) && !this.SelectedJob.Destinations.Contains(",") ? this.SelectedJob.Destinations : null;
                var navMenuItem = new NavMenuItem
                {
                    Icon = "/Resources/plan16.png", PageType = typeof(Pages.FlightPlan), Name = $"New flight plan {flightNumber}",
                    Parameter = new FlightPlan
                    {
                        Id = Guid.NewGuid(), FlightNumber = flightNumber, PlannedDepartureTime = DateTime.UtcNow.AddMinutes(30).RoundUp(TimeSpan.FromMinutes(5)), IsNewFlightPlan = true, OriginICAO = this.SelectedJob.OriginICAO,
                        DestinationICAO = destination
                    }
                };
                Main.ActivateNavMenuItemInSameViewAs(this.ViewReference, navMenuItem);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Refresh my jobs.
        /// </summary>
        /// <remarks>
        /// sushi.at, 18/12/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void RefreshJobs()
        {
            this.LoadingText = "Refreshing jobs...";
            try
            {
                var result = OpenSkyService.Instance.GetMyJobsAsync().Result;
                if (!result.IsError)
                {
                    this.RefreshJobsCommand.ReportProgress(
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
                    this.RefreshJobsCommand.ReportProgress(
                        () =>
                        {
                            Debug.WriteLine("Error refreshing jobs: " + result.Message);
                            if (!string.IsNullOrEmpty(result.ErrorDetails))
                            {
                                Debug.WriteLine(result.ErrorDetails);
                            }

                            var notification = new OpenSkyNotification("Error refreshing jobs", result.Message, MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                            notification.SetErrorColorStyle();
                            Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.ViewReference, this.RefreshJobsCommand, "Error refreshing jobs");
            }
            finally
            {
                this.LoadingText = null;
            }
        }
    }
}
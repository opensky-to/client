// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FlightPlanViewModel.Aircraft.cs" company="OpenSky">
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
    using System.Linq;
    using System.Windows;

    using OpenSky.Client.Controls;
    using OpenSky.Client.Controls.Models;
    using OpenSky.Client.MVVM;
    using OpenSky.Client.Tools;
    using OpenSky.Client.Views;

    using OpenSkyApi;

    using TomsToolbox.Essentials;

    /// -------------------------------------------------------------------------------------------------
    /// <content>
    /// Flight plan view model - Aircraft.
    /// </content>
    /// -------------------------------------------------------------------------------------------------
    public partial class FlightPlanViewModel
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The selected aircraft.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Aircraft selectedAircraft;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the aircraft list.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<Aircraft> Aircraft { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the clear aircraft command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command ClearAircraftCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the refresh aircraft command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand RefreshAircraftCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the selected aircraft.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Aircraft SelectedAircraft
        {
            get => this.selectedAircraft;

            set
            {
                if (Equals(this.selectedAircraft, value))
                {
                    return;
                }

                this.selectedAircraft = value;
                if (value != null)
                {
                    this.FuelGallons = value.Fuel;
                }
                else
                {
                    this.FuelGallons = null;
                }

                this.NotifyPropertyChanged();
                this.IsDirty = true;
                this.NotifyPropertyChanged(nameof(this.CrewWeight));
                this.NotifyPropertyChanged(nameof(this.ZeroFuelWeight));
                this.NotifyPropertyChanged(nameof(this.GrossWeight));
                this.NotifyPropertyChanged(nameof(this.MaxFuelWeight));
                this.NotifyPropertyChanged(nameof(this.FuelPricePerGallon));
                this.NotifyPropertyChanged(nameof(this.FuelPrice));
                this.NotifyPropertyChanged(nameof(this.AlternateAirportFuelWarningVisibility));
                this.NotifyPropertyChanged(nameof(this.AlternateAirportShortRunwayWarningVisibility));
                this.NotifyPropertyChanged(nameof(this.AlternateAirportShortRunwayErrorVisibility));
                this.NotifyPropertyChanged(nameof(this.DestinationAirportFuelWarningVisibility));
                this.NotifyPropertyChanged(nameof(this.DestinationAirportShortRunwayWarningVisibility));
                this.NotifyPropertyChanged(nameof(this.DestinationAirportShortRunwayErrorVisibility));
                this.NotifyPropertyChanged(nameof(this.OriginWrongSimErrorVisibility));
                this.NotifyPropertyChanged(nameof(this.DestinationWrongSimErrorVisibility));
                this.NotifyPropertyChanged(nameof(this.AlternateWrongSimErrorVisibility));

                UpdateGUIDelegate updateAircraftRelated = this.UpdatePlannablePayloads;
                Application.Current.Dispatcher.BeginInvoke(updateAircraftRelated);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Clears the selected aircraft.
        /// </summary>
        /// <remarks>
        /// sushi.at, 31/10/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void ClearAircraft()
        {
            this.SelectedAircraft = null;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Refreshes the list of available aircraft.
        /// </summary>
        /// <remarks>
        /// sushi.at, 31/10/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void RefreshAircraft()
        {
            this.LoadingText = "Refreshing aircraft...";
            try
            {
                var currentSelection = this.SelectedAircraft;
                var currentFuel = this.FuelGallons;
                var result = OpenSkyService.Instance.GetMyAircraftAsync().Result;
                if (!result.IsError)
                {
                    this.RefreshAircraftCommand.ReportProgress(
                        () =>
                        {
                            this.Aircraft.Clear();
                            this.Aircraft.AddRange(result.Data.OrderBy(a => a.Registry));
                            this.UpdateAircraftDistances();

                            if (currentSelection != null && this.Aircraft.Contains(currentSelection))
                            {
                                this.SelectedAircraft = currentSelection;
                                this.FuelGallons = currentFuel;
                                this.IsDirty = false;
                            }
                        });
                }
                else
                {
                    this.RefreshAircraftCommand.ReportProgress(
                        () =>
                        {
                            Debug.WriteLine("Error refreshing aircraft: " + result.Message);
                            if (!string.IsNullOrEmpty(result.ErrorDetails))
                            {
                                Debug.WriteLine(result.ErrorDetails);
                            }

                            var notification = new OpenSkyNotification("Error refreshing aircraft", result.Message, MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                            notification.SetErrorColorStyle();
                            Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.ViewReference, this.RefreshAircraftCommand, "Error refreshing aircraft");
            }
            finally
            {
                this.LoadingText = null;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Update aircraft distances when origin airport changes.
        /// </summary>
        /// <remarks>
        /// sushi.at, 18/12/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void UpdateAircraftDistances()
        {
            var currentSelection = this.SelectedAircraft;
            var currentAircraft = new List<Aircraft>(this.Aircraft.Where(a => a.Registry != "----"));
            this.Aircraft.Clear();
            if (string.IsNullOrEmpty(this.OriginICAO))
            {
                foreach (var aircraft in currentAircraft.OrderBy(a => a.Registry))
                {
                    aircraft.Distance = 0;
                    this.Aircraft.Add(aircraft);
                }
            }
            else
            {
                // Origin is already set, put the aircraft at that airport first
                var airportPackage = AirportPackageClientHandler.GetPackage();
                var origin = airportPackage?.Airports.SingleOrDefault(a => a.ICAO == this.OriginICAO);
                if (origin != null)
                {
                    this.Aircraft.Add(new Aircraft { Registry = "----", Type = new AircraftType { Name = $" Aircraft at {this.OriginICAO} ----" } });
                }

                foreach (var aircraft in currentAircraft.Where(a => a.AirportICAO.Equals(this.OriginICAO, StringComparison.InvariantCultureIgnoreCase)).OrderBy(a => a.Registry))
                {
                    aircraft.Distance = 0;
                    this.Aircraft.Add(aircraft);
                }

                if (origin != null)
                {
                    this.Aircraft.Add(new Aircraft { Registry = "----", Type = new AircraftType { Name = " Aircraft at other airports ----" } });
                }

                var atOtherAirports = new List<Aircraft>();
                foreach (var aircraft in currentAircraft.Where(a => !a.AirportICAO.Equals(this.OriginICAO, StringComparison.InvariantCultureIgnoreCase)))
                {
                    aircraft.Distance = 0;
                    if (origin != null)
                    {
                        var aircraftAirport = airportPackage.Airports.SingleOrDefault(a => a.ICAO == aircraft.AirportICAO);
                        if (aircraftAirport != null)
                        {
                            aircraft.Distance = (int)(new GeoCoordinate(origin.Latitude, origin.Longitude).GetDistanceTo(new GeoCoordinate(aircraftAirport.Latitude, aircraftAirport.Longitude)) / 1852.0);
                        }
                    }

                    atOtherAirports.Add(aircraft);
                }

                this.Aircraft.AddRange(atOtherAirports.OrderBy(a => a.Distance).ThenBy(a => a.Registry));
            }

            if (currentSelection != null && this.Aircraft.Contains(currentSelection))
            {
                this.SelectedAircraft = currentSelection;
            }
        }
    }
}
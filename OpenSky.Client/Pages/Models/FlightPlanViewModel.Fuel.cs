// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FlightPlanViewModel.Fuel.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Pages.Models
{
    using System.Windows;

    using OpenSkyApi;

    /// -------------------------------------------------------------------------------------------------
    /// <content>
    /// Flight plan view model - Fuel.
    /// </content>
    /// -------------------------------------------------------------------------------------------------
    public partial class FlightPlanViewModel
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The fuel dump warning visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Visibility fuelDumpWarningVisibility = Visibility.Collapsed;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The fuel gallons.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private double? fuelGallons;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the fuel dump warning visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Visibility FuelDumpWarningVisibility
        {
            get => this.fuelDumpWarningVisibility;

            set
            {
                if (Equals(this.fuelDumpWarningVisibility, value))
                {
                    return;
                }

                this.fuelDumpWarningVisibility = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the fuel gallons.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double? FuelGallons
        {
            get => this.fuelGallons;

            set
            {
                if (Equals(this.fuelGallons, value))
                {
                    return;
                }

                if (value.HasValue && value > (this.SelectedAircraft?.Type.FuelTotalCapacity ?? 0))
                {
                    value = this.SelectedAircraft?.Type.FuelTotalCapacity ?? 0;
                }

                this.fuelGallons = value;
                this.NotifyPropertyChanged();
                this.IsDirty = true;
                this.NotifyPropertyChanged(nameof(this.FuelWeight));
                this.NotifyPropertyChanged(nameof(this.GrossWeight));
                this.NotifyPropertyChanged(nameof(this.FuelPrice));
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the fuel price.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int FuelPrice
        {
            get
            {
                if (this.SelectedAircraft != null && this.FuelGallons.HasValue)
                {
                    if (this.FuelGallons.Value < this.SelectedAircraft.Fuel)
                    {
                        this.FuelDumpWarningVisibility = Visibility.Visible;
                        return 0;
                    }

                    this.FuelDumpWarningVisibility = Visibility.Collapsed;
                    var gallons = this.FuelGallons.Value - this.SelectedAircraft.Fuel;
                    return (int)(gallons * this.FuelPricePerGallon);
                }

                return 0;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the fuel price per gallon.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double FuelPricePerGallon
        {
            get
            {
                if (this.SelectedAircraft != null && this.OriginAirport != null)
                {
                    return this.SelectedAircraft.Type.FuelType switch
                    {
                        FuelType.AvGas => this.OriginAirport.AvGasPrice,
                        FuelType.JetFuel => this.OriginAirport.JetFuelPrice,
                        _ => 0
                    };
                }

                return 0;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the fuel weight in lbs.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double FuelWeight => (this.FuelGallons ?? 0) * (this.SelectedAircraft?.Type.FuelWeightPerGallon ?? 0);

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the maximum fuel weight in lbs.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double MaxFuelWeight => (this.SelectedAircraft?.Type.FuelTotalCapacity ?? 0) * (this.SelectedAircraft?.Type.FuelWeightPerGallon ?? 0);
    }
}
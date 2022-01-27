// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GroundOperations.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace

namespace OpenSkyApi
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows;

    using JetBrains.Annotations;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Ground operations extensions.
    /// </summary>
    /// <remarks>
    /// sushi.at, 26/01/2022.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public partial class GroundOperations : INotifyPropertyChanged
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The fuel dump warning visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Visibility fuelDumpWarningVisibility = Visibility.Collapsed;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The fuel price per gallon.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private double fuelPricePerGallon;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The fuel weight.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private double fuelWeight;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The fuel weight per gallon.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private double fuelWeightPerGallon;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The selected fuel.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private double selectedFuel;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public event PropertyChangedEventHandler PropertyChanged;

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
                this.OnPropertyChanged();
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
                if (this.SelectedFuel < this.Fuel)
                {
                    this.FuelDumpWarningVisibility = Visibility.Visible;
                    return 0;
                }

                this.FuelDumpWarningVisibility = Visibility.Collapsed;
                var gallons = this.SelectedFuel - this.Fuel;
                return (int)(gallons * this.FuelPricePerGallon);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the fuel price per gallon.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double FuelPricePerGallon
        {
            get => this.fuelPricePerGallon;

            set
            {
                if (Equals(this.fuelPricePerGallon, value))
                {
                    return;
                }

                this.fuelPricePerGallon = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.FuelPrice));
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the fuel weight.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double FuelWeight
        {
            get => this.fuelWeight;

            set
            {
                if (Equals(this.fuelWeight, value))
                {
                    return;
                }

                this.fuelWeight = value;
                this.OnPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the fuel weight per gallon.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double FuelWeightPerGallon
        {
            get => this.fuelWeightPerGallon;

            set
            {
                if (Equals(this.fuelWeightPerGallon, value))
                {
                    return;
                }

                this.fuelWeightPerGallon = value;
                this.OnPropertyChanged();

                this.FuelWeight = this.FuelWeightPerGallon * this.SelectedFuel;
                this.OnPropertyChanged(nameof(this.FuelWeight));
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the selected fuel.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double SelectedFuel
        {
            get => this.selectedFuel;

            set
            {
                if (Equals(this.selectedFuel, value))
                {
                    return;
                }

                this.selectedFuel = value;
                this.OnPropertyChanged();

                this.FuelWeight = this.FuelWeightPerGallon * this.SelectedFuel;
                this.OnPropertyChanged(nameof(this.FuelWeight));
                this.OnPropertyChanged(nameof(this.FuelPrice));
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Executes the property changed action.
        /// </summary>
        /// <remarks>
        /// sushi.at, 26/01/2022.
        /// </remarks>
        /// <param name="propertyName">
        /// (Optional) Name of the property.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
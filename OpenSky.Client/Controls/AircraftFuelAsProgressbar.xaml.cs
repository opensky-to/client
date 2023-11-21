// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AircraftFuelAsProgressbar.xaml.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Controls
{
    using System.ComponentModel;
    using System.Windows;

    using OpenSkyApi;

    /// -------------------------------------------------------------------------------------------------
    /// <content>
    /// Fuel (current vs. max) displayed as progress bar, with optional display of numeric values.
    /// </content>
    /// -------------------------------------------------------------------------------------------------
    public partial class AircraftFuelAsProgressbar
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The aircraft property.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static readonly DependencyProperty AircraftProperty = DependencyProperty.Register("Aircraft", typeof(Aircraft), typeof(AircraftFuelAsProgressbar));

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The show numeric values property.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static readonly DependencyProperty NumericValuesVisibilityProperty = DependencyProperty.Register("NumericValuesVisibility", typeof(Visibility), typeof(AircraftFuelAsProgressbar), new UIPropertyMetadata(Visibility.Visible));

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="AircraftFuelAsProgressbar"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 26/07/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public AircraftFuelAsProgressbar()
        {
            this.InitializeComponent();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the aircraft.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Bindable(true)]
        public Aircraft Aircraft
        {
            get => (Aircraft)this.GetValue(AircraftProperty);
            set => this.SetValue(AircraftProperty, value);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the numeric values visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Bindable(true)]
        public Visibility NumericValuesVisibility
        {
            get => (Visibility)this.GetValue(NumericValuesVisibilityProperty);
            set => this.SetValue(NumericValuesVisibilityProperty, value);
        }
    }
}
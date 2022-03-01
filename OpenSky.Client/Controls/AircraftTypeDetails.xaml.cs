// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AircraftTypeDetails.xaml.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Controls
{
    using System;
    using System.ComponentModel;
    using System.Windows;

    using OpenSky.Client.Controls.Models;

    using OpenSkyApi;

    /// -------------------------------------------------------------------------------------------------
    /// <content>
    /// Aircraft type details user control.
    /// </content>
    /// -------------------------------------------------------------------------------------------------
    public partial class AircraftTypeDetails
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The aircraft type property.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static readonly DependencyProperty AircraftTypeProperty = DependencyProperty.Register("AircraftType", typeof(AircraftType), typeof(AircraftTypeDetails));

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="AircraftTypeDetails"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 16/02/2022.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public AircraftTypeDetails()
        {
            this.InitializeComponent();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the aircraft type.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------

        [Bindable(true)]
        public AircraftType AircraftType
        {
            get => (AircraftType)this.GetValue(AircraftTypeProperty);
            set => this.SetValue(AircraftTypeProperty, value);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Invoked whenever the effective value of any dependency property on this
        /// <see cref="T:System.Windows.FrameworkElement" /> has been updated. The specific dependency
        /// property that changed is reported in the arguments parameter. Overrides
        /// <see cref="M:System.Windows.DependencyObject.OnPropertyChanged(System.Windows.DependencyPropertyChangedEventArgs)" />.
        /// 
        /// </summary>
        /// <remarks>
        /// sushi.at, 17/02/2022.
        /// </remarks>
        /// <param name="e">
        /// The event data that describes the property that changed, as well as old and new values.
        /// </param>
        /// <seealso cref="System.Windows.FrameworkElement.OnPropertyChanged(DependencyPropertyChangedEventArgs)"/>
        /// -------------------------------------------------------------------------------------------------
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property == AircraftTypeProperty && this.DataContext is AircraftTypeDetailsViewModel viewModel)
            {
                if (e.NewValue is AircraftType type && type.Id != Guid.Empty)
                {
                    viewModel.Type = type;
                    this.GroupBoxHeader.InvalidateMeasure();
                    this.EngineInfo.InvalidateMeasure();
                    this.ImageTags.InvalidateMeasure();
                }
                else
                {
                    viewModel.Type = null;
                    this.GroupBoxHeader.InvalidateMeasure();
                    this.EngineInfo.InvalidateMeasure();
                    this.ImageTags.InvalidateMeasure();
                }
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Aircraft type details on loaded.
        /// </summary>
        /// <remarks>
        /// sushi.at, 20/02/2022.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Routed event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void AircraftTypeDetailsOnLoaded(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is AircraftTypeDetailsViewModel viewModel)
            {
                viewModel.ViewReference = this;
            }
        }
    }
}
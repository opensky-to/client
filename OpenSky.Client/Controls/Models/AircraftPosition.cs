// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AircraftPosition.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Controls.Models
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Media;

    using JetBrains.Annotations;

    using Microsoft.Maps.MapControl.WPF;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Aircraft position (image that moves and rotates)
    /// </summary>
    /// <remarks>
    /// sushi.at, 18/11/2021.
    /// </remarks>
    /// <seealso cref="T:System.Windows.Controls.Image"/>
    /// <seealso cref="T:System.ComponentModel.INotifyPropertyChanged"/>
    /// -------------------------------------------------------------------------------------------------
    public class AircraftPosition : Image, INotifyPropertyChanged
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The heading.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private double heading;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The location.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Location location;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The aircraft registry.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string registry;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="AircraftPosition"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 18/11/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public AircraftPosition()
        {
            this.SetValue(Panel.ZIndexProperty, 999);
            this.SetValue(MapLayer.PositionOriginProperty, PositionOrigin.Center);

            this.Width = 20;
            this.Height = 20;

            var rotateTransform = new RotateTransform { CenterX = 10, CenterY = 10 };
            var headingBinding = new Binding { Source = this, Path = new PropertyPath("Heading"), Mode = BindingMode.OneWay };
            BindingOperations.SetBinding(rotateTransform, RotateTransform.AngleProperty, headingBinding);
            this.RenderTransform = rotateTransform;
            var aircraftDrawingImage = this.FindResource("OpenSkyLogoPointingUpForMap") as DrawingImage;
            this.Source = aircraftDrawingImage;
            var positionBinding = new Binding { Source = this, Path = new PropertyPath("Location"), Mode = BindingMode.OneWay };
            BindingOperations.SetBinding(this, MapLayer.PositionProperty, positionBinding);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public event PropertyChangedEventHandler PropertyChanged;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the heading.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double Heading
        {
            get => this.heading;

            set
            {
                if (Equals(this.heading, value))
                {
                    return;
                }

                this.heading = value;
                this.OnPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the location.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Location Location
        {
            get => this.location;

            set
            {
                if (Equals(this.location, value))
                {
                    return;
                }

                this.location = value;
                this.OnPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the aircraft registry.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string Registry
        {
            get => this.registry;

            set
            {
                if (Equals(this.registry, value))
                {
                    return;
                }

                this.registry = value;
                this.OnPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Executes the property changed action.
        /// </summary>
        /// <remarks>
        /// sushi.at, 18/11/2021.
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
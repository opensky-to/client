// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SimbriefWaypointMarker.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Controls.Models
{
    using System.ComponentModel;
    using System.Device.Location;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Media;
    using System.Windows.Shapes;
    using System.Xml.Linq;

    using JetBrains.Annotations;

    using Microsoft.Maps.MapControl.WPF;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Simbrief waypoint marker.
    /// </summary>
    /// <remarks>
    /// sushi.at, 22/03/2021.
    /// </remarks>
    /// <seealso cref="T:System.Windows.Controls.Canvas"/>
    /// -------------------------------------------------------------------------------------------------
    public class SimbriefWaypointMarker : Canvas
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The text label visible property.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static readonly DependencyProperty TextLabelVisibleProperty = DependencyProperty.Register("TextLabelVisible", typeof(Visibility), typeof(SimbriefWaypointMarker), new UIPropertyMetadata(Visibility.Visible));

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="SimbriefWaypointMarker"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 22/03/2021.
        /// </remarks>
        /// <param name="lat">
        /// The latitude of the waypoint.
        /// </param>
        /// <param name="lon">
        /// The longitude of the waypoint.
        /// </param>
        /// <param name="name">
        /// The name of the waypoint.
        /// </param>
        /// <param name="type">
        /// The type of the waypoint.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public SimbriefWaypointMarker(double lat, double lon, string name, string type)
        {
            this.Width = 60;
            this.Height = 60;

            this.Children.Add(
                new Ellipse
                {
                    Width = 8,
                    Height = 8,
                    Fill = new SolidColorBrush(OpenSkyColors.OpenSkySimBrief),
                    Margin = new Thickness(26, 26, 0, 0)
                });

            var textBorder = new Border { BorderBrush = null, Background = new SolidColorBrush(OpenSkyColors.OpenSkySimBrief), CornerRadius = new CornerRadius(1.5), Margin = type != "wpt" ? new Thickness(0, 0, 0, 45) : new Thickness(0, 40, 0, 0) };
            var visibilityBinding = new Binding { Source = this, Path = new PropertyPath("TextLabelVisible"), Mode = BindingMode.OneWay };
            BindingOperations.SetBinding(textBorder, VisibilityProperty, visibilityBinding);
            this.Children.Add(textBorder);

            textBorder.Child = new TextBlock
            {
                Text = name,
                Foreground = new SolidColorBrush(OpenSkyColors.OpenSkySimBriefText),

                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Padding = new Thickness(3),
                FontSize = 11,
                Margin = new Thickness(3)
            };

            MapLayer.SetPosition(this, new Location(lat, lon));
            MapLayer.SetPositionOrigin(this, PositionOrigin.Center);

            this.GeoCoordinate = new GeoCoordinate(lat, lon);
            this.WaypointName = name;
            this.WaypointType = type;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="SimbriefWaypointMarker"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 01/04/2021.
        /// </remarks>
        /// <param name="waypointFromSave">
        /// The waypoint to restore from a save file.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public SimbriefWaypointMarker(XElement waypointFromSave)
        {
            var lat = double.Parse(waypointFromSave.Attribute("Lat")?.Value ?? "missing");
            var lon = double.Parse(waypointFromSave.Attribute("Lon")?.Value ?? "missing");
            var name = waypointFromSave.Attribute("Name")?.Value;
            var type = waypointFromSave.Attribute("Name")?.Value;

            this.Width = 60;
            this.Height = 60;

            this.Children.Add(
                new Ellipse
                {
                    Width = 8,
                    Height = 8,
                    Fill = new SolidColorBrush(OpenSkyColors.OpenSkySimBrief),
                    Margin = new Thickness(26, 26, 0, 0)
                });

            var textBorder = new Border { BorderBrush = null, Background = new SolidColorBrush(OpenSkyColors.OpenSkySimBrief), CornerRadius = new CornerRadius(1.5), Margin = type != "wpt" ? new Thickness(0, 0, 0, 45) : new Thickness(0, 40, 0, 0) };
            var visibilityBinding = new Binding { Source = this, Path = new PropertyPath("TextLabelVisible"), Mode = BindingMode.OneWay };
            BindingOperations.SetBinding(textBorder, VisibilityProperty, visibilityBinding);
            this.Children.Add(textBorder);

            textBorder.Child = new TextBlock
            {
                Text = name,
                Foreground = new SolidColorBrush(OpenSkyColors.OpenSkySimBriefText),

                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Padding = new Thickness(3),
                FontSize = 11,
                Margin = new Thickness(3)
            };

            MapLayer.SetPosition(this, new Location(lat, lon));
            MapLayer.SetPositionOrigin(this, PositionOrigin.Center);

            this.GeoCoordinate = new GeoCoordinate(lat, lon);
            this.WaypointName = name;
            this.WaypointType = type;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the geo coordinate used to calculate distance to another event marker (not for position report).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [NotNull]
        public GeoCoordinate GeoCoordinate { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the fuel tank capacity.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Bindable(true)]
        public Visibility TextLabelVisible
        {
            get => (Visibility)this.GetValue(TextLabelVisibleProperty);
            set => this.SetValue(TextLabelVisibleProperty, value);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the name of the waypoint.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string WaypointName { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the type of the waypoint.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string WaypointType { get; }
    }
}
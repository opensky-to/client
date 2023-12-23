// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SimbriefWaypointMarker.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Controls.Models
{
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Media;
    using System.Windows.Shapes;

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
        /// The text label font size property.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static readonly DependencyProperty TextLabelFontSizeProperty = DependencyProperty.Register(nameof(TextLabelFontSize), typeof(double), typeof(SimbriefWaypointMarker), new UIPropertyMetadata(11.0));

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The text label visible property.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static readonly DependencyProperty TextLabelVisibleProperty = DependencyProperty.Register(nameof(TextLabelVisible), typeof(Visibility), typeof(SimbriefWaypointMarker), new UIPropertyMetadata(Visibility.Visible));

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// A flight navlog waypoint we are wrapping around for saving.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly FlightLogXML.Waypoint waypoint = new();

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

            var textBlock = new TextBlock
            {
                Text = name,
                Foreground = new SolidColorBrush(OpenSkyColors.OpenSkySimBriefText),

                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Padding = new Thickness(3),
                FontSize = 11,
                Margin = new Thickness(3)
            };
            var fontSizeBinding = new Binding { Source = this, Path = new PropertyPath("TextLabelFontSize"), Mode = BindingMode.OneWay };
            BindingOperations.SetBinding(textBlock, TextBlock.FontSizeProperty, fontSizeBinding);
            textBorder.Child = textBlock;

            MapLayer.SetPosition(this, new Location(lat, lon));
            MapLayer.SetPositionOrigin(this, PositionOrigin.Center);

            this.waypoint.Latitude = lat;
            this.waypoint.Longitude = lon;
            this.waypoint.WaypointName = name;
            this.waypoint.WaypointType = type;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="SimbriefWaypointMarker"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 17/11/2021.
        /// </remarks>
        /// <param name="waypoint">
        /// A flight navlog waypoint we are wrapping around for saving, restored from a flight log xml file.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public SimbriefWaypointMarker(FlightLogXML.Waypoint waypoint)
        {
            this.waypoint = waypoint;

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

            var textBorder = new Border
                { BorderBrush = null, Background = new SolidColorBrush(OpenSkyColors.OpenSkySimBrief), CornerRadius = new CornerRadius(1.5), Margin = this.waypoint.WaypointType != "wpt" ? new Thickness(0, 0, 0, 45) : new Thickness(0, 40, 0, 0) };
            var visibilityBinding = new Binding { Source = this, Path = new PropertyPath("TextLabelVisible"), Mode = BindingMode.OneWay };
            BindingOperations.SetBinding(textBorder, VisibilityProperty, visibilityBinding);
            this.Children.Add(textBorder);

            var textBlock = new TextBlock
            {
                Text = this.waypoint.WaypointName,
                Foreground = new SolidColorBrush(OpenSkyColors.OpenSkySimBriefText),

                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Padding = new Thickness(3),
                FontSize = 11,
                Margin = new Thickness(3)
            };
            var fontSizeBinding = new Binding { Source = this, Path = new PropertyPath("TextLabelFontSize"), Mode = BindingMode.OneWay };
            BindingOperations.SetBinding(textBlock, TextBlock.FontSizeProperty, fontSizeBinding);
            textBorder.Child = textBlock;

            MapLayer.SetPosition(this, new Location(this.waypoint.Latitude, this.waypoint.Longitude));
            MapLayer.SetPositionOrigin(this, PositionOrigin.Center);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the size of the text label font.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Bindable(true)]
        public double TextLabelFontSize
        {
            get => (double)this.GetValue(TextLabelFontSizeProperty);
            set => this.SetValue(TextLabelFontSizeProperty, value);
        }

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
        /// Gets the way point we are wrapping around.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public FlightLogXML.Waypoint WayPoint => this.waypoint;
    }
}
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TrackingEventMarker.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Controls.Models
{
    using System.Device.Location;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Shapes;

    using JetBrains.Annotations;

    using Microsoft.Maps.MapControl.WPF;

    using OpenSky.Client.Tools;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// OpenSky tracking event marker.
    /// </summary>
    /// <remarks>
    /// sushi.at, 16/03/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class TrackingEventMarker : Canvas
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The flightLogXML marker object we are wrapping around.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly FlightLogXML.TrackingEventMarker marker = new();

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="TrackingEventMarker"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 17/11/2021.
        /// </remarks>
        /// <param name="marker">
        /// The flightLogXML marker object we are wrapping around, restored from a flight log xml file.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public TrackingEventMarker(FlightLogXML.TrackingEventMarker marker)
        {
            this.marker = marker;
            this.Width = this.MarkerSize;
            this.Height = this.MarkerSize;

            this.Children.Add(
                new Ellipse
                {
                    Width = this.MarkerSize,
                    Height = this.MarkerSize,
                    Fill = new SolidColorBrush(this.MarkerColor.ToMediaColor())
                });

            this.ToolTip = this.MarkerTooltip;

            MapLayer.SetPosition(this, new Location(this.marker.Latitude, this.marker.Longitude, this.marker.Altitude));
            MapLayer.SetPositionOrigin(this, PositionOrigin.Center);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="TrackingEventMarker"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 20/03/2021.
        /// </remarks>
        /// <param name="location">
        /// The location.
        /// </param>
        /// <param name="airportICAO">
        /// The airport icao.
        /// </param>
        /// <param name="markerColor">
        /// The marker color.
        /// </param>
        /// <param name="textColor">
        /// The text color.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public TrackingEventMarker(GeoCoordinate location, string airportICAO, Color markerColor, Color textColor)
        {
            this.Width = 40;
            this.Height = 40;
            this.SetValue(ZIndexProperty, 998);

            var textBorder = new Border { BorderBrush = null, Background = new SolidColorBrush(markerColor), CornerRadius = new CornerRadius(1.5) };
            this.Children.Add(textBorder);
            textBorder.Child = new TextBlock
            {
                Text = airportICAO,
                FontSize = 13,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Padding = new Thickness(3),
                Margin = new Thickness(5),
                Foreground = new SolidColorBrush(textColor)
            };

            MapLayer.SetPosition(this, new Location(location.Latitude, location.Longitude));
            MapLayer.SetPositionOrigin(this, PositionOrigin.Center);

            this.marker.Latitude = location.Latitude;
            this.marker.Longitude = location.Longitude;
            this.marker.Altitude = (int)location.Altitude;
            this.marker.MarkerSize = 40;
            this.MarkerTooltip = airportICAO;
            this.marker.MarkerColor = markerColor.ToDrawingColor();
            this.marker.IsAirportMarker = true;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the geo coordinate used to calculate distance to another event marker (not for position report).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [NotNull]
        public GeoCoordinate GeoCoordinate => new(this.marker.Latitude, this.marker.Longitude, this.marker.Altitude);

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets a value indicating whether this is an airport marker (we don't save those).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool IsAirportMarker => this.marker.IsAirportMarker;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the marker for saving.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public FlightLogXML.TrackingEventMarker Marker => this.marker;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the color of the marker.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public System.Drawing.Color MarkerColor => this.marker.MarkerColor;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the size of the marker.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int MarkerSize => this.marker.MarkerSize;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the marker tooltip.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string MarkerTooltip
        {
            get => this.marker.MarkerTooltip;
            set => this.marker.MarkerTooltip = value;
        }
    }
}
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TrackingEventMarker.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Controls.Models
{
    using System;
    using System.Device.Location;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Media;
    using System.Windows.Shapes;

    using JetBrains.Annotations;

    using Microsoft.Maps.MapControl.WPF;

    using OpenSky.Client.Tools;

    using OpenSkyApi;

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
            this.marker.IsAirportMarker = true;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="TrackingEventMarker"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 01/12/2021.
        /// </remarks>
        /// <param name="airport">
        /// The airport.
        /// </param>
        /// <param name="markerColor">
        /// The marker color.
        /// </param>
        /// <param name="textColor">
        /// The text color.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public TrackingEventMarker(Airport airport, Color markerColor, Color textColor)
        {
            if (airport.Runways.Count > 0)
            {
                this.SetValue(ZIndexProperty, 998);

                var outerBorder = new Border
                    { BorderBrush = new SolidColorBrush(markerColor), CornerRadius = new CornerRadius(1.5), BorderThickness = new Thickness(5), HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch };
                this.Children.Add(outerBorder);
                var widthBinding = new Binding { Source = this, Path = new PropertyPath("ActualWidth"), Mode = BindingMode.OneWay };
                BindingOperations.SetBinding(outerBorder, WidthProperty, widthBinding);
                var heightBinding = new Binding { Source = this, Path = new PropertyPath("ActualHeight"), Mode = BindingMode.OneWay };
                BindingOperations.SetBinding(outerBorder, HeightProperty, heightBinding);

                var textBorder = new Border { BorderBrush = null, Background = new SolidColorBrush(markerColor), CornerRadius = new CornerRadius(1.5) };
                this.Children.Add(textBorder);
                textBorder.Child = new TextBlock
                {
                    Text = airport.Icao,
                    FontSize = 13,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Padding = new Thickness(3),
                    Margin = new Thickness(5),
                    Foreground = new SolidColorBrush(textColor)
                };

                var from = new Location(airport.Latitude, airport.Longitude);
                var to = new Location(airport.Latitude, airport.Longitude);
                foreach (var runway in airport.Runways)
                {
                    foreach (var runwayEnd in runway.RunwayEnds)
                    {
                        from.Latitude = Math.Min(from.Latitude, runwayEnd.Latitude);
                        from.Longitude = Math.Min(from.Longitude, runwayEnd.Longitude);

                        to.Latitude = Math.Max(to.Latitude, runwayEnd.Latitude);
                        to.Longitude = Math.Max(to.Longitude, runwayEnd.Longitude);
                    }
                }

                from.Latitude -= 0.007;
                from.Longitude -= 0.02;
                to.Latitude += 0.007;
                to.Longitude += 0.02;

                MapLayer.SetPositionRectangle(this, new LocationRect(from, to));
            }
            else
            {
                // No runways?, just add the standard airport marker
                this.Width = 40;
                this.Height = 40;
                this.SetValue(ZIndexProperty, 998);

                var textBorder = new Border { BorderBrush = null, Background = new SolidColorBrush(markerColor), CornerRadius = new CornerRadius(1.5) };
                this.Children.Add(textBorder);
                textBorder.Child = new TextBlock
                {
                    Text = airport.Icao,
                    FontSize = 13,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Padding = new Thickness(3),
                    Margin = new Thickness(5),
                    Foreground = new SolidColorBrush(textColor)
                };

                MapLayer.SetPosition(this, new Location(airport.Latitude, airport.Longitude));
                MapLayer.SetPositionOrigin(this, PositionOrigin.Center);
            }

            this.marker.IsAirportMarker = true;
            this.marker.Latitude = airport.Latitude;
            this.marker.Longitude = airport.Longitude;
            this.marker.Altitude = airport.Altitude;
            this.IsAirportDetailMarker = true;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="TrackingEventMarker"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 01/12/2021.
        /// </remarks>
        /// <param name="airport">
        /// The airport.
        /// </param>
        /// <param name="markerColor">
        /// The marker color.
        /// </param>
        /// <param name="textColor">
        /// The text color.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public TrackingEventMarker(AirportsJSON.Airport airport, Color markerColor, Color textColor)
        {
            if (airport.Runways.Count > 0)
            {
                this.SetValue(ZIndexProperty, 998);

                var outerBorder = new Border
                    { BorderBrush = new SolidColorBrush(markerColor), CornerRadius = new CornerRadius(1.5), BorderThickness = new Thickness(5), HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch };
                this.Children.Add(outerBorder);
                var widthBinding = new Binding { Source = this, Path = new PropertyPath("ActualWidth"), Mode = BindingMode.OneWay };
                BindingOperations.SetBinding(outerBorder, WidthProperty, widthBinding);
                var heightBinding = new Binding { Source = this, Path = new PropertyPath("ActualHeight"), Mode = BindingMode.OneWay };
                BindingOperations.SetBinding(outerBorder, HeightProperty, heightBinding);

                var textBorder = new Border { BorderBrush = null, Background = new SolidColorBrush(markerColor), CornerRadius = new CornerRadius(1.5) };
                this.Children.Add(textBorder);
                textBorder.Child = new TextBlock
                {
                    Text = airport.ICAO,
                    FontSize = 13,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Padding = new Thickness(3),
                    Margin = new Thickness(5),
                    Foreground = new SolidColorBrush(textColor)
                };

                var from = new Location(airport.Latitude, airport.Longitude);
                var to = new Location(airport.Latitude, airport.Longitude);
                foreach (var runway in airport.Runways)
                {
                    foreach (var runwayEnd in runway.RunwayEnds)
                    {
                        from.Latitude = Math.Min(from.Latitude, runwayEnd.Latitude);
                        from.Longitude = Math.Min(from.Longitude, runwayEnd.Longitude);

                        to.Latitude = Math.Max(to.Latitude, runwayEnd.Latitude);
                        to.Longitude = Math.Max(to.Longitude, runwayEnd.Longitude);
                    }
                }

                from.Latitude -= 0.007;
                from.Longitude -= 0.02;
                to.Latitude += 0.007;
                to.Longitude += 0.02;

                MapLayer.SetPositionRectangle(this, new LocationRect(from, to));
            }
            else
            {
                // No runways?, just add the standard airport marker
                this.Width = 40;
                this.Height = 40;
                this.SetValue(ZIndexProperty, 998);

                var textBorder = new Border { BorderBrush = null, Background = new SolidColorBrush(markerColor), CornerRadius = new CornerRadius(1.5) };
                this.Children.Add(textBorder);
                textBorder.Child = new TextBlock
                {
                    Text = airport.ICAO,
                    FontSize = 13,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Padding = new Thickness(3),
                    Margin = new Thickness(5),
                    Foreground = new SolidColorBrush(textColor)
                };

                MapLayer.SetPosition(this, new Location(airport.Latitude, airport.Longitude));
                MapLayer.SetPositionOrigin(this, PositionOrigin.Center);
            }

            this.marker.IsAirportMarker = true;
            this.marker.Latitude = airport.Latitude;
            this.marker.Longitude = airport.Longitude;
            this.IsAirportDetailMarker = true;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="TrackingEventMarker"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 01/12/2021.
        /// </remarks>
        /// <param name="runway">
        /// The runway.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public TrackingEventMarker(Runway runway)
        {
            if (runway.RunwayEnds.Count == 2)
            {
                this.SetValue(ZIndexProperty, 998);
                var leftEnd = runway.RunwayEnds.First().Longitude <= runway.RunwayEnds.Last().Longitude ? runway.RunwayEnds.First() : runway.RunwayEnds.Last();
                var rightEnd = runway.RunwayEnds.First().Longitude <= runway.RunwayEnds.Last().Longitude ? runway.RunwayEnds.Last() : runway.RunwayEnds.First();
                var color = leftEnd.HasClosedMarkings && rightEnd.HasClosedMarkings ? OpenSkyColors.OpenSkyRed : OpenSkyColors.OpenSkyTeal;

                var line = new Line
                {
                    StrokeThickness = 5, Stroke = new SolidColorBrush(color),
                    X1 = 0,
                };
                if (leftEnd.Latitude <= rightEnd.Latitude)
                {
                    var heightBinding = new Binding { Source = this, Path = new PropertyPath("ActualHeight"), Mode = BindingMode.OneWay };
                    BindingOperations.SetBinding(line, Line.Y1Property, heightBinding);
                    line.Y2 = 0;
                }
                else
                {
                    line.Y1 = 0;
                    var heightBinding = new Binding { Source = this, Path = new PropertyPath("ActualHeight"), Mode = BindingMode.OneWay };
                    BindingOperations.SetBinding(line, Line.Y2Property, heightBinding);
                }

                var widthBinding = new Binding { Source = this, Path = new PropertyPath("ActualWidth"), Mode = BindingMode.OneWay };
                BindingOperations.SetBinding(line, Line.X2Property, widthBinding);
                this.Children.Add(line);

                {
                    var runwayEnd = runway.RunwayEnds.First();
                    var textBorder = new Border { BorderBrush = null, Background = new SolidColorBrush(runwayEnd.HasClosedMarkings ? OpenSkyColors.OpenSkyRed : OpenSkyColors.OpenSkyTeal), CornerRadius = new CornerRadius(1.5) };
                    this.Children.Add(textBorder);
                    textBorder.Child = new TextBlock
                    {
                        Text = runwayEnd.HasClosedMarkings ? "XX" : runwayEnd.Name,
                        FontSize = 13,
                        FontWeight = FontWeights.Bold,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Padding = new Thickness(3),
                        Margin = new Thickness(5),
                        Foreground = new SolidColorBrush(Colors.White)
                    };
                    SetLeft(textBorder, 0);
                    if (leftEnd.Latitude <= rightEnd.Latitude)
                    {
                        SetBottom(textBorder, 0);
                    }
                    else
                    {
                        SetTop(textBorder, 0);
                    }
                }

                {
                    var runwayEnd = runway.RunwayEnds.Last();
                    var textBorder = new Border { BorderBrush = null, Background = new SolidColorBrush(runwayEnd.HasClosedMarkings ? OpenSkyColors.OpenSkyRed : OpenSkyColors.OpenSkyTeal), CornerRadius = new CornerRadius(1.5) };
                    this.Children.Add(textBorder);
                    textBorder.Child = new TextBlock
                    {
                        Text = runwayEnd.HasClosedMarkings ? "XX" : runwayEnd.Name,
                        FontSize = 13,
                        FontWeight = FontWeights.Bold,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Padding = new Thickness(3),
                        Margin = new Thickness(5),
                        Foreground = new SolidColorBrush(Colors.White)
                    };
                    SetRight(textBorder, 0);
                    if (leftEnd.Latitude <= rightEnd.Latitude)
                    {
                        SetTop(textBorder, 0);
                    }
                    else
                    {
                        SetBottom(textBorder, 0);
                    }
                }

                MapLayer.SetPositionRectangle(this, new LocationRect(new Location(leftEnd.Latitude, leftEnd.Longitude), new Location(rightEnd.Latitude, rightEnd.Longitude)));
                this.marker.IsAirportMarker = true;
                this.marker.Latitude = leftEnd.Latitude;
                this.marker.Longitude = leftEnd.Longitude;
                this.IsAirportDetailMarker = true;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="TrackingEventMarker"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 01/12/2021.
        /// </remarks>
        /// <param name="runway">
        /// The runway.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public TrackingEventMarker(AirportsJSON.Runway runway)
        {
            if (runway.RunwayEnds.Count == 2)
            {
                this.SetValue(ZIndexProperty, 998);
                var leftEnd = runway.RunwayEnds.First().Longitude <= runway.RunwayEnds.Last().Longitude ? runway.RunwayEnds.First() : runway.RunwayEnds.Last();
                var rightEnd = runway.RunwayEnds.First().Longitude <= runway.RunwayEnds.Last().Longitude ? runway.RunwayEnds.Last() : runway.RunwayEnds.First();
                var color = leftEnd.HasClosedMarkings && rightEnd.HasClosedMarkings ? OpenSkyColors.OpenSkyRed : OpenSkyColors.OpenSkyTeal;

                var line = new Line
                {
                    StrokeThickness = 5, Stroke = new SolidColorBrush(color),
                    X1 = 0,
                };
                if (leftEnd.Latitude <= rightEnd.Latitude)
                {
                    var heightBinding = new Binding { Source = this, Path = new PropertyPath("ActualHeight"), Mode = BindingMode.OneWay };
                    BindingOperations.SetBinding(line, Line.Y1Property, heightBinding);
                    line.Y2 = 0;
                }
                else
                {
                    line.Y1 = 0;
                    var heightBinding = new Binding { Source = this, Path = new PropertyPath("ActualHeight"), Mode = BindingMode.OneWay };
                    BindingOperations.SetBinding(line, Line.Y2Property, heightBinding);
                }

                var widthBinding = new Binding { Source = this, Path = new PropertyPath("ActualWidth"), Mode = BindingMode.OneWay };
                BindingOperations.SetBinding(line, Line.X2Property, widthBinding);
                this.Children.Add(line);

                {
                    var runwayEnd = runway.RunwayEnds.First();
                    var textBorder = new Border { BorderBrush = null, Background = new SolidColorBrush(runwayEnd.HasClosedMarkings ? OpenSkyColors.OpenSkyRed : OpenSkyColors.OpenSkyTeal), CornerRadius = new CornerRadius(1.5) };
                    this.Children.Add(textBorder);
                    textBorder.Child = new TextBlock
                    {
                        Text = runwayEnd.HasClosedMarkings ? "XX" : runwayEnd.Name,
                        FontSize = 13,
                        FontWeight = FontWeights.Bold,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Padding = new Thickness(3),
                        Margin = new Thickness(5),
                        Foreground = new SolidColorBrush(Colors.White)
                    };
                    SetLeft(textBorder, 0);
                    if (leftEnd.Latitude <= rightEnd.Latitude)
                    {
                        SetBottom(textBorder, 0);
                    }
                    else
                    {
                        SetTop(textBorder, 0);
                    }
                }

                {
                    var runwayEnd = runway.RunwayEnds.Last();
                    var textBorder = new Border { BorderBrush = null, Background = new SolidColorBrush(runwayEnd.HasClosedMarkings ? OpenSkyColors.OpenSkyRed : OpenSkyColors.OpenSkyTeal), CornerRadius = new CornerRadius(1.5) };
                    this.Children.Add(textBorder);
                    textBorder.Child = new TextBlock
                    {
                        Text = runwayEnd.HasClosedMarkings ? "XX" : runwayEnd.Name,
                        FontSize = 13,
                        FontWeight = FontWeights.Bold,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Padding = new Thickness(3),
                        Margin = new Thickness(5),
                        Foreground = new SolidColorBrush(Colors.White)
                    };
                    SetRight(textBorder, 0);
                    if (leftEnd.Latitude <= rightEnd.Latitude)
                    {
                        SetTop(textBorder, 0);
                    }
                    else
                    {
                        SetBottom(textBorder, 0);
                    }
                }

                MapLayer.SetPositionRectangle(this, new LocationRect(new Location(leftEnd.Latitude, leftEnd.Longitude), new Location(rightEnd.Latitude, rightEnd.Longitude)));
                this.marker.IsAirportMarker = true;
                this.marker.Latitude = leftEnd.Latitude;
                this.marker.Longitude = leftEnd.Longitude;
                this.IsAirportDetailMarker = true;
            }
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
        /// Gets a value indicating whether this is an airport detail marker (box or runways - we don't save those and they have a different zoom level setting).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool IsAirportDetailMarker { get; }

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
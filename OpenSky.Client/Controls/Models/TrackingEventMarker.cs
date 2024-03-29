﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TrackingEventMarker.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Controls.Models
{
    using System;
    using System.ComponentModel;
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
        /// The text label font size property.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static readonly DependencyProperty TextLabelFontSizeProperty = DependencyProperty.Register(nameof(TextLabelFontSize), typeof(double), typeof(TrackingEventMarker), new UIPropertyMetadata(11.0));

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The text label visible property.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static readonly DependencyProperty TextLabelVisibleProperty = DependencyProperty.Register(nameof(TextLabelVisible), typeof(Visibility), typeof(TrackingEventMarker), new UIPropertyMetadata(Visibility.Visible));

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
            this.FromCoordinate = new GeoCoordinate(this.marker.Latitude, this.marker.Longitude);
            this.ToCoordinate = new GeoCoordinate(this.marker.Latitude, this.marker.Longitude);
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
        /// <param name="dynamicFontSize">
        /// (Optional) True to enable dynamic font size, bound to TextLabelFontSize property.
        /// </param>
        /// <param name="airportSize">
        /// (Optional) The size of the airport.
        /// </param>
        /// <param name="supportsSuper">
        /// (Optional) True if the airport supports super (underlines ICAO code).
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public TrackingEventMarker(GeoCoordinate location, string airportICAO, Color markerColor, Color textColor, bool dynamicFontSize = false, int airportSize = -1, bool supportsSuper = false)
        {
            this.Width = 40;
            this.Height = 40;
            this.AirportSize = airportSize;

            var minSize = 12;
            if (airportSize is >= 0 and < 5)
            {
                minSize = 15;
            }

            var zIndex = airportSize switch
            {
                6 => 998,
                5 => 997,
                4 => 996,
                3 => 995,
                2 => 994,
                1 => 993,
                0 => 992,
                _ => 998
            };

            this.SetValue(ZIndexProperty, zIndex);

            var textBorder = new Border { BorderBrush = null, Background = new SolidColorBrush(markerColor), CornerRadius = new CornerRadius(1.5), MinHeight = minSize, MinWidth = minSize };
            this.Children.Add(textBorder);
            var textBlock = new TextBlock
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
            if (supportsSuper)
            {
                textBlock.TextDecorations.Add(TextDecorations.Underline);
            }

            textBorder.Child = textBlock;

            if (dynamicFontSize)
            {
                var fontSizeBinding = new Binding { Source = this, Path = new PropertyPath("TextLabelFontSize"), Mode = BindingMode.OneWay };
                BindingOperations.SetBinding(textBlock, TextBlock.FontSizeProperty, fontSizeBinding);
                this.HasDynamicFontSize = true;

                var textLabelVisibleBinding = new Binding { Source = this, Path = new PropertyPath("TextLabelVisible"), Mode = BindingMode.OneWay };
                BindingOperations.SetBinding(textBlock, VisibilityProperty, textLabelVisibleBinding);
            }

            MapLayer.SetPosition(this, new Location(location.Latitude, location.Longitude));
            MapLayer.SetPositionOrigin(this, PositionOrigin.Center);
            this.marker.Latitude = location.Latitude;
            this.marker.Longitude = location.Longitude;
            this.marker.IsAirportMarker = true;
            this.FromCoordinate = location;
            this.ToCoordinate = location;
            this.ToolTip = "Loading...";
            this.ToolTipOpening += (_, _) =>
            {
                this.ToolTip = new AirportDetails { AirportICAO = airportICAO };
            };
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
                var textBlock = new TextBlock
                {
                    Text = $"{airport.Icao}: {airport.Name}",
                    FontSize = 13,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Padding = new Thickness(3),
                    Margin = new Thickness(5),
                    Foreground = new SolidColorBrush(textColor)
                };
                textBorder.Child = textBlock;
                if (airport.SupportsSuper)
                {
                    textBlock.TextDecorations.Add(TextDecorations.Underline);
                }

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
                this.FromCoordinate = new GeoCoordinate(from.Latitude, from.Longitude);
                this.ToCoordinate = new GeoCoordinate(to.Latitude, to.Longitude);

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
                var textBlock = new TextBlock
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
                textBorder.Child = textBlock;
                if (airport.SupportsSuper)
                {
                    textBlock.TextDecorations.Add(TextDecorations.Underline);
                }

                MapLayer.SetPosition(this, new Location(airport.Latitude, airport.Longitude));
                MapLayer.SetPositionOrigin(this, PositionOrigin.Center);
                this.FromCoordinate = new GeoCoordinate(airport.Latitude, airport.Longitude);
                this.ToCoordinate = new GeoCoordinate(airport.Latitude, airport.Longitude);
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
                var textBlock = new TextBlock
                {
                    Text = $"{airport.ICAO}: {airport.Name}",
                    FontSize = 13,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Padding = new Thickness(3),
                    Margin = new Thickness(5),
                    Foreground = new SolidColorBrush(textColor)
                };
                textBorder.Child = textBlock;
                if (airport.SupportsSuper)
                {
                    textBlock.TextDecorations.Add(TextDecorations.Underline);
                }

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
                this.FromCoordinate = new GeoCoordinate(from.Latitude, from.Longitude);
                this.ToCoordinate = new GeoCoordinate(to.Latitude, to.Longitude);

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
                var textBlock = new TextBlock
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
                textBorder.Child = textBlock;
                if (airport.SupportsSuper)
                {
                    textBlock.TextDecorations.Add(TextDecorations.Underline);
                }

                MapLayer.SetPosition(this, new Location(airport.Latitude, airport.Longitude));
                MapLayer.SetPositionOrigin(this, PositionOrigin.Center);
                this.FromCoordinate = new GeoCoordinate(airport.Latitude, airport.Longitude);
                this.ToCoordinate = new GeoCoordinate(airport.Latitude, airport.Longitude);
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
                this.SetValue(ZIndexProperty, 997);
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
                    var runwayEnd = leftEnd;
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
                    var runwayEnd = rightEnd;
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
                this.FromCoordinate = new GeoCoordinate(leftEnd.Latitude, leftEnd.Longitude);
                this.ToCoordinate = new GeoCoordinate(rightEnd.Latitude, rightEnd.Longitude);
            }
            else
            {
                this.FromCoordinate = new GeoCoordinate(0, 0);
                this.ToCoordinate = new GeoCoordinate(0, 0);
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
                this.SetValue(ZIndexProperty, 997);
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
                    var runwayEnd = leftEnd;
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
                    var runwayEnd = rightEnd;
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
                this.FromCoordinate = new GeoCoordinate(leftEnd.Latitude, leftEnd.Longitude);
                this.ToCoordinate = new GeoCoordinate(rightEnd.Latitude, rightEnd.Longitude);
            }
            else
            {
                this.FromCoordinate = new GeoCoordinate(0, 0);
                this.ToCoordinate = new GeoCoordinate(0, 0);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the size of the airport.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int AirportSize { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets from coordinate (geo bounding box).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [NotNull]
        public GeoCoordinate FromCoordinate { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the geo coordinate used to calculate distance to another event marker (not for position report).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [NotNull]
        public GeoCoordinate GeoCoordinate => new(this.marker.Latitude, this.marker.Longitude, this.marker.Altitude);

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets a value indicating whether this object has dynamic font size enabled.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool HasDynamicFontSize { get; }

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
        /// Gets or sets to coordinate (geo bounding box).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [NotNull]
        public GeoCoordinate ToCoordinate { get; set; }
    }
}
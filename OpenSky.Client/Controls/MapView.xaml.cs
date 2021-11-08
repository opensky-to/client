﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MapView.xaml.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Controls
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Input;
    using System.Windows.Media;

    using Microsoft.Maps.MapControl.WPF;

    using OpenSky.Client.Controls.Models;
    using OpenSky.Client.Converters;
    using OpenSky.Client.Tools;

    using TomsToolbox.Essentials;

    /// -------------------------------------------------------------------------------------------------
    /// <content>
    /// Mapview user control.
    /// </content>
    /// -------------------------------------------------------------------------------------------------
    public partial class MapView
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The aircraft trail locations property.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static readonly DependencyProperty AircraftTrailLocationsProperty = DependencyProperty.Register("AircraftTrailLocations", typeof(LocationCollection), typeof(MapView), new UIPropertyMetadata(new LocationCollection()));

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The show loading property.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static readonly DependencyProperty ShowFollowPlaneProperty = DependencyProperty.Register("ShowFollowPlane", typeof(bool), typeof(MapView), new UIPropertyMetadata(false));

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The simbrief route locations property.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static readonly DependencyProperty SimbriefRouteLocationsProperty = DependencyProperty.Register("SimbriefRouteLocations", typeof(LocationCollection), typeof(MapView), new UIPropertyMetadata(new LocationCollection()));

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The simbrief waypoint markers property.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static readonly DependencyProperty SimbriefWaypointMarkersProperty = DependencyProperty.Register(
            "SimbriefWaypointMarkers",
            typeof(ObservableCollection<SimbriefWaypointMarker>),
            typeof(MapView),
            new UIPropertyMetadata(new ObservableCollection<SimbriefWaypointMarker>()));

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The tracking event markers property.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------T
        public static readonly DependencyProperty TrackingEventMarkersProperty = DependencyProperty.Register(
            "TrackingEventMarkers",
            typeof(ObservableCollection<TrackingEventMarker>),
            typeof(MapView),
            new UIPropertyMetadata(new ObservableCollection<TrackingEventMarker>()));

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="MapView"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 04/11/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public MapView()
        {
            this.InitializeComponent();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the aircraft trail locations.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Bindable(true)]
        public LocationCollection AircraftTrailLocations
        {
            get => (LocationCollection)this.GetValue(AircraftTrailLocationsProperty);
            set => this.SetValue(AircraftTrailLocationsProperty, value);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether to show the follow plane option.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Bindable(true)]
        public bool ShowFollowPlane
        {
            get => (bool)this.GetValue(ShowFollowPlaneProperty);
            set => this.SetValue(ShowFollowPlaneProperty, value);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the simbrief route locations.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Bindable(true)]
        public LocationCollection SimbriefRouteLocations
        {
            get => (LocationCollection)this.GetValue(SimbriefRouteLocationsProperty);
            set => this.SetValue(SimbriefRouteLocationsProperty, value);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the simbrief waypoint markers.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Bindable(true)]
        public ObservableCollection<SimbriefWaypointMarker> SimbriefWaypointMarkers
        {
            get => (ObservableCollection<SimbriefWaypointMarker>)this.GetValue(SimbriefWaypointMarkersProperty);

            set
            {
                this.SetValue(SimbriefWaypointMarkersProperty, value);
                value.CollectionChanged += this.SimbriefWaypointMarkersCollectionChanged;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the tracking event markers.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Bindable(true)]
        public ObservableCollection<TrackingEventMarker> TrackingEventMarkers
        {
            get => (ObservableCollection<TrackingEventMarker>)this.GetValue(TrackingEventMarkersProperty);

            set
            {
                this.SetValue(TrackingEventMarkersProperty, value);
                value.CollectionChanged += this.TrackingEventMarkersCollectionChanged;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Map type selection changed.
        /// </summary>
        /// <remarks>
        /// sushi.at, 25/03/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Selection changed event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void MapTypeOnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.MapType.SelectedItem is ComboBoxItem item)
            {
                switch (item.Content as string)
                {
                    case "Road":
                        this.WpfMapView.Mode = new RoadMode();
                        break;
                    case "Aerial":
                        this.WpfMapView.Mode = new AerialMode();
                        break;
                }
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Map view on loaded.
        /// </summary>
        /// <remarks>
        /// sushi.at, 04/11/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Routed event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void MapViewOnLoaded(object sender, RoutedEventArgs e)
        {
            this.WpfMapView.CredentialsProvider = new ApplicationIdCredentialsProvider(UserSessionService.Instance.LinkedAccounts?.BingMapsKey);

            var minLat = 90.0;
            var maxLat = -90.0;
            var minLon = 80.0;
            var maxLon = -180.0;

            var simbriefRoute = new MapPolyline { Stroke = new SolidColorBrush(OpenSkyColors.OpenSkySimBrief), StrokeThickness = 4, Locations = this.SimbriefRouteLocations };
            this.WpfMapView.Children.Add(simbriefRoute);
            if (this.SimbriefRouteLocations.Count > 0)
            {
                minLat = Math.Min(minLat, this.SimbriefRouteLocations.Min(l => l.Latitude));
                maxLat = Math.Max(maxLat, this.SimbriefRouteLocations.Max(l => l.Latitude));
                minLon = Math.Min(minLon, this.SimbriefRouteLocations.Min(l => l.Longitude));
                maxLon = Math.Max(maxLon, this.SimbriefRouteLocations.Max(l => l.Longitude));
            }

            var aircraftTrail = new MapPolyline { Stroke = new SolidColorBrush(OpenSkyColors.OpenSkyTeal), StrokeThickness = 4, Locations = this.AircraftTrailLocations };
            this.WpfMapView.Children.Add(aircraftTrail);

            // todo Aircraft positions, define bind-able structure that can contain multiple (for main map view)

            if (this.SimbriefWaypointMarkers.Count > 0)
            {
                this.SimbriefWaypointMarkersCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, this.SimbriefWaypointMarkers));

                minLat = Math.Min(minLat, this.SimbriefWaypointMarkers.Min(m => m.GeoCoordinate.Latitude));
                maxLat = Math.Max(maxLat, this.SimbriefWaypointMarkers.Max(m => m.GeoCoordinate.Latitude));
                minLon = Math.Min(minLon, this.SimbriefWaypointMarkers.Min(m => m.GeoCoordinate.Longitude));
                maxLon = Math.Max(maxLon, this.SimbriefWaypointMarkers.Max(m => m.GeoCoordinate.Longitude));
            }

            this.SimbriefWaypointMarkers.CollectionChanged += this.SimbriefWaypointMarkersCollectionChanged;

            if (this.TrackingEventMarkers.Count > 0)
            {
                this.TrackingEventMarkersCollectionChanged(this,new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, this.TrackingEventMarkers));

                minLat = Math.Min(minLat, this.TrackingEventMarkers.Min(m => m.GeoCoordinate.Latitude));
                maxLat = Math.Max(maxLat, this.TrackingEventMarkers.Max(m => m.GeoCoordinate.Latitude));
                minLon = Math.Min(minLon, this.TrackingEventMarkers.Min(m => m.GeoCoordinate.Longitude));
                maxLon = Math.Max(maxLon, this.TrackingEventMarkers.Max(m => m.GeoCoordinate.Longitude));
            }
            
            this.TrackingEventMarkers.CollectionChanged += this.TrackingEventMarkersCollectionChanged;

            // Add a bit of a margin around the view to zoom into
            minLat = Math.Max(-90.0, minLat - 0.5);
            maxLat = Math.Min(90.0, maxLat + 0.5);
            minLon = Math.Max(-180.0, minLon - 0.5);
            maxLon = Math.Min(80, maxLon + 0.5);

            UpdateGUIDelegate moveMap = () =>
            {
                this.WpfMapView.AnimationLevel = AnimationLevel.None;
                this.WpfMapView.SetView(new LocationRect(new Location(minLat, minLon), new Location(maxLat, maxLon)));
                this.WpfMapView.AnimationLevel = AnimationLevel.Full;
            };
            this.Dispatcher.BeginInvoke(moveMap);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Map view on mouse wheel.
        /// </summary>
        /// <remarks>
        /// sushi.at, 04/11/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Mouse wheel event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void MapViewOnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Prevent scrolling of outer containers, we want to zoom the map
            e.Handled = true;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// simBrief waypoint markers collection changed.
        /// </summary>
        /// <remarks>
        /// sushi.at, 08/11/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Notify collection changed event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void SimbriefWaypointMarkersCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (SimbriefWaypointMarker item in e.NewItems)
                {
                    var existingMapLayer = item.Parent as MapLayer;
                    existingMapLayer?.Children.Remove(item);
                    if (BindingOperations.IsDataBound(item, SimbriefWaypointMarker.TextLabelVisibleProperty))
                    {
                        BindingOperations.ClearBinding(item, SimbriefWaypointMarker.TextLabelVisibleProperty);
                    }

                    this.WpfMapView.Children.Add(item);

                    // Add zoom level -> visibility binding with custom converter
                    var zoomLevelBinding = new Binding { Source = this.WpfMapView, Path = new PropertyPath("ZoomLevel"), Converter = new MapZoomLevelVisibilityConverter() };
                    BindingOperations.SetBinding(item, SimbriefWaypointMarker.TextLabelVisibleProperty, zoomLevelBinding);
                }
            }

            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (SimbriefWaypointMarker item in e.OldItems)
                {
                    this.WpfMapView.Children.Remove(item);
                }
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Tracking event markers collection changed.
        /// </summary>
        /// <remarks>
        /// sushi.at, 08/11/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Notify collection changed event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void TrackingEventMarkersCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (TrackingEventMarker item in e.NewItems)
                {
                    var existingMapLayer = item.Parent as MapLayer;
                    existingMapLayer?.Children.Remove(item);

                    this.WpfMapView.Children.Add(item);
                }
            }

            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (TrackingEventMarker item in e.OldItems)
                {
                    this.WpfMapView.Children.Remove(item);
                }
            }

            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                var toRemove = new List<TrackingEventMarker>();
                foreach (UIElement child in this.WpfMapView.Children)
                {
                    if (child is TrackingEventMarker trackingEventMarker)
                    {
                        toRemove.Add(trackingEventMarker);
                    }
                }

                this.WpfMapView.Children.RemoveRange(toRemove);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The user interacted with the map.
        /// </summary>
        /// <remarks>
        /// sushi.at, 24/03/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Mouse button event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void UserMapInteraction(object sender, EventArgs e)
        {
            var viewModel = (MapViewViewModel)this.DataContext;
            viewModel.LastUserMapInteraction = DateTime.UtcNow;
        }
    }
}
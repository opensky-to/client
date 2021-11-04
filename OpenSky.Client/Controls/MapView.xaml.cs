// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MapView.xaml.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Controls
{
    using System;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Input;
    using System.Windows.Media;

    using Microsoft.Maps.MapControl.WPF;

    using OpenSky.Client.Controls.Models;
    using OpenSky.Client.Converters;

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
        public static readonly DependencyProperty SimbriefWaypointMarkersProperty = DependencyProperty.Register("SimbriefWaypointMarkers", typeof(ObservableCollection<SimbriefWaypointMarker>), typeof(MapView), new UIPropertyMetadata(new ObservableCollection<SimbriefWaypointMarker>()));

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

            var simbriefRoute = new MapPolyline { Stroke = new SolidColorBrush(OpenSkyColors.OpenSkySimBrief), StrokeThickness = 4, Locations = this.SimbriefRouteLocations };
            this.WpfMapView.Children.Add(simbriefRoute);

            var aircraftTrail = new MapPolyline { Stroke = new SolidColorBrush(OpenSkyColors.OpenSkyTeal), StrokeThickness = 4, Locations = this.AircraftTrailLocations };
            this.WpfMapView.Children.Add(aircraftTrail);

            // todo Aircraft positions, define bind-able structure that can contain multiple (for main map view)

            this.SimbriefWaypointMarkers.CollectionChanged += this.SimbriefWaypointMarkersCollectionChanged;
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
        /// Map view on unloaded.
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
        private void MapViewOnUnloaded(object sender, RoutedEventArgs e)
        {
            foreach (UIElement child in this.WpfMapView.Children)
            {
                if (child is SimbriefWaypointMarker simbrief)
                {
                    BindingOperations.ClearAllBindings(simbrief);
                }
            }

            this.WpfMapView.Children.Clear();
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
        /// Flight tracking view model tracking event marker added.
        /// </summary>
        /// <remarks>
        /// sushi.at, 17/03/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// A TrackingEventMarker to process.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void FlightTrackingViewModelTrackingEventMarkerAdded(object sender, TrackingEventMarker e)
        {
            //try
            //{
            //    // Make sure we remove it from any previous map layers
            //    var existingMapLayer = e.Parent as MapLayer;
            //    existingMapLayer?.Children.Remove(e);

            //    this.MapView.Children.Add(e);
            //}
            //catch (Exception ex)
            //{
            //    Debug.WriteLine(ex);
            //}
        }
    }
}
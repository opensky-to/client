// --------------------------------------------------------------------------------------------------------------------
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
    using System.Windows.Media.Imaging;

    using Microsoft.Maps.MapControl.WPF;

    using OpenSky.Client.Controls.Animations;
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
        /// The aircraft positions property.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static readonly DependencyProperty AircraftPositionsProperty = DependencyProperty.Register(
            "AircraftPositions",
            typeof(ObservableCollection<AircraftPosition>),
            typeof(MapView),
            new UIPropertyMetadata(new ObservableCollection<AircraftPosition>()));

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The aircraft trail locations property.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static readonly DependencyProperty AircraftTrailLocationsProperty = DependencyProperty.Register("AircraftTrailLocations", typeof(LocationCollection), typeof(MapView), new UIPropertyMetadata(new LocationCollection()));

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The job trails property.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static readonly DependencyProperty JobTrailsProperty = DependencyProperty.Register("JobTrails", typeof(ObservableCollection<MapPolyline>), typeof(MapView), new UIPropertyMetadata(new ObservableCollection<MapPolyline>()));

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
        /// The zoom level font size converter.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private static readonly MapZoomLevelFontSizeConverter ZoomLevelFontSizeConverter = new();

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The zoom level visibility converter.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private static readonly MapZoomLevelVisibilityConverter ZoomLevelVisibilityConverter = new();

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The collections we already subscribed the changed event.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly List<INotifyCollectionChanged> collectionChangedSubscribed = new();

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The job trail animation images.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly List<UIElement> jobTrailAnimationImages = new();

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The job trail animations.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly List<MapPathAnimation> jobTrailAnimations = new();

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The aircraft trail animation.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private MapPathAnimation aircraftTrailAnimation;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The animated aircraft position.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private AircraftPosition animatedAircraftPosition;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The last map view frame update Date/Time.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private DateTime lastFrameUpdate = DateTime.MinValue;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// True if the user interacted with the map (move, zoom, etc.)
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private bool userMapInteraction;

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
        /// Gets or sets the aircraft positions.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Bindable(true)]
        public ObservableCollection<AircraftPosition> AircraftPositions
        {
            get => (ObservableCollection<AircraftPosition>)this.GetValue(AircraftPositionsProperty);
            set
            {
                this.SetValue(AircraftPositionsProperty, value);
                value.CollectionChanged += this.AircraftPositionsCollectionChanged;
            }
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
        /// Gets or sets the job trails.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Bindable(true)]
        public ObservableCollection<MapPolyline> JobTrails
        {
            get => (ObservableCollection<MapPolyline>)this.GetValue(JobTrailsProperty);
            set
            {
                this.SetValue(JobTrailsProperty, value);
                value.CollectionChanged += this.JobTrailsCollectionChanged;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether to show all airports.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool ShowAllAirports { get; set; }

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
        /// Gets or sets a value indicating whether to zoom for aircraft location changes.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool ZoomForAircraftLocations { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Animate the aircraft trail.
        /// </summary>
        /// <remarks>
        /// sushi.at, 14/12/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public void AnimateAircraftTrail(int duration = 25, int delay = 30)
        {
            if (this.aircraftTrailAnimation != null)
            {
                this.aircraftTrailAnimation.Stop();
                this.aircraftTrailAnimation = null;
                if (this.animatedAircraftPosition != null)
                {
                    this.WpfMapView.Children.Remove(this.animatedAircraftPosition);
                    this.animatedAircraftPosition = null;
                }
            }

            if (this.AircraftTrailLocations.Count >= 2)
            {
                var aircraftPosition = new AircraftPosition
                {
                    Location = this.AircraftTrailLocations[0],
                    Heading = 0.0
                };
                this.animatedAircraftPosition = aircraftPosition;
                this.WpfMapView.Children.Add(aircraftPosition);

                this.aircraftTrailAnimation = new MapPathAnimation(
                    this.AircraftTrailLocations,
                    (location, _, bearing) =>
                    {
                        aircraftPosition.Location = location;
                        aircraftPosition.Heading = bearing;
                    },
                    false,
                    duration * 1000,
                    true,
                    delay);
                this.aircraftTrailAnimation.Play();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Center the mapview on the specified location.
        /// </summary>
        /// <remarks>
        /// sushi.at, 20/11/2021.
        /// </remarks>
        /// <param name="location">
        /// The location.
        /// </param>
        /// <param name="isUserMapInteraction">
        /// (Optional) True if this is user map interaction, false if not.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public void Center(Location location, bool isUserMapInteraction = false)
        {
            if (isUserMapInteraction)
            {
                this.userMapInteraction = true;
            }

            this.WpfMapView.Center = location;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Moves and zooms the map to show all added markers.
        /// </summary>
        /// <remarks>
        /// sushi.at, 09/11/2021.
        /// </remarks>
        /// <param name="overrideUser">
        /// (Optional) True to override the user.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public void ShowAllMarkers(bool overrideUser = false)
        {
            if (!this.userMapInteraction || overrideUser)
            {
                var anyMarkers = false;
                var minLat = 90.0;
                var maxLat = -90.0;
                var minLon = 180.0;
                var maxLon = -180.0;

                if (this.AircraftPositions.Count > 0)
                {
                    minLat = Math.Min(minLat, this.AircraftPositions.Min(l => l.Location.Latitude));
                    maxLat = Math.Max(maxLat, this.AircraftPositions.Max(l => l.Location.Latitude));
                    minLon = Math.Min(minLon, this.AircraftPositions.Min(l => l.Location.Longitude));
                    maxLon = Math.Max(maxLon, this.AircraftPositions.Max(l => l.Location.Longitude));
                    anyMarkers = true;
                }

                if (this.SimbriefRouteLocations.Count > 0)
                {
                    minLat = Math.Min(minLat, this.SimbriefRouteLocations.Min(l => l.Latitude));
                    maxLat = Math.Max(maxLat, this.SimbriefRouteLocations.Max(l => l.Latitude));
                    minLon = Math.Min(minLon, this.SimbriefRouteLocations.Min(l => l.Longitude));
                    maxLon = Math.Max(maxLon, this.SimbriefRouteLocations.Max(l => l.Longitude));
                    anyMarkers = true;
                }

                if (this.SimbriefWaypointMarkers.Count > 0)
                {
                    minLat = Math.Min(minLat, this.SimbriefWaypointMarkers.Min(m => m.WayPoint.Latitude));
                    maxLat = Math.Max(maxLat, this.SimbriefWaypointMarkers.Max(m => m.WayPoint.Latitude));
                    minLon = Math.Min(minLon, this.SimbriefWaypointMarkers.Min(m => m.WayPoint.Longitude));
                    maxLon = Math.Max(maxLon, this.SimbriefWaypointMarkers.Max(m => m.WayPoint.Longitude));
                    anyMarkers = true;
                }

                if (this.TrackingEventMarkers.Count > 0)
                {
                    minLat = Math.Min(minLat, this.TrackingEventMarkers.Min(m => m.GeoCoordinate.Latitude));
                    maxLat = Math.Max(maxLat, this.TrackingEventMarkers.Max(m => m.GeoCoordinate.Latitude));
                    minLon = Math.Min(minLon, this.TrackingEventMarkers.Min(m => m.GeoCoordinate.Longitude));
                    maxLon = Math.Max(maxLon, this.TrackingEventMarkers.Max(m => m.GeoCoordinate.Longitude));
                    anyMarkers = true;
                }

                if (anyMarkers)
                {
                    // Add a bit of a margin around the view to zoom into
                    minLat = Math.Max(-90.0, minLat - 0.5);
                    maxLat = Math.Min(90.0, maxLat + 0.5);
                    minLon = Math.Max(-180.0, minLon - 2);
                    maxLon = Math.Min(180, maxLon + 2);

                    UpdateGUIDelegate moveMap = () =>
                    {
                        this.WpfMapView.AnimationLevel = AnimationLevel.None;
                        this.WpfMapView.SetView(new LocationRect(new Location(minLat, minLon), new Location(maxLat, maxLon)));
                        this.WpfMapView.AnimationLevel = AnimationLevel.Full;
                    };
                    this.Dispatcher.BeginInvoke(moveMap);
                    this.userMapInteraction = false;
                }
                else
                {
                    UpdateGUIDelegate resetMap = () =>
                    {
                        this.WpfMapView.AnimationLevel = AnimationLevel.None;
                        this.WpfMapView.SetView(new LocationRect(new Location(80, -50), new Location(-65, 60)));
                        this.WpfMapView.AnimationLevel = AnimationLevel.Full;
                    };
                    this.Dispatcher.BeginInvoke(resetMap);
                    this.userMapInteraction = false;
                }
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Aircraft positions collection changed.
        /// </summary>
        /// <remarks>
        /// sushi.at, 18/11/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Notify collection changed event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void AircraftPositionsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (AircraftPosition item in e.NewItems)
                {
                    var existingMapLayer = item.Parent as MapLayer;
                    existingMapLayer?.Children.Remove(item);

                    this.WpfMapView.Children.Add(item);
                    if (this.ZoomForAircraftLocations)
                    {
                        this.ShowAllMarkers(true);
                    }
                }
            }

            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (AircraftPosition item in e.OldItems)
                {
                    this.WpfMapView.Children.Remove(item);
                    if (this.ZoomForAircraftLocations)
                    {
                        this.ShowAllMarkers();
                    }
                }
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Job trails collection changed.
        /// </summary>
        /// <remarks>
        /// sushi.at, 13/12/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Notify collection changed event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void JobTrailsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (MapPolyline item in e.NewItems)
                {
                    var existingMapLayer = item.Parent as MapLayer;
                    existingMapLayer?.Children.Remove(item);
                    this.WpfMapView.Children.Add(item);

                    if (item.Locations.Count >= 2)
                    {
                        var boxImage = new Image
                        {
                            Width = 24,
                            Height = 24,
                            Source = new BitmapImage(new Uri("pack://application:,,,/OpenSky.Client;component/Resources/job24.png"))
                        };

                        MapLayer.SetPosition(boxImage, item.Locations[0]);
                        MapLayer.SetPositionOrigin(boxImage, PositionOrigin.Center);
                        this.jobTrailAnimationImages.Add(boxImage);
                        this.WpfMapView.Children.Add(boxImage);

                        var animation = new MapPathAnimation(
                            item.Locations,
                            (location, progress, _) =>
                            {
                                MapLayer.SetPosition(boxImage, location);

                                if (progress >= 1.0)
                                {
                                    boxImage.Visibility = Visibility.Collapsed;
                                }
                            },
                            false,
                            2000,
                            true);
                        this.jobTrailAnimations.Add(animation);
                        animation.Play();
                    }
                }
            }

            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (MapPolyline item in e.OldItems)
                {
                    this.WpfMapView.Children.Remove(item);
                }

                // If trails are getting removed, stop all current animations
                foreach (var animation in this.jobTrailAnimations)
                {
                    animation.Stop();
                }

                this.jobTrailAnimations.Clear();
                this.WpfMapView.Children.RemoveRange(this.jobTrailAnimationImages);
                this.jobTrailAnimationImages.Clear();
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

            var simbriefRoute = new MapPolyline { Stroke = new SolidColorBrush(OpenSkyColors.OpenSkySimBrief), StrokeThickness = 4, Locations = this.SimbriefRouteLocations };
            this.WpfMapView.Children.Add(simbriefRoute);

            var aircraftTrail = new MapPolyline { Stroke = new SolidColorBrush(OpenSkyColors.OpenSkyTeal), StrokeThickness = 4, Locations = this.AircraftTrailLocations };
            this.WpfMapView.Children.Add(aircraftTrail);

            if (this.AircraftPositions.Count > 0)
            {
                this.AircraftPositionsCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, this.AircraftPositions));
            }

            if (!this.collectionChangedSubscribed.Contains(this.AircraftPositions))
            {
                this.AircraftPositions.CollectionChanged += this.AircraftPositionsCollectionChanged;
                this.collectionChangedSubscribed.Add(this.AircraftPositions);
            }

            if (this.SimbriefWaypointMarkers.Count > 0)
            {
                this.SimbriefWaypointMarkersCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, this.SimbriefWaypointMarkers));
            }

            if (!this.collectionChangedSubscribed.Contains(this.SimbriefWaypointMarkers))
            {
                this.SimbriefWaypointMarkers.CollectionChanged += this.SimbriefWaypointMarkersCollectionChanged;
                this.collectionChangedSubscribed.Add(this.SimbriefWaypointMarkers);
            }

            if (this.TrackingEventMarkers.Count > 0)
            {
                this.TrackingEventMarkersCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, this.TrackingEventMarkers));
            }

            if (!this.collectionChangedSubscribed.Contains(this.TrackingEventMarkers))
            {
                this.TrackingEventMarkers.CollectionChanged += this.TrackingEventMarkersCollectionChanged;
                this.collectionChangedSubscribed.Add(this.TrackingEventMarkers);
            }

            this.ShowAllMarkers();

            if (this.DataContext is MapViewViewModel viewModel)
            {
                viewModel.Airports.CollectionChanged += this.TrackingEventMarkersCollectionChanged;

                if (this.ShowAllAirports)
                {
                    viewModel.EnableAirportsCommand.DoExecute(null);
                }
            }

            if (this.JobTrails.Count > 0)
            {
                this.JobTrailsCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, this.JobTrails));
            }

            if (!this.collectionChangedSubscribed.Contains(this.JobTrails))
            {
                this.JobTrails.CollectionChanged += this.JobTrailsCollectionChanged;
                this.collectionChangedSubscribed.Add(this.JobTrails);
            }

            this.Legend.Visibility = this.ShowAllAirports ? Visibility.Visible : Visibility.Collapsed;

            // Was the aircraft trail animated?
            if (this.aircraftTrailAnimation != null)
            {
                this.AnimateAircraftTrail();
            }
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
        /// sushi.at, 14/12/2021.
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
            this.aircraftTrailAnimation?.Stop();
            foreach (var animation in this.jobTrailAnimations)
            {
                animation.Stop();
            }

            this.jobTrailAnimations.Clear();

            foreach (var image in this.jobTrailAnimationImages)
            {
                this.WpfMapView.Children.Remove(image);
            }

            if (this.animatedAircraftPosition != null)
            {
                this.WpfMapView.Children.Remove(this.animatedAircraftPosition);
                this.animatedAircraftPosition = null;
            }
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

                    if (BindingOperations.IsDataBound(item, SimbriefWaypointMarker.TextLabelFontSizeProperty))
                    {
                        BindingOperations.ClearBinding(item, SimbriefWaypointMarker.TextLabelFontSizeProperty);
                    }

                    this.WpfMapView.Children.Add(item);

                    // Add zoom level -> visibility and font size bindings with custom converters
                    var zoomLevelBinding = new Binding { Source = this.WpfMapView, Path = new PropertyPath("ZoomLevel"), Converter = ZoomLevelVisibilityConverter, ConverterParameter = 6.0 };
                    BindingOperations.SetBinding(item, SimbriefWaypointMarker.TextLabelVisibleProperty, zoomLevelBinding);
                    var fontSizeBinding = new Binding { Source = this.WpfMapView, Path = new PropertyPath("ZoomLevel"), Converter = ZoomLevelFontSizeConverter };
                    BindingOperations.SetBinding(item, SimbriefWaypointMarker.TextLabelFontSizeProperty, fontSizeBinding);
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
                    if (BindingOperations.IsDataBound(item, VisibilityProperty))
                    {
                        BindingOperations.ClearBinding(item, VisibilityProperty);
                    }

                    if (BindingOperations.IsDataBound(item, TrackingEventMarker.TextLabelFontSizeProperty))
                    {
                        BindingOperations.ClearBinding(item, TrackingEventMarker.TextLabelFontSizeProperty);
                    }

                    this.WpfMapView.Children.Add(item);
                    if (item.IsAirportMarker)
                    {
                        if (item.IsAirportDetailMarker)
                        {
                            var zoomLevelBinding = new Binding { Source = this.WpfMapView, Path = new PropertyPath("ZoomLevel"), Converter = ZoomLevelVisibilityConverter, ConverterParameter = 12.0 };
                            BindingOperations.SetBinding(item, VisibilityProperty, zoomLevelBinding);
                        }
                        else
                        {
                            var zoomLevelBinding = new Binding { Source = this.WpfMapView, Path = new PropertyPath("ZoomLevel"), Converter = ZoomLevelVisibilityConverter, ConverterParameter = -12.0 };
                            BindingOperations.SetBinding(item, VisibilityProperty, zoomLevelBinding);

                            if (item.HasDynamicFontSize)
                            {
                                var fontSizeBinding = new Binding { Source = this.WpfMapView, Path = new PropertyPath("ZoomLevel"), Converter = ZoomLevelFontSizeConverter, ConverterParameter = item.AirportSize >= 5 ? 4 : 2 };
                                BindingOperations.SetBinding(item, TrackingEventMarker.TextLabelFontSizeProperty, fontSizeBinding);

                                var zoomLevelParameter = item.AirportSize switch
                                {
                                    >= 5 => 4.0,
                                    4 => 7.0,
                                    3 => 8.0,
                                    _ => 10.0
                                };

                                var textLabelZoomLevelBinding = new Binding { Source = this.WpfMapView, Path = new PropertyPath("ZoomLevel"), Converter = ZoomLevelVisibilityConverter, ConverterParameter = zoomLevelParameter };
                                BindingOperations.SetBinding(item, TrackingEventMarker.TextLabelVisibleProperty, textLabelZoomLevelBinding);
                            }
                        }

                        if (!this.ShowAllAirports)
                        {
                            this.ShowAllMarkers(true);
                        }
                    }
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
            this.userMapInteraction = true;
            var viewModel = (MapViewViewModel)this.DataContext;
            viewModel.LastUserMapInteraction = DateTime.UtcNow;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// WPF map view on view change end.
        /// </summary>
        /// <remarks>
        /// sushi.at, 08/12/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Map event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void WpfMapViewOnViewChangeEnd(object sender, MapEventArgs e)
        {
            if (this.ShowAllAirports)
            {
                if (this.DataContext is MapViewViewModel viewModel)
                {
                    viewModel.UpdateAirports(this.WpfMapView.ZoomLevel, this.WpfMapView.BoundingRectangle);
                }
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// WPF map view on view change on frame.
        /// </summary>
        /// <remarks>
        /// sushi.at, 08/12/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Map event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void WpfMapViewOnViewChangeOnFrame(object sender, MapEventArgs e)
        {
            if (this.ShowAllAirports && (DateTime.Now - this.lastFrameUpdate).TotalMilliseconds > 500)
            {
                this.lastFrameUpdate = DateTime.Now;

                if (this.DataContext is MapViewViewModel viewModel)
                {
                    viewModel.UpdateAirports(this.WpfMapView.ZoomLevel, this.WpfMapView.BoundingRectangle);
                }
            }
        }
    }
}
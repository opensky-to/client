// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MapPathAnimation.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Controls.Animations
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Threading;

    using JetBrains.Annotations;

    using Microsoft.Maps.MapControl.WPF;

    using OpenSky.Client.Tools;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Map path animation.
    /// </summary>
    /// <remarks>
    /// sushi.at, 14/12/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class MapPathAnimation
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The animation dispatcher delay.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly int delay;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The dispatcher timer.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly DispatcherTimer dispatcherTimer;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The pre-calculated bearings between locations.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private double[] bearings;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The duration of the animation.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private int duration;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The interval locations.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private LocationCollection intervalLocs;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// True to follow the geodesic path, false to follow straight lines.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private bool isGeodesic;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// True if is paused, false if not.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private bool isPaused;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The location collection describing the path to follow for the animation.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private LocationCollection path;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Zero-based index of the frame.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private int frameIndex;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="MapPathAnimation"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 14/12/2021.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the requested operation is invalid.
        /// </exception>
        /// <param name="path">
        /// The location collection describing the path to follow for the animation.
        /// </param>
        /// <param name="intervalCallback">
        /// The interval callback.
        /// </param>
        /// <param name="isGeodesic">
        /// True to follow the geodesic path, false to follow straight lines.
        /// </param>
        /// <param name="duration">
        /// (Optional)
        /// The duration of the animation.
        /// </param>
        /// <param name="loop">
        /// (Optional) True to loop the animation.
        /// </param>
        /// <param name="delay">
        /// (Optional)
        /// The animation dispatcher delay.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public MapPathAnimation([NotNull] LocationCollection path, [CanBeNull] IntervalCallback intervalCallback, bool isGeodesic, int duration = 2000, bool loop = false, int delay = 30)
        {
            if (path.Count < 2)
            {
                throw new InvalidOperationException("Path most contain at least two locations");
            }

            this.path = path;
            this.isGeodesic = isGeodesic;
            this.duration = duration;
            this.delay = delay;

            this.PreCalculate();

            this.dispatcherTimer = new DispatcherTimer(DispatcherPriority.Render)
            {
                Interval = TimeSpan.FromMilliseconds(this.delay),
            };

            this.dispatcherTimer.Tick += (_, _) =>
            {
                if (!this.isPaused)
                {
                    var progress = (double)(this.frameIndex * this.delay) / this.duration;
                    if (progress >= 1.0)
                    {
                        if (loop)
                        {
                            this.frameIndex = 0;
                        }
                        else
                        {
                            this.dispatcherTimer.Stop();
                        }
                    }
                    else
                    {
                        try
                        {
                            intervalCallback?.Invoke(this.intervalLocs[this.frameIndex], progress, this.bearings[this.frameIndex]);
                        }
                        catch
                        {
                            // Ignore
                        }
                    }

                    this.frameIndex++;
                }
            };
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Callback, called when the dispatcher interval ticks.
        /// </summary>
        /// <remarks>
        /// sushi.at, 14/12/2021.
        /// </remarks>
        /// <param name="loc">
        /// The location.
        /// </param>
        /// <param name="progress">
        /// The progress.
        /// </param>
        /// <param name="bearing">
        /// The bearing.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public delegate void IntervalCallback(Location loc, double progress, double bearing);

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the duration of the animation.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int Duration
        {
            get => this.duration;
            set
            {
                this.duration = value;
                this.PreCalculate();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the delay.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int Delay => this.delay;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether to follow the geodesic path.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool IsGeodesic
        {
            get => this.isGeodesic;
            set
            {
                this.isGeodesic = value;
                this.PreCalculate();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the path to follow.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [NotNull]
        public LocationCollection Path
        {
            get => this.path;
            set
            {
                if (value.Count < 2)
                {
                    throw new InvalidOperationException("Path most contain at least two locations");
                }

                this.path = value;
                this.PreCalculate();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Pauses the animation.
        /// </summary>
        /// <remarks>
        /// sushi.at, 14/12/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public void Pause()
        {
            this.isPaused = true;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Plays the animation.
        /// </summary>
        /// <remarks>
        /// sushi.at, 14/12/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public void Play()
        {
            this.isPaused = false;
            this.dispatcherTimer.Start();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Stops the animation.
        /// </summary>
        /// <remarks>
        /// sushi.at, 14/12/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public void Stop()
        {
            if (this.dispatcherTimer.IsEnabled)
            {
                this.isPaused = false;
                this.dispatcherTimer.Stop();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Pre-calculate the path and indices of the animation.
        /// </summary>
        /// <remarks>
        /// sushi.at, 14/12/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void PreCalculate()
        {
            // Stop the timer
            if (this.dispatcherTimer is { IsEnabled: true })
            {
                this.dispatcherTimer.Stop();
            }

            this.intervalLocs = new LocationCollection { this.path[0] };
            var bearingsList = new List<double> { 0.0 };

            double dlat, dlon;
            double totalDistance = 0;

            if (this.isGeodesic)
            {
                // Calculate the total distance along the path in KMs
                for (var i = 0; i < this.path.Count - 1; i++)
                {
                    totalDistance += this.path[i].HaversineDistance(this.path[i + 1]);
                }
            }
            else
            {
                // Calculate the total distance along the path in degrees
                for (var i = 0; i < this.path.Count - 1; i++)
                {
                    dlat = this.path[i + 1].Latitude - this.path[i].Latitude;
                    dlon = this.path[i + 1].Longitude - this.path[i].Longitude;

                    totalDistance += Math.Sqrt(dlat * dlat + dlon * dlon);
                }
            }

            var frameCount = (int)Math.Ceiling((double)this.duration / this.delay);
            var idx = 0;

            // Pre-calculate step points for smoother rendering
            for (var f = 0; f < frameCount; f++)
            {
                var progress = (double)(f * this.delay) / this.duration;

                var travel = progress * totalDistance;
                double alpha = 0;
                double dist = 0;
                var dx = travel;

                for (var i = 0; i < this.path.Count - 1; i++)
                {
                    if (this.isGeodesic)
                    {
                        dist += this.path[i].HaversineDistance(this.path[i + 1]);
                    }
                    else
                    {
                        dlat = this.path[i + 1].Latitude - this.path[i].Latitude;
                        dlon = this.path[i + 1].Longitude - this.path[i].Longitude;
                        alpha = Math.Atan2(dlat * Math.PI / 180, dlon * Math.PI / 180);
                        dist += Math.Sqrt(dlat * dlat + dlon * dlon);
                    }

                    if (dist >= travel)
                    {
                        idx = i;
                        break;
                    }

                    dx = travel - dist;
                }

                if (dx != 0 && idx < this.path.Count - 1)
                {
                    if (this.isGeodesic)
                    {
                        var bearing = this.path[idx].CalculateBearing(this.path[idx + 1]);
                        this.intervalLocs.Add(this.path[idx].CalculateCoord(bearing, dx));

                        if (f > 0)
                        {
                            bearingsList.Add(this.intervalLocs[f - 1].CalculateBearing(this.intervalLocs[f]));
                        }
                    }
                    else
                    {
                        dlat = dx * Math.Sin(alpha);
                        dlon = dx * Math.Cos(alpha);

                        this.intervalLocs.Add(new Location(this.path[idx].Latitude + dlat, this.path[idx].Longitude + dlon));
                        if (f > 0)
                        {
                            bearingsList.Add(this.intervalLocs[f - 1].CalculateBearing(this.intervalLocs[f]));
                        }
                    }
                }
            }

            // Ensure the last location is the last coordinate in the path
            this.intervalLocs.Add(this.path[this.path.Count - 1]);
            bearingsList.Add(this.intervalLocs[this.intervalLocs.Count - 1].CalculateBearing(this.path[this.path.Count - 1]));
            this.bearings = bearingsList.ToArray();
        }
    }
}
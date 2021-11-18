// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Flight.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace
namespace OpenSkyApi
{
    using System;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    public partial class Flight
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the flight paused/active image.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ImageSource FlightPausedImage => new BitmapImage(new Uri($"pack://application:,,,/OpenSky.Client;component/Resources/{(this.Paused.HasValue ? "pause16.png" : "departure16.png")}"));

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the flight position report image.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ImageSource FlightPositionImage => new BitmapImage(
            new Uri(
                $"pack://application:,,,/OpenSky.Client;component/Resources/{((this.Latitude.HasValue && this.Longitude.HasValue && this.LastPositionReport.HasValue) ? (((DateTime.UtcNow - this.LastPositionReport.Value).TotalMinutes > 5) ? "question16.png" : "pin16.png") : "x16.png")}"));

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the flight saved image.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ImageSource FlightSavedImage => new BitmapImage(new Uri($"pack://application:,,,/OpenSky.Client;component/Resources/{(this.HasAutoSaveLog ? "save16.png" : "x16.png")}"));

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets status information describing the flight (ground?, speed, altitude, heading).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string FlightStatus
        {
            get
            {
                if (this.OnGround)
                {
                    return $"On ground, {this.GroundSpeed ?? 0:F0} kts";
                }

                return $"Airborne {this.Altitude ?? 0:F0} ft, {this.AirspeedTrue ?? 0:F0} kts, heading {this.Heading ?? 0:F0}";
            }
        }
    }
}
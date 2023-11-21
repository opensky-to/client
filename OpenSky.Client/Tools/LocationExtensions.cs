// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LocationExtensions.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Tools
{
    using System;

    using Microsoft.Maps.MapControl.WPF;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Location extension methods.
    /// </summary>
    /// <remarks>
    /// sushi.at, 14/12/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public static class LocationExtensions
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The earth radius in kilometers.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private const double EarthRadiusKm = 6378.1;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// A Location extension method that calculates the bearing between two locations.
        /// </summary>
        /// <remarks>
        /// sushi.at, 14/12/2021.
        /// </remarks>
        /// <param name="origin">
        /// The origin.
        /// </param>
        /// <param name="dest">
        /// The destination.
        /// </param>
        /// <returns>
        /// The calculated bearing.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public static double CalculateBearing(this Location origin, Location dest)
        {
            var lat1 = origin.Latitude.DegToRad();
            var lon1 = origin.Longitude;
            var lat2 = dest.Latitude.DegToRad();
            var lon2 = dest.Longitude;
            var dLon = DegToRad(lon2 - lon1);
            var y = Math.Sin(dLon) * Math.Cos(lat2);
            var x = Math.Cos(lat1) * Math.Sin(lat2) - Math.Sin(lat1) * Math.Cos(lat2) * Math.Cos(dLon);
            return (RadToDeg(Math.Atan2(y, x)) + 360) % 360;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// A Location extension method that calculates the coordinate from a location vector.
        /// </summary>
        /// <remarks>
        /// sushi.at, 14/12/2021.
        /// </remarks>
        /// <param name="origin">
        /// The origin.
        /// </param>
        /// <param name="brng">
        /// The bearing.
        /// </param>
        /// <param name="arcLength">
        /// The arc length of the vector.
        /// </param>
        /// <returns>
        /// The calculated coordinate.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public static Location CalculateCoord(this Location origin, double brng, double arcLength)
        {
            double lat1 = origin.Latitude.DegToRad(),
                lon1 = origin.Longitude.DegToRad(),
                centralAngle = arcLength / EarthRadiusKm;

            var lat2 = Math.Asin(Math.Sin(lat1) * Math.Cos(centralAngle) + Math.Cos(lat1) * Math.Sin(centralAngle) * Math.Cos(brng.DegToRad()));
            var lon2 = lon1 + Math.Atan2(Math.Sin(brng.DegToRad()) * Math.Sin(centralAngle) * Math.Cos(lat1), Math.Cos(centralAngle) - Math.Sin(lat1) * Math.Sin(lat2));

            return new Location(lat2.RadToDeg(), lon2.RadToDeg());
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// A double extension method that converts degrees to radians.
        /// </summary>
        /// <remarks>
        /// sushi.at, 14/12/2021.
        /// </remarks>
        /// <param name="x">
        /// The degrees.
        /// </param>
        /// <returns>
        /// The radians.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public static double DegToRad(this double x)
        {
            return x * Math.PI / 180;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// A Location extension method that calculates the Haversine distance between two locations.
        /// </summary>
        /// <remarks>
        /// sushi.at, 14/12/2021.
        /// </remarks>
        /// <param name="origin">
        /// The origin.
        /// </param>
        /// <param name="dest">
        /// The destination.
        /// </param>
        /// <returns>
        /// The Haversine distance between two locations in KM.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public static double HaversineDistance(this Location origin, Location dest)
        {
            double lat1 = origin.Latitude.DegToRad(),
                lon1 = origin.Longitude.DegToRad(),
                lat2 = dest.Latitude.DegToRad(),
                lon2 = dest.Longitude.DegToRad();

            double dLat = lat2 - lat1,
                dLon = lon2 - lon1,
                cordLength = Math.Pow(Math.Sin(dLat / 2), 2) + Math.Cos(lat1) * Math.Cos(lat2) * Math.Pow(Math.Sin(dLon / 2), 2),
                centralAngle = 2 * Math.Atan2(Math.Sqrt(cordLength), Math.Sqrt(1 - cordLength));

            return EarthRadiusKm * centralAngle;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// A double extension method that converts radians to degrees.
        /// </summary>
        /// <remarks>
        /// sushi.at, 14/12/2021.
        /// </remarks>
        /// <param name="x">
        /// The radians.
        /// </param>
        /// <returns>
        /// The degrees.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public static double RadToDeg(this double x)
        {
            return x * 180 / Math.PI;
        }
    }
}
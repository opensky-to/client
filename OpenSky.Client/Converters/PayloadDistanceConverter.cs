﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PayloadDistanceConverter.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Converters
{
    using System;
    using System.Device.Location;
    using System.Globalization;
    using System.Linq;
    using System.Windows.Data;

    using OpenSky.Client.Tools;

    using OpenSkyApi;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Payload distance converter.
    /// </summary>
    /// <remarks>
    /// sushi.at, 20/12/2021.
    /// </remarks>
    /// <seealso cref="T:System.Windows.Data.IValueConverter"/>
    /// -------------------------------------------------------------------------------------------------
    public class PayloadDistanceConverter : IValueConverter
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <remarks>
        /// sushi.at, 20/12/2021.
        /// </remarks>
        /// <param name="value">
        /// The value produced by the binding source.
        /// </param>
        /// <param name="targetType">
        /// The type of the binding target property.
        /// </param>
        /// <param name="parameter">
        /// The converter parameter to use.
        /// </param>
        /// <param name="culture">
        /// The culture to use in the converter.
        /// </param>
        /// <returns>
        /// A converted value. If the method returns <see langword="null" />, the valid null value is
        /// used.
        /// </returns>
        /// <seealso cref="M:System.Windows.Data.IValueConverter.Convert(object,Type,object,CultureInfo)"/>
        /// -------------------------------------------------------------------------------------------------
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Payload payload)
            {
                if (!string.IsNullOrEmpty(payload.AirportICAO))
                {
                    var airportPackage = AirportPackageClientHandler.GetPackage();
                    var origin = airportPackage?.Airports.SingleOrDefault(a => a.ICAO == payload.AirportICAO);
                    var destination = airportPackage?.Airports.SingleOrDefault(a => a.ICAO == payload.DestinationICAO);

                    if (origin != null && destination != null)
                    {
                        var distance = new GeoCoordinate(origin.Latitude, origin.Longitude).GetDistanceTo(new GeoCoordinate(destination.Latitude, destination.Longitude)) / 1852;
                        if (targetType == typeof(string))
                        {
                            return new SettingsUnitConverter().Convert(distance, typeof(string), "distance|F0|true", CultureInfo.CurrentCulture);
                        }

                        return distance;
                    }
                }

                if (!string.IsNullOrEmpty(payload.AircraftRegistry))
                {
                    var airportPackage = AirportPackageClientHandler.GetPackage();
                    var destination = airportPackage?.Airports.SingleOrDefault(a => a.ICAO == payload.DestinationICAO);

                    if (destination != null)
                    {
                        var distance = new GeoCoordinate(payload.AircraftLatitude ?? 0, payload.AircraftLongitude ?? 0).GetDistanceTo(new GeoCoordinate(destination.Latitude, destination.Longitude)) / 1852;
                        
                        if (targetType == typeof(string))
                        {
                            return new SettingsUnitConverter().Convert(distance, typeof(string), "distance|F0|true", CultureInfo.CurrentCulture);
                        }

                        return distance;
                    }
                }
            }

            if (targetType == typeof(string))
            {
                return new SettingsUnitConverter().Convert(0.0, typeof(string), "distance|F0|true", CultureInfo.CurrentCulture);
            }

            return 0.0;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <remarks>
        /// sushi.at, 20/12/2021.
        /// </remarks>
        /// <param name="value">
        /// The value that is produced by the binding target.
        /// </param>
        /// <param name="targetType">
        /// The type to convert to.
        /// </param>
        /// <param name="parameter">
        /// The converter parameter to use.
        /// </param>
        /// <param name="culture">
        /// The culture to use in the converter.
        /// </param>
        /// <returns>
        /// A converted value. If the method returns <see langword="null" />, the valid null value is
        /// used.
        /// </returns>
        /// <seealso cref="M:System.Windows.Data.IValueConverter.ConvertBack(object,Type,object,CultureInfo)"/>
        /// -------------------------------------------------------------------------------------------------
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
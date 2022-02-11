// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AircraftRegistryConverter.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    using OpenSky.Client.Tools;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Aircraft registry converter (removes simulator prefix)
    /// </summary>
    /// <remarks>
    /// sushi.at, 08/02/2022.
    /// </remarks>
    /// <seealso cref="System.Windows.Data.IValueConverter"/>
    /// -------------------------------------------------------------------------------------------------
    public class AircraftRegistryConverter : IValueConverter
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <remarks>
        /// sushi.at, 08/02/2022.
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
        /// <seealso cref="IValueConverter.Convert(object,Type,object,CultureInfo)"/>
        /// -------------------------------------------------------------------------------------------------
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string registry)
            {
                return registry.RemoveSimPrefix();
            }

            return value;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Converts a value - not supported.
        /// </summary>
        /// <remarks>
        /// sushi.at, 08/02/2022.
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
        /// <seealso cref="IValueConverter.ConvertBack(object,Type,object,CultureInfo)"/>
        /// -------------------------------------------------------------------------------------------------
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LargeNumberConverter.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Large number converter, 8531780 => 8.5m, 11580 => 11.6k.
    /// </summary>
    /// <remarks>
    /// sushi.at, 04/12/2023.
    /// </remarks>
    /// <seealso cref="System.Windows.Data.IValueConverter"/>
    /// -------------------------------------------------------------------------------------------------
    public class LargeNumberConverter : IValueConverter
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <remarks>
        /// sushi.at, 04/12/2023.
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
            var valueToConvert = double.NaN;
            if (value is long longValue)
            {
                valueToConvert = longValue;
            }

            if (value is double doubleValue)
            {
                valueToConvert = doubleValue;
            }

            if (value is int intValue)
            {
                valueToConvert = intValue;
            }

            if (!double.IsNaN(valueToConvert))
            {
                // Trillions, seriously?
                if (valueToConvert > Math.Pow(10, 12))
                {
                    valueToConvert /= Math.Pow(10, 12);
                    valueToConvert = Math.Round(valueToConvert, 1, MidpointRounding.AwayFromZero);
                    return $"{valueToConvert}t";
                }

                // Billions, good job
                if (valueToConvert > Math.Pow(10, 9))
                {
                    valueToConvert /= Math.Pow(10, 9);
                    valueToConvert = Math.Round(valueToConvert, 1, MidpointRounding.AwayFromZero);
                    return $"{valueToConvert}b";
                }

                // Millions
                if (valueToConvert > Math.Pow(10, 6))
                {
                    valueToConvert /= Math.Pow(10, 6);
                    valueToConvert = Math.Round(valueToConvert, 1, MidpointRounding.AwayFromZero);
                    return $"{valueToConvert}m";
                }

                // Thousands
                if (valueToConvert > Math.Pow(10, 3))
                {
                    valueToConvert /= Math.Pow(10, 3);
                    valueToConvert = Math.Round(valueToConvert, 1, MidpointRounding.AwayFromZero);
                    return $"{valueToConvert}k";
                }

                // Below 1k
                return $"{valueToConvert}";
            }

            return value;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Converts a value - not supported.
        /// </summary>
        /// <remarks>
        /// sushi.at, 04/12/2023.
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
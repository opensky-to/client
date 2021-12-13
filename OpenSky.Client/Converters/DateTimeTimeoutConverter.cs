// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DateTimeTimeoutConverter.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using System.Windows.Media;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// DateTime timeout converter.
    /// </summary>
    /// <remarks>
    /// sushi.at, 13/12/2021.
    /// </remarks>
    /// <seealso cref="T:System.Windows.Data.IValueConverter"/>
    /// -------------------------------------------------------------------------------------------------
    public class DateTimeTimeoutConverter : IValueConverter
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <remarks>
        /// sushi.at, 13/12/2021.
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
            if (value is DateTime dateTime)
            {
                var utc = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
                var timeout = utc - DateTime.UtcNow;

                // Return color instead of string
                if (parameter is string para && para.Equals("color", StringComparison.InvariantCultureIgnoreCase))
                {
                    var color = timeout.TotalHours switch
                    {
                        > 24.0 => OpenSkyColors.OpenSkyTeal,
                        > 4.0 => OpenSkyColors.OpenSkyLightYellow,
                        > 1.0 => OpenSkyColors.OpenSkyWarningOrange,
                        < 0.0 => Colors.Black,
                        _ => Colors.DarkRed,
                    };
                    return new SolidColorBrush(color);
                }

                return timeout.TotalDays > 1.0 ? $"{timeout.Days}d {timeout:hh\\:mm}" : timeout.ToString("hh\\:mm");
            }

            if (value is DateTimeOffset offset)
            {
                var utc = offset.UtcDateTime;
                var timeout = utc - DateTime.UtcNow;

                // Return color instead of string
                if (parameter is string para && para.Equals("color", StringComparison.InvariantCultureIgnoreCase))
                {
                    var color = timeout.TotalHours switch
                    {
                        > 24.0 => OpenSkyColors.OpenSkyTeal,
                        > 4.0 => OpenSkyColors.OpenSkyLightYellow,
                        > 1.0 => OpenSkyColors.OpenSkyWarningOrange,
                        < 0.0 => Colors.Black,
                        _ => Colors.DarkRed,
                    };
                    return new SolidColorBrush(color);
                }

                return timeout.TotalDays > 1.0 ? $"{timeout.Days}d {timeout:hh\\:mm}" : timeout.ToString("hh\\:mm");
            }

            return null;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Converts a value - not supported.
        /// </summary>
        /// <remarks>
        /// sushi.at, 13/12/2021.
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
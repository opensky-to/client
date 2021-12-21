// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SettingsUnitConverter.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Converters
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Windows.Data;

    using OpenSky.Client.Models.Enums;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Weight unit converter (based on settings)
    /// </summary>
    /// <remarks>
    /// sushi.at, 26/07/2021.
    /// </remarks>
    /// <seealso cref="T:System.Windows.Data.IValueConverter"/>
    /// -------------------------------------------------------------------------------------------------
    public class SettingsUnitConverter : IValueConverter
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <remarks>
        /// sushi.at, 26/07/2021.
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
            try
            {
                if (value == null)
                {
                    return string.Empty;
                }

                if (parameter is string settings)
                {
                    var settingsSplit = settings.Split('|');
                    if (settingsSplit.Length != 3)
                    {
                        return "Settings error";
                    }

                    var unit = settingsSplit[0];
                    var format = settingsSplit[1];
                    var showUnit = bool.Parse(settingsSplit[2]);
                    string unitName;

                    var unitValue = value switch
                    {
                        double dValue => dValue,
                        int iValue => iValue,
                        _ => throw new Exception("Unsupported value type")
                    };

                    switch (unit.ToLowerInvariant())
                    {
                        case "weight":
                            unitValue = (WeightUnit)Properties.Settings.Default.WeightUnit switch
                            {
                                WeightUnit.lbs => unitValue,
                                WeightUnit.kg => unitValue * 0.453592,
                                _ => throw new Exception("Unsupported weight unit")
                            };
                            unitName = (WeightUnit)Properties.Settings.Default.WeightUnit switch
                            {
                                WeightUnit.lbs => " lbs",
                                WeightUnit.kg => " kg",
                                _ => throw new Exception("Unsupported weight unit")
                            };
                            break;
                        case "fuel":
                            unitValue = (FuelUnit)Properties.Settings.Default.FuelUnit switch
                            {
                                FuelUnit.gal => unitValue,
                                FuelUnit.l => unitValue * 3.78541,
                                _ => throw new Exception("Unsupported fuel unit")
                            };
                            unitName = (FuelUnit)Properties.Settings.Default.FuelUnit switch
                            {
                                FuelUnit.gal => " gal",
                                FuelUnit.l => " l",
                                _ => throw new Exception("Unsupported fuel unit")
                            };
                            break;
                        case "distance":
                            unitValue = (DistanceUnit)Properties.Settings.Default.DistanceUnit switch
                            {
                                DistanceUnit.nm => unitValue,
                                DistanceUnit.km => unitValue * 1.852,
                                _ => throw new Exception("Unsupported distance unit")
                            };
                            unitName = (DistanceUnit)Properties.Settings.Default.DistanceUnit switch
                            {
                                DistanceUnit.nm => " nm",
                                DistanceUnit.km => " km",
                                _ => throw new Exception("Unsupported distance unit")
                            };
                            break;
                        default:
                            return "Unsupported unit";
                    }

                    if (!showUnit)
                    {
                        unitName = string.Empty;
                    }

                    return $"{unitValue.ToString(format, CultureInfo.InvariantCulture)}{unitName}";
                }

                return "Settings error";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Converts a value - not supported.
        /// </summary>
        /// <remarks>
        /// sushi.at, 26/07/2021.
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
            try
            {
                if (parameter is string settings)
                {
                    var settingsSplit = settings.Split('|');
                    if (settingsSplit.Length != 3)
                    {
                        return "Settings error";
                    }

                    var unit = settingsSplit[0];
                    if (value is string valueString)
                    {
                        string unitName;
                        double unitValue;
                        switch (unit.ToLowerInvariant())
                        {
                            case "weight":
                                unitName = (WeightUnit)Properties.Settings.Default.WeightUnit switch
                                {
                                    WeightUnit.lbs => "lbs",
                                    WeightUnit.kg => "kg",
                                    _ => throw new Exception("Unsupported weight unit")
                                };
                                valueString = valueString.Replace(unitName, string.Empty).Trim();
                                unitValue = double.Parse(valueString);

                                unitValue = (WeightUnit)Properties.Settings.Default.WeightUnit switch
                                {
                                    WeightUnit.lbs => unitValue,
                                    WeightUnit.kg => unitValue * 2.20462,
                                    _ => throw new Exception("Unsupported weight unit")
                                };
                                break;
                            case "fuel":
                                unitName = (FuelUnit)Properties.Settings.Default.FuelUnit switch
                                {
                                    FuelUnit.gal => "gal",
                                    FuelUnit.l => "l",
                                    _ => throw new Exception("Unsupported fuel unit")
                                };
                                valueString = valueString.Replace(unitName, string.Empty).Trim();
                                unitValue = double.Parse(valueString);

                                unitValue = (FuelUnit)Properties.Settings.Default.FuelUnit switch
                                {
                                    FuelUnit.gal => unitValue,
                                    FuelUnit.l => unitValue * 0.264172,
                                    _ => throw new Exception("Unsupported fuel unit")
                                };
                                break;
                            case "distance":
                                unitName = (DistanceUnit)Properties.Settings.Default.DistanceUnit switch
                                {
                                    DistanceUnit.nm => "nm",
                                    DistanceUnit.km => "km",
                                    _ => throw new Exception("Unsupported distance unit")
                                };
                                valueString = valueString.Replace(unitName, string.Empty).Trim();
                                unitValue = double.Parse(valueString);

                                unitValue = (DistanceUnit)Properties.Settings.Default.DistanceUnit switch
                                {
                                    DistanceUnit.nm => unitValue,
                                    DistanceUnit.km => unitValue * 0.539957,
                                    _ => throw new Exception("Unsupported distance unit")
                                };
                                break;
                            default:
                                return "Unsupported unit";
                        }

                        return unitValue;
                    }
                }

                return 0.0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return 0.0;
            }
        }
    }
}
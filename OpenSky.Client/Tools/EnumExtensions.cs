// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EnumExtensions.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Tools
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Enum extension methods.
    /// </summary>
    /// <remarks>
    /// sushi.at, 25/07/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public static class EnumExtensions
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets enum value description text from DescriptionAttribute.
        /// </summary>
        /// <remarks>
        /// sushi.at, 25/07/2021.
        /// </remarks>
        /// <param name="e">
        /// The enum value to act on.
        /// </param>
        /// <returns>
        /// The description text or NULL.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public static string GetDescription<T>(this T e) where T : IConvertible
        {
            if (e is Enum)
            {
                var type = e.GetType();
                var values = Enum.GetValues(type);

                foreach (int val in values)
                {
                    if (val == e.ToInt32(CultureInfo.InvariantCulture))
                    {
                        var memInfo = type.GetMember(type.GetEnumName(val) ?? string.Empty);
                        if (memInfo.Length > 0 && memInfo[0]
                                                  .GetCustomAttributes(typeof(DescriptionAttribute), false)
                                                  .FirstOrDefault() is DescriptionAttribute descriptionAttribute)
                        {
                            return descriptionAttribute.Description;
                        }
                    }
                }
            }

            return null;
        }
    }
}
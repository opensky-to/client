// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StringEnumExtension.cs" company="OpenSky">
// sushi.at for OpenSky 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Tools
{
    using System;

    using JetBrains.Annotations;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Extension method for retrieving string enum values specified using <see cref="StringValueAttribute" /> attributes.
    /// </summary>
    /// <remarks>
    /// sushi.at, 19/03/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public static class StringEnumExtension
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Will get the string value for a given enums value, this will only work if you assign the StringValue attribute to the items in your enum.
        /// </summary>
        /// <remarks>
        /// sushi.at, 19/03/2021.
        /// </remarks>
        /// <param name="value">
        /// The enum value to inspect for a string value.
        /// </param>
        /// <returns>
        /// The string value of the enum value, or NULL if no string value was found.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [CanBeNull]
        public static string GetStringValue([NotNull] this Enum value)
        {
            // Get the type
            var type = value.GetType();

            // Get field info for this type
            var fieldInfo = type.GetField(value.ToString());

            // Get the StringValue attributes
            var attributes =
                fieldInfo.GetCustomAttributes(typeof(StringValueAttribute), false) as StringValueAttribute[];

            // Return the first if there was a match.
            return attributes?.Length > 0 ? attributes[0].StringValue : null;
        }
    }
}
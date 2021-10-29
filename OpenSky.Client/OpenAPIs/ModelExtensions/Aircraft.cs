// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Aircraft.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace

namespace OpenSkyApi
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Aircraft extensions.
    /// </summary>
    /// <remarks>
    /// sushi.at, 29/10/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public partial class Aircraft
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <remarks>
        /// sushi.at, 29/10/2021.
        /// </remarks>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        /// <seealso cref="M:System.Object.ToString()"/>
        /// -------------------------------------------------------------------------------------------------
        public override string ToString()
        {
            var displayString = $"{this.Registry}";
            if (!string.IsNullOrEmpty(this.Name))
            {
                displayString += $" ({this.Name})";
            }

            displayString += $": {this.Type.Name}";
            return displayString;
        }
    }
}
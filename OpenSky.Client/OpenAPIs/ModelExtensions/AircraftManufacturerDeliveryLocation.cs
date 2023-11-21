// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AircraftManufacturerDeliveryLocation.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace

namespace OpenSkyApi
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Aircraft manufacturer delivery location - local extensions.
    /// </summary>
    /// <remarks>
    /// sushi.at, 20/02/2022.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public partial class AircraftManufacturerDeliveryLocation
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <remarks>
        /// sushi.at, 20/02/2022.
        /// </remarks>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        /// <seealso cref="System.Object.ToString()"/>
        /// -------------------------------------------------------------------------------------------------
        public override string ToString()
        {
            return $"{this.AirportICAO}: {this.AirportName}";
        }
    }
}
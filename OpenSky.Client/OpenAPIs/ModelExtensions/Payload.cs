// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Payload.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace

namespace OpenSkyApi
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Payload extensions.
    /// </summary>
    /// <remarks>
    /// sushi.at, 18/12/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public partial class Payload
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets information describing the current location and destination.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string LocationDestinationInfo
        {
            get
            {
                if (!string.IsNullOrEmpty(this.AirportICAO))
                {
                    return $"{this.AirportICAO} ▷ {this.DestinationICAO}";
                }

                if (!string.IsNullOrEmpty(this.AircraftRegistry))
                {
                    return $"{this.AircraftRegistry} ▷ {this.DestinationICAO}";
                }

                return $"??? ▷ {this.DestinationICAO}";
            }
        }
    }
}
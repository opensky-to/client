// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AirportClientPackageRunwayEntry.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Models
{
    using System.Collections.Generic;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Airport client package runway entries for airports.
    /// </summary>
    /// <remarks>
    /// sushi.at, 01/12/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class AirportClientPackageRunwayEntry
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether this object has lighting.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool HasLighting { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the length of the runway in feet.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int Length { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the runway ends.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public List<AirportClientPackageRunwayEndEntry> RunwayEnds { get; set; }
    }
}
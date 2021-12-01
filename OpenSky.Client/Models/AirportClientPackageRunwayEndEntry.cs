﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AirportClientPackageRunwayEndEntry.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Models
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Airport client package runway end entries for airports.
    /// </summary>
    /// <remarks>
    /// sushi.at, 01/12/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class AirportClientPackageRunwayEndEntry
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether the runway end has closed markings painted on it.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool HasClosedMarkings { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the latitude.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double Latitude { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the longitude.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double Longitude { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the name (for example 04L).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string Name { get; set; }
    }
}
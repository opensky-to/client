// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Job.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace

namespace OpenSkyApi
{
    using System.Linq;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Job extensions.
    /// </summary>
    /// <remarks>
    /// sushi.at, 13/12/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public partial class Job
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the destination(s) of the payload(s).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string Destinations => this.Payloads.Select(p => p.DestinationICAO).Distinct().OrderBy(d => d).Aggregate(string.Empty, (current, destination) => current + $"{destination}, ").TrimEnd(' ', ',');

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the total payload weight.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double TotalWeight => this.Payloads.Sum(p => p.Weight);
    }
}
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Job.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace

namespace OpenSkyApi
{
    using System.Globalization;
    using System.Linq;

    using OpenSky.Client.Converters;

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
        /// Gets the maximum distance of any payload within the job.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double MaxDistance
        {
            get
            {
                var distanceConverter = new PayloadDistanceConverter();
                var maxDistance = 0.0;

                foreach (var payload in this.Payloads)
                {
                    var distanceObj = distanceConverter.Convert(payload, typeof(double), null, CultureInfo.CurrentCulture);
                    if (distanceObj is double distance && distance > maxDistance)
                    {
                        maxDistance = distance;
                    }
                }

                return maxDistance;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the total payload weight.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double TotalWeight => this.Payloads.Sum(p => p.Weight);
    }
}
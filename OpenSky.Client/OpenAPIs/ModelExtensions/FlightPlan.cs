// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FlightPlan.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace
namespace OpenSkyApi
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Flight plan extensions.
    /// </summary>
    /// <remarks>
    /// sushi.at, 11/11/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public partial class FlightPlan
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether this object is new flight plan.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool IsNewFlightPlan { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the total payload weight in lbs.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double TotalPayloadWeight
        {
            get
            {
                var totalWeight = 0.0;
                if (this.Payloads != null)
                {
                    foreach (var flightPayload in this.Payloads)
                    {
                        totalWeight += flightPayload.Payload?.Weight ?? 0;
                    }
                }

                return totalWeight;
            }
        }
    }
}
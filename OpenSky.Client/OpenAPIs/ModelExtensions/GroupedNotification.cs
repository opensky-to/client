// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GroupedNotification.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace

namespace OpenSkyApi
{
    using System.Linq;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Grouped notification extensions.
    /// </summary>
    /// <remarks>
    /// sushi.at, 19/12/2023.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public partial class GroupedNotification
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the delivery status.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string DeliveryStatus
        {
            get
            {
                var delivered = this.Recipients.Count(r => r.ClientPickup || r.AgentPickup || r.EmailSent);
                return $"{delivered} / {this.Recipients.Count}";
            }
        }
    }
}
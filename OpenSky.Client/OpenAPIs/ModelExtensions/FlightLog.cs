// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FlightLog.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace

namespace OpenSkyApi
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Flight log extensions.
    /// </summary>
    /// <remarks>
    /// sushi.at, 15/11/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public partial class FlightLog
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the landed at info.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string LandedAt
        {
            get
            {
                if (this.Crashed)
                {
                    return "**Crashed**";
                }

                return this.LandedAtICAO;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the off block info.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string OffBlock
        {
            get
            {
                var plan = this.PlannedDepartureTime.UtcDateTime;
                var start = this.Started.UtcDateTime;
                var delta = (start - plan).TotalHours;
                if (delta is < 0 and > -24)
                {
                    return $"-{start - plan:hh\\:mm} ({start:HH:mmZ})";
                }

                if (delta is > 0 and < 24)
                {
                    return $"+{start - plan:hh\\:mm} ({start:HH:mmZ})";
                }

                return $"{start:dd/MM HH:mmZ}";
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the on block info.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string OnBlock
        {
            get
            {
                var start = this.Started.UtcDateTime;
                var end = this.Completed.UtcDateTime;
                var delta = (end - start).TotalHours;

                if (delta < 24)
                {
                    return $"{end - start:hh\\:mm} ({end:HH:mmZ})";
                }

                return $"{end:dd/MM HH:mmZ}";
            }
        }
    }
}
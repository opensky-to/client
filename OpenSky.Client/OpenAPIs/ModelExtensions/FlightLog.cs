// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FlightLog.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace

namespace OpenSkyApi
{
    using System;

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
        /// Gets the fuel consumed weight.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double FuelConsumedWeight => this.FuelConsumption * this.FuelWeightPerGallon;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the landed at airport name or "", if crashed.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string LandedAtCrash => this.Crashed ? string.Empty : this.LandedAt;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the landed at ICAO or **Crashed**.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string LandedAtICAOCrash => this.Crashed ? "**Crashed**" : this.LandedAtICAO;

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
        /// Gets the off block fuel weight.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double OffBlockFuelWeight => this.OffBlockFuel * this.FuelWeightPerGallon;

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

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the on block fuel weight.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double OnBlockFuelWeight => this.OnBlockFuel * this.FuelWeightPerGallon;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the online network duration info.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string OnlineNetworkDuration
        {
            get
            {
                if (this.OnlineNetwork != OnlineNetwork.Offline)
                {
                    var duration = TimeSpan.FromSeconds(this.OnlineNetworkConnectedSeconds);
                    var percentageOfFlightTime = this.OnlineNetworkConnectedSeconds / ((this.Completed - this.Started).TotalSeconds) * 100.0;

                    return $"{duration}, {percentageOfFlightTime:N1} % of flight";
                }

                return string.Empty;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets information describing the time warp.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string TimeWarpInfo
        {
            get
            {
                if (this.TimeWarpTimeSavedSeconds == 0)
                {
                    return "No";
                }

                return $"Yes, saved {TimeSpan.FromSeconds(this.TimeWarpTimeSavedSeconds)}";
            }
        }
    }
}
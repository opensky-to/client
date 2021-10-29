// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DateTimeExtensions.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Extensions
{
    using System;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// DateTime extension methods.
    /// </summary>
    /// <remarks>
    /// sushi.at, 29/10/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public static class DateTimeExtensions
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// A DateTime extension method that round up.
        /// </summary>
        /// <remarks>
        /// sushi.at, 29/10/2021.
        /// </remarks>
        /// <param name="dt">
        /// The datetime to act on.
        /// </param>
        /// <param name="d">
        /// A TimeSpan to specify what to round to (ex. 15 minutes).
        /// </param>
        /// <returns>
        /// The rounded-up datetime.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public static DateTime RoundUp(this DateTime dt, TimeSpan d)
        {
            return new DateTime((dt.Ticks + d.Ticks - 1) / d.Ticks * d.Ticks, dt.Kind);
        }
    }
}
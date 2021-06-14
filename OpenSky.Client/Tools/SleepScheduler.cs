// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SleepScheduler.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Tools
{
    using System;
    using System.Threading;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Sleep scheduler (interrupts long running sleep operations).
    /// </summary>
    /// <remarks>
    /// sushi.at, 12/03/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public static class SleepScheduler
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets a value indicating whether a shutdown is in progress.
        /// </summary>
        /// <value>
        /// true if this object is shutdown in progress, false if not.
        /// </value>
        /// -------------------------------------------------------------------------------------------------
        public static bool IsShutdownInProgress { get; private set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Signal the scheduler that we are shutting down.
        /// </summary>
        /// <remarks>
        /// sushi.at, 12/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public static void Shutdown()
        {
            IsShutdownInProgress = true;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Sleep for specified time span (interrupts if server shuts down).
        /// </summary>
        /// <remarks>
        /// sushi.at, 12/03/2021.
        /// </remarks>
        /// <param name="timeSpan">
        /// The amount of time to sleep.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public static void SleepFor(TimeSpan timeSpan)
        {
            if (IsShutdownInProgress)
            {
                return;
            }

            var startSleep = DateTime.Now;

            while ((DateTime.Now - startSleep) < timeSpan)
            {
                if (IsShutdownInProgress)
                {
                    return;
                }

                Thread.Sleep(5000);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Sleep until specified target time (interrupts if server shuts down).
        /// </summary>
        /// <remarks>
        /// sushi.at, 12/03/2021.
        /// </remarks>
        /// <param name="targetTime">
        /// The target time at which the method should wake up.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public static void SleepUntil(DateTime targetTime)
        {
            if (IsShutdownInProgress)
            {
                return;
            }

            while (DateTime.Now < targetTime)
            {
                if (IsShutdownInProgress)
                {
                    return;
                }

                Thread.Sleep(5000);
            }
        }
    }
}
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TrackingEventLogEntry.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Controls.Models
{
    using System.Windows.Media;

    using JetBrains.Annotations;

    using Microsoft.Maps.MapControl.WPF;

    using OpenSky.Client.Tools;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// OpenSky tracking event.
    /// </summary>
    /// <remarks>
    /// sushi.at, 16/03/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class TrackingEventLogEntry : FlightLogXML.TrackingEventLogEntry
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="TrackingEventLogEntry"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 17/11/2021.
        /// </remarks>
        /// <param name="entry">
        /// The entry, restored from a flight log xml file.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public TrackingEventLogEntry(FlightLogXML.TrackingEventLogEntry entry)
        {
            this.EventType = entry.EventType;
            this.EventTime = entry.EventTime;

            this.Latitude = entry.Latitude;
            this.Longitude = entry.Longitude;
            this.Altitude = entry.Altitude;

            this.EventColor = entry.EventColor;
            this.LogMessage = entry.LogMessage;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the color brush of the event.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Brush EventColorBrush => new SolidColorBrush(this.EventColor.ToMediaColor());

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the location of the event.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [NotNull]
        public Location Location => new(this.Latitude, this.Longitude, this.Altitude);
    }
}
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AircraftTrailLocation.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Controls.Models
{
    using Microsoft.Maps.MapControl.WPF;

    using OpenSky.FlightLogXML;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// An aircraft trail location.
    /// </summary>
    /// <remarks>
    /// sushi.at, 25/03/2021.
    /// </remarks>
    /// <seealso cref="T:Microsoft.Maps.MapControl.WPF.Location"/>
    /// -------------------------------------------------------------------------------------------------
    public class AircraftTrailLocation : Location
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="AircraftTrailLocation"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 17/11/2021.
        /// </remarks>
        /// <param name="position">
        /// The position report we are wrapping around, restored from a flight log xml file.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public AircraftTrailLocation(PositionReport position) : base(position.Latitude, position.Longitude, position.Altitude)
        {
            this.Position = position;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the position.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public PositionReport Position { get; }
    }
}
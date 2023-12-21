// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OsmTileSource.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Controls.Models
{
    using System;

    using Microsoft.Maps.MapControl.WPF;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// OpenStreetMap tile source.
    /// </summary>
    /// <remarks>
    /// sushi.at, 20/12/2023.
    /// </remarks>
    /// <seealso cref="Microsoft.Maps.MapControl.WPF.TileSource"/>
    /// -------------------------------------------------------------------------------------------------
    public class OsmTileSource : TileSource
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Retrieves the URI for the tile specified by the given x, y coordinates and zoom level.
        /// </summary>
        /// <remarks>
        /// sushi.at, 20/12/2023.
        /// </remarks>
        /// <param name="x">
        /// The horizontal position of the tile.
        /// </param>
        /// <param name="y">
        /// The vertical position of the tile.
        /// </param>
        /// <param name="zoomLevel">
        /// The zoom level of the tile.
        /// </param>
        /// <returns>
        /// Returns a <see cref="T:System.Uri"></see> for the tile.
        /// </returns>
        /// <seealso cref="Microsoft.Maps.MapControl.WPF.TileSource.GetUri(int,int,int)"/>
        /// -------------------------------------------------------------------------------------------------
        public override Uri GetUri(int x, int y, int zoomLevel)
        {
            if (string.IsNullOrEmpty(this.UriFormat))
            {
                return null;
            }

            return new Uri(
                this.UriFormat.Replace("{x}", x.ToString()).Replace("{y}", y.ToString()).Replace("{z}", zoomLevel.ToString()));
        }
    }
}
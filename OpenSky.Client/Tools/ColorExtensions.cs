// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ColorExtensions.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Tools
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Color conversion extension methods.
    /// </summary>
    /// <remarks>
    /// sushi.at, 17/11/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public static class ColorExtensions
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Convert media to drawing color (wpf to winforms)
        /// </summary>
        /// <remarks>
        /// sushi.at, 17/11/2021.
        /// </remarks>
        /// <param name="mediaColor">
        /// The media color.
        /// </param>
        /// <returns>
        /// MediaColor as a System.Drawing.Color.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public static System.Drawing.Color ToDrawingColor(this System.Windows.Media.Color mediaColor)
        {
            return System.Drawing.Color.FromArgb(mediaColor.A, mediaColor.R, mediaColor.G, mediaColor.B);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Convert drawing to media color (winforms to wpf)
        /// </summary>
        /// <remarks>
        /// sushi.at, 17/11/2021.
        /// </remarks>
        /// <param name="drawingColor">
        /// The drawing color.
        /// </param>
        /// <returns>
        /// DrawingColor as a System.Windows.Media.Color.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public static System.Windows.Media.Color ToMediaColor(this System.Drawing.Color drawingColor)
        {
            return System.Windows.Media.Color.FromArgb(drawingColor.A, drawingColor.R, drawingColor.G, drawingColor.B);
        }
    }
}

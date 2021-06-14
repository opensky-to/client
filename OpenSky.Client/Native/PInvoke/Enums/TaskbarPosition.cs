// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TaskbarPosition.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Native.PInvoke.Enums
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// TaskbarPosition enumeration.
    /// </summary>
    /// <remarks>
    /// sushi.at, 27/03/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public enum TaskbarPosition
    {
        /// <summary>
        /// Position is unknown
        /// </summary>
        Unknown = -1,

        /// <summary>
        /// Taskbar is on the left side of the screen
        /// </summary>
        Left = 0,

        /// <summary>
        /// Taskbar is at the top of the screen
        /// </summary>
        Top = 1,

        /// <summary>
        /// Taskbar is on the right side of the screen
        /// </summary>
        Right = 2,

        /// <summary>
        /// Taskbar is on the bottom of the screen
        /// </summary>
        Bottom = 3
    }
}
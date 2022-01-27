// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Mouse.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Native
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;

    using OpenSky.Client.Native.PInvoke;
    using OpenSky.Client.Native.PInvoke.Structs;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Global mouse functions.
    /// </summary>
    /// <remarks>
    /// sushi.at, 27/03/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public static class Mouse
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets screen mouse position.
        /// </summary>
        /// <remarks>
        /// sushi.at, 12/03/2021.
        /// </remarks>
        /// <exception cref="Exception">
        /// Thrown when an exception error condition occurs.
        /// </exception>
        /// <returns>
        /// The screen mouse position.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [SuppressMessage("Microsoft.Interoperability", "CA1404:CallGetLastErrorImmediatelyAfterPInvoke", Justification = "Reviewed, ok.")]
        public static System.Windows.Point GetScreenMousePosition()
        {
            var point = default(Point);
            if (User32.GetCursorPos(ref point))
            {
                return new System.Windows.Point(point.x, point.y);
            }

            throw new Exception("Error retrieving screen mouse position: " + Marshal.GetLastWin32Error());
        }
    }
}
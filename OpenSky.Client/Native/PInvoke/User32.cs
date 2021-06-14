// --------------------------------------------------------------------------------------------------------------------
// <copyright file="User32.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Native.PInvoke
{
    using System;
    using System.Runtime.InteropServices;

    using JetBrains.Annotations;

    using OpenSky.Client.Native.PInvoke.Structs;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// PInvoke interface class for User32.dll.
    /// </summary>
    /// <remarks>
    /// sushi.at, 12/03/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public static class User32
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Retrieves the position of the mouse cursor, in screen coordinates.
        /// </summary>
        /// <remarks>
        /// sushi.at, 12/03/2021.
        /// </remarks>
        /// <param name="point">
        /// [in,out] A pointer to a POINT structure that receives the screen coordinates of the cursor.
        /// </param>
        /// <returns>
        /// If the function succeeds, the return value is nonzero. If the function fails, the return
        /// value is zero. To get extended error information, call GetLastError.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetCursorPos(ref Point point);

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Find window.
        /// </summary>
        /// <remarks>
        /// sushi.at, 27/03/2021.
        /// </remarks>
        /// <param name="lpClassName">
        /// The class name or a class atom created by a previous call to the RegisterClass or
        /// RegisterClassEx function. The atom must be in the low-order word of lpClassName; the high-
        /// order word must be zero.
        /// </param>
        /// <param name="lpWindowName">
        /// The window name (the window's title). If this parameter is NULL, all window names match.
        /// </param>
        /// <returns>
        /// If the function succeeds, the return value is a handle to the window that has the specified
        /// class name and window name. If the function fails, the return value is NULL. To get extended
        /// error information, call GetLastError.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindow([CanBeNull] string lpClassName, [CanBeNull] string lpWindowName);
    }
}
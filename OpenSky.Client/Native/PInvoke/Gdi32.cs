// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Gdi32.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Native.PInvoke
{
    using System;
    using System.Runtime.InteropServices;

    using JetBrains.Annotations;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// PInvoke interface class for Gdi32.dll.
    /// </summary>
    /// <remarks>
    /// sushi.at, 27/03/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public static class Gdi32
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Number of pixels per logical inch along the screen width. In a system with multiple display
        /// monitors, this value is the same for all monitors.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public const int LogPixelsX = 88;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// This function creates a device context (DC) for a device.
        /// </summary>
        /// <remarks>
        /// sushi.at, 27/03/2021.
        /// </remarks>
        /// <param name="lpszDriver">
        /// Long pointer to a null-terminated string that specifies the file name of a driver. If this
        /// parameter is set to NULL, the system returns a screen DC.
        /// </param>
        /// <param name="lpszDeviceName">
        /// Long pointer to a null-terminated string that specifies the name of the specific output
        /// device being used, as shown by the Print Manager. It is not the printer model name. The
        /// lpszDevice parameter must be used.
        /// </param>
        /// <param name="lpszOutput">
        /// Long pointer to an output destination.
        /// </param>
        /// <param name="lpInitData">
        /// Long pointer to a DEVMODE structure containing device-specific initialization data for the
        /// device driver. The lpInitData parameter must be NULL if the device driver is to use the
        /// default initialization (if any) specified by the user.
        /// </param>
        /// <returns>
        /// The handle to a device context for the specified device indicates success. NULL indicates
        /// failure. To get extended error information, call GetLastError.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [DllImport("gdi32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr CreateDC(
            [CanBeNull] string lpszDriver,
            [CanBeNull] string lpszDeviceName,
            [CanBeNull] string lpszOutput,
            IntPtr lpInitData);

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// This function retrieves information about the capabilities of a specified device.
        /// </summary>
        /// <remarks>
        /// sushi.at, 27/03/2021.
        /// </remarks>
        /// <param name="hDC">
        /// Handle to the device context.
        /// </param>
        /// <param name="nIndex">
        /// Specifies the item to return. (see http://msdn.microsoft.com/en-us/library/ms929230.aspx for
        /// details).
        /// </param>
        /// <returns>
        /// Returns the value of the desired item.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [DllImport("gdi32.dll")]
        public static extern int GetDeviceCaps(IntPtr hDC, int nIndex);

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The DeleteObject function deletes a logical pen, brush, font, bitmap, region, or palette,
        /// freeing all system resources associated with the object. After the object is deleted, the
        /// specified handle is no longer valid.
        /// </summary>
        /// <remarks>
        /// sushi.at, 27/03/2021.
        /// </remarks>
        /// <param name="hDc">
        /// A handle to a logical pen, brush, font, bitmap, region, or palette.
        /// </param>
        /// <returns>
        /// If the function succeeds, the return value is nonzero. If the specified handle is not valid
        /// or is currently selected into a DC, the return value is zero.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [DllImport("gdi32.dll")]
        public static extern IntPtr DeleteObject(IntPtr hDc);
    }
}
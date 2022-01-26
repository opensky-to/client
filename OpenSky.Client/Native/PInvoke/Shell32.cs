// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Shell32.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Native.PInvoke
{
    using System;
    using System.Runtime.InteropServices;

    using OpenSky.Client.Native.PInvoke.Enums;
    using OpenSky.Client.Native.PInvoke.Structs;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// PInvoke interface class for Shell32.dll.
    /// </summary>
    /// <remarks>
    /// sushi.at, 27/03/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public static class Shell32
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Sends a message to the app bar.
        /// </summary>
        /// <remarks>
        /// sushi.at, 27/03/2021.
        /// </remarks>
        /// <param name="dwMessage">
        /// Appbar message value to send. This parameter can be one of the <see cref="ABM"/> values.
        /// </param>
        /// <param name="pData">
        /// [in,out] The address of an <see cref="AppBarData"/> structure. The content of the structure
        /// on entry and on exit depends on the value set in the dwMessage parameter.
        /// </param>
        /// <returns>
        /// This function returns a message-dependent value. For more information, see the Windows SDK
        /// documentation for the specific appbar message sent.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [DllImport("shell32.dll", SetLastError = true)]
        public static extern IntPtr SHAppBarMessage(ABM dwMessage, [In] ref AppBarData pData);
    }
}
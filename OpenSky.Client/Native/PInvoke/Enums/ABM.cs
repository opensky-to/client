// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ABM.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Native.PInvoke.Enums
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// ABM enumeration.
    /// </summary>
    /// <remarks>
    /// sushi.at, 27/03/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public enum ABM : uint
    {
        /// <summary>
        /// New message
        /// </summary>
        New = 0x00000000,

        /// <summary>
        /// Remove message
        /// </summary>
        Remove = 0x00000001,

        /// <summary>
        /// Query position message
        /// </summary>
        QueryPos = 0x00000002,

        /// <summary>
        /// Set position message
        /// </summary>
        SetPos = 0x00000003,

        /// <summary>
        /// Get state message
        /// </summary>
        GetState = 0x00000004,

        /// <summary>
        /// Get taskbar position message
        /// </summary>
        GetTaskbarPos = 0x00000005,

        /// <summary>
        /// Activate message
        /// </summary>
        Activate = 0x00000006,

        /// <summary>
        /// Get auto hide bar message
        /// </summary>
        GetAutoHideBar = 0x00000007,

        /// <summary>
        /// Set auto hide bar message
        /// </summary>
        SetAutoHideBar = 0x00000008,

        /// <summary>
        /// Window position changed message
        /// </summary>
        WindowPosChanged = 0x00000009,

        /// <summary>
        /// Set state message
        /// </summary>
        SetState = 0x0000000A
    }
}
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WindowExtensions.cs" company="OpenSky">
// sushi.at for OpenSky 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Tools
{
    using System.Windows;

    using OpenSky.Client.Native;
    using OpenSky.Client.Native.PInvoke.Enums;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Window extension methods.
    /// </summary>
    /// <remarks>
    /// sushi.at, 01/06/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public static class WindowExtensions
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Positions the window next to notification area of the taskbar.
        /// </summary>
        /// <remarks>
        /// sushi.at, 01/06/2021.
        /// </remarks>
        /// <param name="window">
        /// The window to act on.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public static void PositionWindowToNotificationArea(this Window window)
        {
            var taskbarInfo = Taskbar.TaskbarInfo;

            if (taskbarInfo.Position == TaskbarPosition.Top)
            {
                window.Left = taskbarInfo.Bounds.X + taskbarInfo.Bounds.Width - window.Width;
                window.Top = taskbarInfo.Bounds.Height;
            }

            if (taskbarInfo.Position == TaskbarPosition.Bottom)
            {
                window.Left = taskbarInfo.Bounds.X + taskbarInfo.Bounds.Width - window.Width;
                window.Top = taskbarInfo.Bounds.Y - window.Height;
            }

            if (taskbarInfo.Position == TaskbarPosition.Left)
            {
                window.Left = taskbarInfo.Bounds.X + taskbarInfo.Bounds.Width;
                window.Top = taskbarInfo.Bounds.Y + taskbarInfo.Bounds.Height - window.Height;
            }

            if (taskbarInfo.Position == TaskbarPosition.Right)
            {
                window.Left = taskbarInfo.Bounds.X - window.Width;
                window.Top = taskbarInfo.Bounds.Y + taskbarInfo.Bounds.Height - window.Height;
            }
        }
    }
}
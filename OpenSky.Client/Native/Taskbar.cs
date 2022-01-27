// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Taskbar.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Native
{
    using System;
    using System.Drawing;
    using System.Runtime.InteropServices;

    using JetBrains.Annotations;

    using OpenSky.Client.Native.PInvoke;
    using OpenSky.Client.Native.PInvoke.Enums;
    using OpenSky.Client.Native.PInvoke.Structs;

    using Point = System.Drawing.Point;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Retrieves information about the Windows taskbar.
    /// </summary>
    /// <remarks>
    /// sushi.at, 27/03/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public sealed class Taskbar
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The windows/class name of the Windows taskbar.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private const string ClassName = "Shell_TrayWnd";

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Prevents a default instance of the <see cref="Taskbar"/> class from being created. Private
        /// constructor.
        /// </summary>
        /// <remarks>
        /// sushi.at, 27/03/2021.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the requested operation is invalid.
        /// </exception>
        /// -------------------------------------------------------------------------------------------------
        private Taskbar()
        {
            IntPtr taskbarHandle = User32.FindWindow(ClassName, null);
            var data = new AppBarData { cbSize = (uint)Marshal.SizeOf(typeof(AppBarData)), hWnd = taskbarHandle };

            IntPtr result = Shell32.SHAppBarMessage(ABM.GetTaskbarPos, ref data);
            if (result == IntPtr.Zero)
            {
                throw new InvalidOperationException();
            }

            this.Position = (TaskbarPosition)data.uEdge;
            this.Bounds = Rectangle.FromLTRB(data.rc.left, data.rc.top, data.rc.right, data.rc.bottom);

            // Perform DPI scaling corrections?
            var displayDC = Gdi32.CreateDC("DISPLAY", null, null, IntPtr.Zero);
            var dpi = Gdi32.GetDeviceCaps(displayDC, Gdi32.LogPixelsX);
            Gdi32.DeleteObject(displayDC);
            if (dpi > 96)
            {
                var scaleFactor = 96f / dpi;
                this.Bounds = Rectangle.FromLTRB(
                    (int)(this.Bounds.Left * scaleFactor),
                    (int)(this.Bounds.Top * scaleFactor),
                    (int)(this.Bounds.Right * scaleFactor),
                    (int)(this.Bounds.Bottom * scaleFactor));
            }

            data.cbSize = (uint)Marshal.SizeOf(typeof(AppBarData));
            result = Shell32.SHAppBarMessage(ABM.GetState, ref data);

            int state = result.ToInt32();
            this.AlwaysOnTop = (state & (int)ABS.AlwaysOnTop) == (int)ABS.AlwaysOnTop;
            this.AutoHide = (state & (int)ABS.AutoHide) == (int)ABS.AutoHide;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the current taskbar information.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [NotNull]
        public static Taskbar TaskbarInfo => new();

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets a value indicating whether the taskbar is always on top.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool AlwaysOnTop { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets a value indicating whether the taskbar will automatically hide if not used.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool AutoHide { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the bounds of the taskbar.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Rectangle Bounds { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the screen location of the taskbar.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Point Location => this.Bounds.Location;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the position of the taskbar.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public TaskbarPosition Position { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the size of the taskbar.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Size Size => this.Bounds.Size;
    }
}
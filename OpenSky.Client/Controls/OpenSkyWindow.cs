// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OpenSkyWindow.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Controls
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Forms;
    using System.Windows.Input;
    using System.Windows.Interop;
    using System.Windows.Shapes;

    using Button = System.Windows.Controls.Button;
    using Cursors = System.Windows.Input.Cursors;
    using MouseEventArgs = System.Windows.Input.MouseEventArgs;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Custom OpenSky window class (implements style element event handlers)
    /// </summary>
    /// <remarks>
    /// sushi.at, 23/03/2021.
    /// </remarks>
    /// <seealso cref="T:System.Windows.Window"/>
    /// -------------------------------------------------------------------------------------------------
    public class OpenSkyWindow : Window
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The horizontal scroll bar property.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static readonly DependencyProperty HorizontalScrollBarProperty = DependencyProperty.Register("HorizontalScrollBar", typeof(bool), typeof(OpenSkyWindow), new UIPropertyMetadata(true));

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The loading text property.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static readonly DependencyProperty LoadingTextProperty = DependencyProperty.Register("LoadingText", typeof(string), typeof(OpenSkyWindow), new UIPropertyMetadata(string.Empty, LoadingTextPropertyChangedCallback));

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The show loading property.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static readonly DependencyProperty ShowLoadingProperty = DependencyProperty.Register("ShowLoading", typeof(bool), typeof(OpenSkyWindow), new UIPropertyMetadata(false));

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The show minimize/maximize buttons property.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static readonly DependencyProperty ShowMinMaxButtonsProperty = DependencyProperty.Register("ShowMinMaxButtons", typeof(bool), typeof(OpenSkyWindow), new UIPropertyMetadata(true));

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The vertical scroll bar property.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static readonly DependencyProperty VerticalScrollBarProperty = DependencyProperty.Register("VerticalScrollBar", typeof(bool), typeof(OpenSkyWindow), new UIPropertyMetadata(true));

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The native window handle source.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private HwndSource hwndSource;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="OpenSkyWindow"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 23/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public OpenSkyWindow()
        {
            if (!App.IsDesignMode)
            {
                // Set our custom window style
                this.Style = this.FindResource("OpenSkyWindowStyle") as Style;
                this.PreviewMouseMove += this.OnPreviewMouseMove;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Values that represent resize directions.
        /// </summary>
        /// <remarks>
        /// sushi.at, 23/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private enum ResizeDirection
        {
            Left = 1,

            Right = 2,

            Top = 3,

            TopLeft = 4,

            TopRight = 5,

            Bottom = 6,

            BottomLeft = 7,

            BottomRight = 8,
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether the horizontal scroll bar is enabled.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Bindable(true)]
        public bool HorizontalScrollBar
        {
            get => (bool)this.GetValue(HorizontalScrollBarProperty);
            set => this.SetValue(HorizontalScrollBarProperty, value);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the loading text (setting a non-empty string will trigger the loading overlay to
        /// be displayed).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Bindable(true)]
        public string LoadingText
        {
            get => (string)this.GetValue(LoadingTextProperty);
            set => this.SetValue(LoadingTextProperty, value);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether to show the loading overlay.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Bindable(true)]
        public bool ShowLoading
        {
            get => (bool)this.GetValue(ShowLoadingProperty);
            set => this.SetValue(ShowLoadingProperty, value);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether the minimize/maximize buttons are shown.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Bindable(true)]
        public bool ShowMinMaxButtons
        {
            get => (bool)this.GetValue(ShowMinMaxButtonsProperty);
            set => this.SetValue(ShowMinMaxButtonsProperty, value);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether the vertical scroll bar is enabled.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Bindable(true)]
        public bool VerticalScrollBar
        {
            get => (bool)this.GetValue(VerticalScrollBarProperty);
            set => this.SetValue(VerticalScrollBarProperty, value);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal
        /// processes call <see cref="M:System.Windows.FrameworkElement.ApplyTemplate" />.
        /// </summary>
        /// <remarks>
        /// sushi.at, 23/03/2021.
        /// </remarks>
        /// <seealso cref="M:System.Windows.FrameworkElement.OnApplyTemplate()"/>
        /// -------------------------------------------------------------------------------------------------
        public override void OnApplyTemplate()
        {
            if (!App.IsDesignMode)
            {
                if (this.GetTemplateChild("MinimizeButton") is Button minimizeButton)
                {
                    minimizeButton.Click += this.MinimizeClick;
                }

                if (this.GetTemplateChild("MaximizeButton") is Button maximizebutton)
                {
                    maximizebutton.Click += this.MaximizeClick;
                }

                if (this.GetTemplateChild("CloseButton") is Button closeButton)
                {
                    closeButton.Click += this.CloseClick;
                }

                if (this.GetTemplateChild("MoveWindow") is Rectangle moveWindow)
                {
                    moveWindow.MouseLeftButtonDown += this.MoveWindowMouseLeftButtonDown;
                }

                if (this.GetTemplateChild("resizeGrid") is Grid resizeGrid)
                {
                    foreach (UIElement element in resizeGrid.Children)
                    {
                        if (element is Rectangle resizeRectangle)
                        {
                            resizeRectangle.PreviewMouseDown += this.ResizeRectanglePreviewMouseDown;
                            resizeRectangle.MouseMove += this.ResizeRectangleMouseMove;
                        }
                    }
                }

                if (this.GetTemplateChild("SantaHat") is Image santaHat)
                {
                    santaHat.Visibility = DateTime.UtcNow.Month == 12 ? Visibility.Visible : Visibility.Collapsed;
                }
            }

            base.OnApplyTemplate();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Close button clicked.
        /// </summary>
        /// <remarks>
        /// sushi.at, 23/03/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Routed event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        protected void CloseClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Maximize button clicked.
        /// </summary>
        /// <remarks>
        /// sushi.at, 23/03/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Routed event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        protected void MaximizeClick(object sender, RoutedEventArgs e)
        {
            this.WindowState = this.WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Minimize button clicked.
        /// </summary>
        /// <remarks>
        /// sushi.at, 23/03/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Routed event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        protected void MinimizeClick(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Raises the <see cref="E:System.Windows.FrameworkElement.Initialized" /> event. This method is
        /// invoked whenever <see cref="P:System.Windows.FrameworkElement.IsInitialized" /> is set to
        /// <see langword="true " />internally.
        /// </summary>
        /// <remarks>
        /// sushi.at, 23/03/2021.
        /// </remarks>
        /// <param name="e">
        /// The <see cref="T:System.Windows.RoutedEventArgs" /> that contains the event data.
        /// </param>
        /// <seealso cref="M:System.Windows.FrameworkElement.OnInitialized(EventArgs)"/>
        /// -------------------------------------------------------------------------------------------------
        protected override void OnInitialized(EventArgs e)
        {
            this.SourceInitialized += this.OnSourceInitialized;
            base.OnInitialized(e);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Callback, called when the loading text property changed.
        /// </summary>
        /// <remarks>
        /// sushi.at, 04/06/2021.
        /// </remarks>
        /// <param name="d">
        /// A DependencyObject to process.
        /// </param>
        /// <param name="e">
        /// Dependency property changed event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private static void LoadingTextPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is string text)
            {
                d.SetValue(ShowLoadingProperty, !string.IsNullOrEmpty(text));
            }

            if (e.NewValue == null)
            {
                d.SetValue(ShowLoadingProperty, false);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Sends a user32 window message.
        /// </summary>
        /// <remarks>
        /// sushi.at, 23/03/2021.
        /// </remarks>
        /// <param name="hWnd">
        /// The window.
        /// </param>
        /// <param name="msg">
        /// The message.
        /// </param>
        /// <param name="wParam">
        /// The parameter.
        /// </param>
        /// <param name="lParam">
        /// The parameter.
        /// </param>
        /// <returns>
        /// An IntPtr.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// An application-defined function that processes messages sent to a window. The WNDPROC type defines a pointer to this callback function.
        /// </summary>
        /// <remarks>
        /// sushi.at, 24/03/2021.
        /// </remarks>
        /// <param name="hwnd">
        /// A handle to the window.
        /// </param>
        /// <param name="msg">
        /// The message.
        /// </param>
        /// <param name="wParam">
        /// Additional message information. The contents of this parameter depend on the value of the uMsg parameter.
        /// </param>
        /// <param name="lParam">
        /// Additional message information. The contents of this parameter depend on the value of the uMsg parameter.
        /// </param>
        /// <param name="handled">
        /// [in,out] True if handled.
        /// </param>
        /// <returns>
        /// An IntPtr set to zero.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        private static IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                // WM_GETMINMAXINFO
                case 0x0024:
                    WmGetMinMaxInfo(hwnd, lParam);
                    handled = true;
                    break;
            }

            return (IntPtr)0;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Sent to a window when the size or position of the window is about to change. An application
        /// can use this message to override the window's default maximized size and position, or its
        /// default minimum or maximum tracking size.
        /// </summary>
        /// <remarks>
        /// sushi.at, 24/03/2021.
        /// </remarks>
        /// <param name="hwnd">
        /// A handle to the window.
        /// </param>
        /// <param name="lParam">
        /// A pointer to a MINMAXINFO structure that contains the default maximized position and
        /// dimensions, and the default minimum and maximum tracking sizes. An application can override
        /// the defaults by setting the members of this structure.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private static void WmGetMinMaxInfo(IntPtr hwnd, IntPtr lParam)
        {
            var mmi = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));

            // Adjust the maximized size and position to fit the work area of the correct monitor
            var currentScreen = Screen.FromHandle(hwnd);
            var workArea = currentScreen.WorkingArea;
            var monitorArea = currentScreen.Bounds;
            mmi.ptMaxPosition.x = Math.Abs(workArea.Left - monitorArea.Left);
            mmi.ptMaxPosition.y = Math.Abs(workArea.Top - monitorArea.Top);
            mmi.ptMaxSize.x = Math.Abs(workArea.Right - workArea.Left);
            mmi.ptMaxSize.y = Math.Abs(workArea.Bottom - workArea.Top);
            Marshal.StructureToPtr(mmi, lParam, true);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Move window mouse left button down.
        /// </summary>
        /// <remarks>
        /// sushi.at, 23/03/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Mouse button event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void MoveWindowMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                this.MaximizeClick(sender, e);
            }
            else
            {
                if (this.WindowState == WindowState.Maximized)
                {
                    var mousePosition = this.PointToScreen(Mouse.GetPosition(this));
                    this.WindowState = WindowState.Normal;
                    this.Left = mousePosition.X - (this.Width / 2);
                    this.Top = mousePosition.Y - 30; // Header is 60px tall
                }

                this.DragMove();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// On preview mouse move.
        /// </summary>
        /// <remarks>
        /// sushi.at, 23/03/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Event information to send to registered event handlers.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void OnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (Mouse.LeftButton != MouseButtonState.Pressed)
            {
                this.Cursor = Cursors.Arrow;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The source initialized.
        /// </summary>
        /// <remarks>
        /// sushi.at, 23/03/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Event information to send to registered event handlers.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void OnSourceInitialized(object sender, EventArgs e)
        {
            this.hwndSource = (HwndSource)PresentationSource.FromVisual(this);
            this.hwndSource?.AddHook(WindowProc);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Resize rectangle mouse moved.
        /// </summary>
        /// <remarks>
        /// sushi.at, 23/03/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Mouse event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void ResizeRectangleMouseMove(object sender, MouseEventArgs e)
        {
            if (sender is Rectangle rectangle && this.ResizeMode == ResizeMode.CanResize)
            {
                this.Cursor = rectangle.Name switch
                {
                    "top" => Cursors.SizeNS,
                    "bottom" => Cursors.SizeNS,
                    "left" => Cursors.SizeWE,
                    "right" => Cursors.SizeWE,
                    "topLeft" => Cursors.SizeNWSE,
                    "topRight" => Cursors.SizeNESW,
                    "bottomLeft" => Cursors.SizeNESW,
                    "bottomRight" => Cursors.SizeNWSE,
                    _ => this.Cursor
                };
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Resize rectangle preview mouse down.
        /// </summary>
        /// <remarks>
        /// sushi.at, 23/03/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Mouse button event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void ResizeRectanglePreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Rectangle rectangle && this.ResizeMode == ResizeMode.CanResize)
            {
                switch (rectangle.Name)
                {
                    case "top":
                        this.Cursor = Cursors.SizeNS;
                        this.ResizeWindow(ResizeDirection.Top);
                        break;
                    case "bottom":
                        this.Cursor = Cursors.SizeNS;
                        this.ResizeWindow(ResizeDirection.Bottom);
                        break;
                    case "left":
                        this.Cursor = Cursors.SizeWE;
                        this.ResizeWindow(ResizeDirection.Left);
                        break;
                    case "right":
                        this.Cursor = Cursors.SizeWE;
                        this.ResizeWindow(ResizeDirection.Right);
                        break;
                    case "topLeft":
                        this.Cursor = Cursors.SizeNWSE;
                        this.ResizeWindow(ResizeDirection.TopLeft);
                        break;
                    case "topRight":
                        this.Cursor = Cursors.SizeNESW;
                        this.ResizeWindow(ResizeDirection.TopRight);
                        break;
                    case "bottomLeft":
                        this.Cursor = Cursors.SizeNESW;
                        this.ResizeWindow(ResizeDirection.BottomLeft);
                        break;
                    case "bottomRight":
                        this.Cursor = Cursors.SizeNWSE;
                        this.ResizeWindow(ResizeDirection.BottomRight);
                        break;
                }
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Resize the window.
        /// </summary>
        /// <remarks>
        /// sushi.at, 23/03/2021.
        /// </remarks>
        /// <param name="direction">
        /// The direction.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void ResizeWindow(ResizeDirection direction)
        {
            SendMessage(this.hwndSource.Handle, 0x112, (IntPtr)(61440 + direction), IntPtr.Zero);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Contains information about a window's maximized size and position and its minimum and maximum tracking size.
        /// </summary>
        /// <remarks>
        /// sushi.at, 24/03/2021.
        /// </remarks>
        /// <seealso cref="T:System.Windows.Window"/>
        /// -------------------------------------------------------------------------------------------------
        [StructLayout(LayoutKind.Sequential)]

        // ReSharper disable once InconsistentNaming
        public struct MINMAXINFO
        {
            /// -------------------------------------------------------------------------------------------------
            /// <summary>
            /// Reserved; do not use.
            /// </summary>
            /// -------------------------------------------------------------------------------------------------
            public POINT ptReserved;

            /// -------------------------------------------------------------------------------------------------
            /// <summary>
            /// The maximized width (x member) and the maximized height (y member) of the window. For top-
            /// level windows, this value is based on the width of the primary monitor.
            /// </summary>
            /// -------------------------------------------------------------------------------------------------
            public POINT ptMaxSize;

            /// -------------------------------------------------------------------------------------------------
            /// <summary>
            /// The position of the left side of the maximized window (x member) and the position of the top
            /// of the maximized window (y member). For top-level windows, this value is based on the
            /// position of the primary monitor.
            /// </summary>
            /// -------------------------------------------------------------------------------------------------
            public POINT ptMaxPosition;

            /// -------------------------------------------------------------------------------------------------
            /// <summary>
            /// The minimum tracking width (x member) and the minimum tracking height (y member) of the
            /// window. This value can be obtained programmatically from the system metrics SM_CXMINTRACK and
            /// SM_CYMINTRACK (see the GetSystemMetrics function).
            /// </summary>
            /// -------------------------------------------------------------------------------------------------
            public POINT ptMinTrackSize;

            /// -------------------------------------------------------------------------------------------------
            /// <summary>
            /// The maximum tracking width (x member) and the maximum tracking height (y member) of the
            /// window. This value is based on the size of the virtual screen and can be obtained
            /// programmatically from the system metrics SM_CXMAXTRACK and SM_CYMAXTRACK (see the
            /// GetSystemMetrics function).
            /// </summary>
            /// -------------------------------------------------------------------------------------------------
            public POINT ptMaxTrackSize;
        };

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The POINT structure defines the x- and y-coordinates of a point.
        /// </summary>
        /// <remarks>
        /// sushi.at, 24/03/2021.
        /// </remarks>
        /// <seealso cref="T:System.Windows.Window"/>
        /// -------------------------------------------------------------------------------------------------
        [StructLayout(LayoutKind.Sequential)]

        // ReSharper disable once InconsistentNaming
        public struct POINT
        {
            /// -------------------------------------------------------------------------------------------------
            /// <summary>
            /// Specifies the x-coordinate of the point.
            /// </summary>
            /// -------------------------------------------------------------------------------------------------
            public int x;

            /// -------------------------------------------------------------------------------------------------
            /// <summary>
            /// Specifies the y-coordinate of the point.
            /// </summary>
            /// -------------------------------------------------------------------------------------------------
            public int y;

            /// -------------------------------------------------------------------------------------------------
            /// <summary>
            /// Construct a point of coordinates (x,y).
            /// </summary>
            /// <remarks>
            /// sushi.at, 24/03/2021.
            /// </remarks>
            /// <param name="x">
            /// x coordinate of point.
            /// </param>
            /// <param name="y">
            /// y coordinate of point.
            /// </param>
            /// -------------------------------------------------------------------------------------------------
            public POINT(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
        }
    }
}
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
            // Set our custom window style
            this.Style = this.FindResource("OpenSkyWindowStyle") as Style;
            this.PreviewMouseMove += this.OnPreviewMouseMove;
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
        private static extern IntPtr SendMessage(IntPtr hWnd, UInt32 msg, IntPtr wParam, IntPtr lParam);

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
                switch (rectangle.Name)
                {
                    case "top":
                        this.Cursor = Cursors.SizeNS;
                        break;
                    case "bottom":
                        this.Cursor = Cursors.SizeNS;
                        break;
                    case "left":
                        this.Cursor = Cursors.SizeWE;
                        break;
                    case "right":
                        this.Cursor = Cursors.SizeWE;
                        break;
                    case "topLeft":
                        this.Cursor = Cursors.SizeNWSE;
                        break;
                    case "topRight":
                        this.Cursor = Cursors.SizeNESW;
                        break;
                    case "bottomLeft":
                        this.Cursor = Cursors.SizeNESW;
                        break;
                    case "bottomRight":
                        this.Cursor = Cursors.SizeNWSE;
                        break;
                }
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
    }
}
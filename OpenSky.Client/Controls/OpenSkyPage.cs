// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OpenSkyPage.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Controls
{
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// OpenSky page user control.
    /// </summary>
    /// <remarks>
    /// sushi.at, 01/07/2021.
    /// </remarks>
    /// <seealso cref="T:System.Windows.Controls.UserControl"/>
    /// -------------------------------------------------------------------------------------------------
    public abstract class OpenSkyPage : UserControl
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The horizontal scroll bar property.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static readonly DependencyProperty HorizontalScrollBarProperty = DependencyProperty.Register("HorizontalScrollBar", typeof(bool), typeof(OpenSkyPage), new UIPropertyMetadata(true));

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The loading text property.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static readonly DependencyProperty LoadingTextProperty = DependencyProperty.Register("LoadingText", typeof(string), typeof(OpenSkyPage), new UIPropertyMetadata(string.Empty, LoadingTextPropertyChangedCallback));

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The show loading property.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static readonly DependencyProperty ShowLoadingProperty = DependencyProperty.Register("ShowLoading", typeof(bool), typeof(OpenSkyPage), new UIPropertyMetadata(false));

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The vertical scroll bar property.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static readonly DependencyProperty VerticalScrollBarProperty = DependencyProperty.Register("VerticalScrollBar", typeof(bool), typeof(OpenSkyPage), new UIPropertyMetadata(true));

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="OpenSkyPage"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 01/07/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        protected OpenSkyPage()
        {
            if (!App.IsDesignMode)
            {
                this.Style = this.FindResource("OpenSkyPageStyle") as Style;
            }
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
        /// Method that receives an optional page parameter when the page is opened.
        /// </summary>
        /// <remarks>
        /// sushi.at, 28/10/2021.
        /// </remarks>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public abstract void PassPageParameter(object parameter);

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
    }
}
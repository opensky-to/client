// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DragablzItemsControlEx.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Controls
{
    using System;
    using System.Collections.Specialized;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Data;

    using Dragablz;

    using ModernWpf.Controls.Primitives;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Dragablz item control extension for ModernWpf.
    /// </summary>
    /// <remarks>
    /// sushi.at, 17/06/2021.
    /// </remarks>
    /// <seealso cref="T:Dragablz.DragablzItemsControl"/>
    /// -------------------------------------------------------------------------------------------------
    public class DragablzItemsControlEx : DragablzItemsControl
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The scroll amount.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private const double ScrollAmount = 50.0;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The scroll decrease button.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private RepeatButton scrollDecreaseButton;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The scroll increase button.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private RepeatButton scrollIncreaseButton;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The scroll viewer.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private ScrollViewer scrollViewer;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes static members of the <see cref="DragablzItemsControlEx"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 17/06/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        static DragablzItemsControlEx()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DragablzItemsControlEx), new FrameworkPropertyMetadata(typeof(DragablzItemsControlEx)));
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal
        /// processes call <see cref="M:System.Windows.FrameworkElement.ApplyTemplate" />.
        /// </summary>
        /// <remarks>
        /// sushi.at, 17/06/2021.
        /// </remarks>
        /// <seealso cref="M:System.Windows.FrameworkElement.OnApplyTemplate()"/>
        /// -------------------------------------------------------------------------------------------------
        public override void OnApplyTemplate()
        {
            if (this.scrollViewer != null)
            {
                this.scrollViewer.ScrollChanged -= this.OnScrollViewerScrollChanged;
            }

            if (this.scrollDecreaseButton != null)
            {
                this.scrollDecreaseButton.IsVisibleChanged -= this.OnScrollButtonIsVisibleChanged;
                this.scrollDecreaseButton.Click -= this.OnScrollDecreaseClick;
            }

            if (this.scrollIncreaseButton != null)
            {
                this.scrollIncreaseButton.IsVisibleChanged -= this.OnScrollButtonIsVisibleChanged;
                this.scrollIncreaseButton.Click -= this.OnScrollIncreaseClick;
            }

            base.OnApplyTemplate();

            this.scrollViewer = this.GetTemplateChild("ScrollViewer") as ScrollViewer;
            if (this.scrollViewer != null)
            {
                this.scrollViewer.ApplyTemplate();
                this.scrollViewer.ScrollChanged += this.OnScrollViewerScrollChanged;

                this.scrollDecreaseButton = this.scrollViewer.Template?.FindName("ScrollDecreaseButton", this.scrollViewer) as RepeatButton;
                if (this.scrollDecreaseButton != null)
                {
                    this.scrollDecreaseButton.IsVisibleChanged += this.OnScrollButtonIsVisibleChanged;
                    this.scrollDecreaseButton.Click += this.OnScrollDecreaseClick;
                }

                this.scrollIncreaseButton = this.scrollViewer.Template?.FindName("ScrollIncreaseButton", this.scrollViewer) as RepeatButton;
                if (this.scrollIncreaseButton != null)
                {
                    this.scrollIncreaseButton.IsVisibleChanged += this.OnScrollButtonIsVisibleChanged;
                    this.scrollIncreaseButton.Click += this.OnScrollIncreaseClick;
                }
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Updates the scroll viewer decrease and increase buttons view state.
        /// </summary>
        /// <remarks>
        /// sushi.at, 17/06/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        internal void UpdateScrollViewerDecreaseAndIncreaseButtonsViewState()
        {
            if (this.scrollViewer != null && this.scrollDecreaseButton != null && this.scrollIncreaseButton != null)
            {
                const double minThreshold = 0.1;
                var horizontalOffset = this.scrollViewer.HorizontalOffset;
                var scrollableWidth = this.scrollViewer.ScrollableWidth;

                if (Math.Abs(horizontalOffset - scrollableWidth) < minThreshold)
                {
                    this.scrollDecreaseButton.IsEnabled = true;
                    this.scrollIncreaseButton.IsEnabled = false;
                }
                else if (Math.Abs(horizontalOffset) < minThreshold)
                {
                    this.scrollDecreaseButton.IsEnabled = false;
                    this.scrollIncreaseButton.IsEnabled = true;
                }
                else
                {
                    this.scrollDecreaseButton.IsEnabled = true;
                    this.scrollIncreaseButton.IsEnabled = true;
                }
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// When overridden in a derived class, undoes the effects of the
        /// <see cref="M:System.Windows.Controls.ItemsControl.PrepareContainerForItemOverride(System.Windows.DependencyObject,System.Object)" />
        /// method.
        /// </summary>
        /// <remarks>
        /// sushi.at, 17/06/2021.
        /// </remarks>
        /// <param name="element">
        /// The container element.
        /// </param>
        /// <param name="item">
        /// The item.
        /// </param>
        /// <seealso cref="M:System.Windows.Controls.ItemsControl.ClearContainerForItemOverride(DependencyObject,object)"/>
        /// -------------------------------------------------------------------------------------------------
        protected override void ClearContainerForItemOverride(DependencyObject element, object item)
        {
            base.ClearContainerForItemOverride(element, item);

            if (element is DragablzItem dragablzItem && item is TabItem)
            {
                dragablzItem.ClearValue(DragablzItemHelper.IconProperty);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Called to remeasure a control.
        /// </summary>
        /// <remarks>
        /// sushi.at, 17/06/2021.
        /// </remarks>
        /// <param name="constraint">
        /// The maximum size that the method can return.
        /// </param>
        /// <returns>
        /// The size of the control, up to the maximum specified by <paramref name="constraint" />.
        /// </returns>
        /// <seealso cref="M:System.Windows.Controls.Control.MeasureOverride(Size)"/>
        /// -------------------------------------------------------------------------------------------------
        protected override Size MeasureOverride(Size constraint)
        {
            var desiredSize = base.MeasureOverride(constraint);

            if (this.ItemsOrganiser != null)
            {
                var padding = this.Padding;
                var width = this.ItemsPresenterWidth + padding.Left + padding.Right;
                var height = this.ItemsPresenterHeight + padding.Top + padding.Bottom;

                if (this.scrollDecreaseButton is { IsVisible: true })
                {
                    width += this.scrollDecreaseButton.ActualWidth;
                }

                if (this.scrollIncreaseButton is { IsVisible: true })
                {
                    width += this.scrollIncreaseButton.ActualWidth;
                }

                desiredSize.Width = Math.Min(constraint.Width, width);
                desiredSize.Height = Math.Min(constraint.Height, height);
            }

            return desiredSize;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Invoked when the <see cref="P:System.Windows.Controls.ItemsControl.Items" /> property changes.
        /// </summary>
        /// <remarks>
        /// sushi.at, 17/06/2021.
        /// </remarks>
        /// <param name="e">
        /// Information about the change.
        /// </param>
        /// <seealso cref="M:System.Windows.Controls.ItemsControl.OnItemsChanged(NotifyCollectionChangedEventArgs)"/>
        /// -------------------------------------------------------------------------------------------------
        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);
            this.InvalidateMeasure();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Prepares the specified element to display the specified item.
        /// </summary>
        /// <remarks>
        /// sushi.at, 17/06/2021.
        /// </remarks>
        /// <param name="element">
        /// Element used to display the specified item.
        /// </param>
        /// <param name="item">
        /// Specified item.
        /// </param>
        /// <seealso cref="M:System.Windows.Controls.ItemsControl.PrepareContainerForItemOverride(DependencyObject,object)"/>
        /// -------------------------------------------------------------------------------------------------
        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);

            if (element is DragablzItem dragablzItem && item is TabItem tabItem)
            {
                dragablzItem.SetBinding(
                    DragablzItemHelper.IconProperty,
                    new Binding { Path = new PropertyPath(TabItemHelper.IconProperty), Source = tabItem });
            }
        }

        private void OnScrollButtonIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(bool)e.NewValue)
            {
                this.InvalidateMeasure();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Scroll decrease was clicked.
        /// </summary>
        /// <remarks>
        /// sushi.at, 17/06/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Event information to send to registered event handlers.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void OnScrollDecreaseClick(object sender, RoutedEventArgs e)
        {
            this.scrollViewer?.ScrollToHorizontalOffset(Math.Max(0, this.scrollViewer.HorizontalOffset - ScrollAmount));
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Scroll increase was clicked.
        /// </summary>
        /// <remarks>
        /// sushi.at, 17/06/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Event information to send to registered event handlers.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void OnScrollIncreaseClick(object sender, RoutedEventArgs e)
        {
            this.scrollViewer?.ScrollToHorizontalOffset(Math.Min(this.scrollViewer.ScrollableWidth, this.scrollViewer.HorizontalOffset + ScrollAmount));
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Scroll viewer scroll changed.
        /// </summary>
        /// <remarks>
        /// sushi.at, 17/06/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Event information to send to registered event handlers.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void OnScrollViewerScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            this.UpdateScrollViewerDecreaseAndIncreaseButtonsViewState();
        }
    }
}
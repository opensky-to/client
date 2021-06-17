// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TabablzControlEx.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Controls
{
    using System;
    using System.Collections.Specialized;
    using System.Windows;
    using System.Windows.Controls;

    using Dragablz;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Tabablz control extension for ModernWpf.
    /// </summary>
    /// <remarks>
    /// sushi.at, 17/06/2021.
    /// </remarks>
    /// <seealso cref="T:Dragablz.TabablzControl"/>
    /// -------------------------------------------------------------------------------------------------
    public class TabablzControlEx : TabablzControl
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The add button column.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private ColumnDefinition addButtonColumn;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The items control.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private DragablzItemsControlEx itemsControl;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The items presenter.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private ItemsPresenter itemsPresenter;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The left content column.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private ColumnDefinition leftContentColumn;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///The previously available size.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Size previousAvailableSize;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The right content column.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private ColumnDefinition rightContentColumn;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The right content presenter.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private ContentPresenter rightContentPresenter;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The tab column.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private ColumnDefinition tabColumn;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The tab container grid.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Grid tabContainerGrid;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes static members of the <see cref="TabablzControlEx"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 17/06/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        static TabablzControlEx()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TabablzControlEx), new FrameworkPropertyMetadata(typeof(TabablzControlEx)));
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Called when <see cref="M:System.Windows.FrameworkElement.ApplyTemplate" /> is called.
        /// </summary>
        /// <remarks>
        /// sushi.at, 17/06/2021.
        /// </remarks>
        /// <seealso cref="M:Dragablz.TabablzControl.OnApplyTemplate()"/>
        /// -------------------------------------------------------------------------------------------------
        public override void OnApplyTemplate()
        {
            if (this.itemsPresenter != null)
            {
                this.itemsPresenter.SizeChanged -= this.OnItemsPresenterSizeChanged;
                this.itemsPresenter = null;
            }

            base.OnApplyTemplate();

            this.rightContentPresenter = this.GetTemplateChild("RightContentPresenter") as ContentPresenter;

            this.leftContentColumn = this.GetTemplateChild("LeftContentColumn") as ColumnDefinition;
            this.tabColumn = this.GetTemplateChild("TabColumn") as ColumnDefinition;
            this.addButtonColumn = this.GetTemplateChild("AddButtonColumn") as ColumnDefinition;
            this.rightContentColumn = this.GetTemplateChild("RightContentColumn") as ColumnDefinition;

            this.tabContainerGrid = this.GetTemplateChild("TabContainerGrid") as Grid;

            this.itemsControl = this.GetTemplateChild(HeaderItemsControlPartName) as DragablzItemsControlEx;
            if (this.itemsControl != null)
            {
                this.itemsControl.ApplyTemplate();

                this.itemsPresenter = this.itemsControl.Template?.FindName("TabsItemsPresenter", this.itemsControl) as ItemsPresenter;
                if (this.itemsPresenter != null)
                {
                    this.itemsPresenter.SizeChanged += this.OnItemsPresenterSizeChanged;
                }
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
            if (Math.Abs(this.previousAvailableSize.Width - constraint.Width) > 1.0)
            {
                this.previousAvailableSize = constraint;
                this.UpdateTabWidths();
            }

            return base.MeasureOverride(constraint);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// when the items change we remove any generated panel children and add any new ones as
        /// necessary.
        /// </summary>
        /// <remarks>
        /// sushi.at, 17/06/2021.
        /// </remarks>
        /// <param name="e">
        /// .
        /// </param>
        /// <seealso cref="M:Dragablz.TabablzControl.OnItemsChanged(NotifyCollectionChangedEventArgs)"/>
        /// -------------------------------------------------------------------------------------------------
        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);
            this.UpdateTabWidths();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Items presenter size was changed.
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
        private void OnItemsPresenterSizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.UpdateTabWidths();
            this.itemsControl?.UpdateScrollViewerDecreaseAndIncreaseButtonsViewState();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Update the tab widths.
        /// </summary>
        /// <remarks>
        /// sushi.at, 17/06/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void UpdateTabWidths()
        {
            if (this.tabContainerGrid != null)
            {
                // Add up width taken by custom content and + button
                var widthTaken = 0.0;
                if (this.leftContentColumn != null)
                {
                    widthTaken += this.leftContentColumn.ActualWidth;
                }

                if (this.addButtonColumn != null)
                {
                    widthTaken += this.addButtonColumn.ActualWidth;
                }

                if (this.rightContentColumn != null)
                {
                    if (this.rightContentPresenter != null)
                    {
                        var rightContentSize = this.rightContentPresenter.DesiredSize;
                        this.rightContentPresenter.MinWidth = rightContentSize.Width;
                        widthTaken += rightContentSize.Width;
                    }
                }

                if (this.tabColumn != null)
                {
                    // Note: can be infinite
                    var availableWidth = this.previousAvailableSize.Width - widthTaken;

                    // Size can be 0 when window is first created; in that case, skip calculations; we'll get a new size soon
                    if (availableWidth > 0)
                    {
                        this.tabColumn.MaxWidth = availableWidth;
                        this.tabColumn.Width = GridLength.Auto;
                        if (this.itemsControl != null)
                        {
                            this.itemsControl.MaxWidth = availableWidth;

                            // Calculate if the scroll buttons should be visible.
                            if (this.itemsPresenter != null)
                            {
                                var visible = this.itemsPresenter.ActualWidth > availableWidth;
                                ScrollViewer.SetHorizontalScrollBarVisibility(
                                    this.itemsControl,
                                    visible
                                        ? ScrollBarVisibility.Visible
                                        : ScrollBarVisibility.Hidden);
                                if (visible)
                                {
                                    this.itemsControl.UpdateScrollViewerDecreaseAndIncreaseButtonsViewState();
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
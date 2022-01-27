// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MultiSelectExtension.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Tools
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Multi select extension for DataGrid and ListBox.
    /// </summary>
    /// <remarks>
    /// sushi.at, 08/06/2021.
    /// </remarks>
    /// <seealso cref="T:System.Windows.DependencyObject"/>
    /// -------------------------------------------------------------------------------------------------
    public class MultiSelectExtension : DependencyObject
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The is subscribed to selection changed property.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static readonly DependencyProperty IsSubscribedToSelectionChangedProperty = DependencyProperty.RegisterAttached("IsSubscribedToSelectionChanged", typeof(bool), typeof(MultiSelectExtension), new PropertyMetadata(default(bool)));

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The selected items property.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static readonly DependencyProperty SelectedItemsProperty = DependencyProperty.RegisterAttached("SelectedItems", typeof(IList), typeof(MultiSelectExtension), new PropertyMetadata(default(IList), OnSelectedItemsChanged));

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets is subscribed to selection changed.
        /// </summary>
        /// <remarks>
        /// sushi.at, 08/06/2021.
        /// </remarks>
        /// <param name="element">
        /// The element.
        /// </param>
        /// <returns>
        /// True if subscribed, false otherwise.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public static bool GetIsSubscribedToSelectionChanged(DependencyObject element)
        {
            return (bool)element.GetValue(IsSubscribedToSelectionChangedProperty);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets selected items.
        /// </summary>
        /// <remarks>
        /// sushi.at, 08/06/2021.
        /// </remarks>
        /// <param name="element">
        /// The element.
        /// </param>
        /// <returns>
        /// The selected items.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public static IList GetSelectedItems(DependencyObject element)
        {
            return (IList)element.GetValue(SelectedItemsProperty);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Sets is subscribed to selection changed.
        /// </summary>
        /// <remarks>
        /// sushi.at, 08/06/2021.
        /// </remarks>
        /// <param name="element">
        /// The element.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public static void SetIsSubscribedToSelectionChanged(DependencyObject element, bool value)
        {
            element.SetValue(IsSubscribedToSelectionChangedProperty, value);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Sets selected items.
        /// </summary>
        /// <remarks>
        /// sushi.at, 08/06/2021.
        /// </remarks>
        /// <param name="element">
        /// The element.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public static void SetSelectedItems(DependencyObject element, IList value)
        {
            element.SetValue(SelectedItemsProperty, value);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// When the attached object implements INotifyCollectionChanged, the attached listbox or grid
        /// will have its selectedItems adjusted by this handler.
        /// </summary>
        /// <remarks>
        /// sushi.at, 08/06/2021.
        /// </remarks>
        /// <param name="sender">
        /// The listbox or grid.
        /// </param>
        /// <param name="e">
        /// The added and removed items.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private static void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Push the changes to the selected item.
            if (sender is ListBox listbox)
            {
                listbox.SelectionChanged -= OnSelectorSelectionChanged;
                if (e.Action == NotifyCollectionChangedAction.Reset) listbox.SelectedItems.Clear();
                else
                {
                    foreach (var oldItem in e.OldItems)
                    {
                        listbox.SelectedItems.Remove(oldItem);
                    }

                    foreach (var newItem in e.NewItems)
                    {
                        listbox.SelectedItems.Add(newItem);
                    }
                }

                listbox.SelectionChanged += OnSelectorSelectionChanged;
            }

            if (sender is MultiSelector grid)
            {
                grid.SelectionChanged -= OnSelectorSelectionChanged;
                if (e.Action == NotifyCollectionChangedAction.Reset)
                {
                    grid.SelectedItems.Clear();
                }
                else
                {
                    foreach (var oldItem in e.OldItems)
                    {
                        grid.SelectedItems.Remove(oldItem);
                    }

                    foreach (var newItem in e.NewItems)
                    {
                        grid.SelectedItems.Add(newItem);
                    }
                }

                grid.SelectionChanged += OnSelectorSelectionChanged;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Attaches a list or observable collection to the grid or listbox, syncing both lists (one way
        /// sync for simple lists).
        /// </summary>
        /// <remarks>
        /// sushi.at, 08/06/2021.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// Thrown when one or more arguments have unsupported or illegal values.
        /// </exception>
        /// <param name="d">
        /// The DataGrid or ListBox.
        /// </param>
        /// <param name="e">
        /// The list to sync to.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private static void OnSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not (ListBox or MultiSelector))
            {
                throw new ArgumentException("ListBoxes and Multiselectors (DataGrid), only");
            }

            var selector = (Selector)d;
            if (e.OldValue is IList oldList)
            {
                if (oldList is INotifyCollectionChanged obs)
                {
                    obs.CollectionChanged -= OnCollectionChanged;
                }

                // If we're orphaned, disconnect lb/dg events.
                if (e.NewValue == null)
                {
                    selector.SelectionChanged -= OnSelectorSelectionChanged;
                    SetIsSubscribedToSelectionChanged(selector, false);
                }
            }

            var newList = (IList)e.NewValue;
            if (newList != null)
            {
                if (newList is INotifyCollectionChanged obs)
                {
                    obs.CollectionChanged += OnCollectionChanged;
                }

                PushCollectionDataToSelectedItems(newList, selector);
                var isSubscribed = GetIsSubscribedToSelectionChanged(selector);
                if (!isSubscribed)
                {
                    selector.SelectionChanged += OnSelectorSelectionChanged;
                    SetIsSubscribedToSelectionChanged(selector, true);
                }
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// When the listbox or grid fires a selectionChanged even, we update the attached list to match
        /// it.
        /// </summary>
        /// <remarks>
        /// sushi.at, 08/06/2021.
        /// </remarks>
        /// <param name="sender">
        /// The listbox or grid.
        /// </param>
        /// <param name="e">
        /// Items added and removed.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private static void OnSelectorSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var dep = (DependencyObject)sender;
            var items = GetSelectedItems(dep);
            var col = items as INotifyCollectionChanged;
            var type = typeof(object);
            if (items.GetType().IsGenericType)
            {
                type = items.GetType().GetGenericArguments()[0];
            }

            // Remove the events so we don't fire back and forth, then re-add them.
            if (col != null)
            {
                col.CollectionChanged -= OnCollectionChanged;
            }

            foreach (var oldItem in e.RemovedItems)
            {
                items.Remove(oldItem);
            }

            foreach (var newItem in e.AddedItems)
            {
                if (newItem.GetType() == type)
                {
                    items.Add(newItem);
                }
            }

            if (col != null)
            {
                col.CollectionChanged += OnCollectionChanged;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initially set the selected items to the items in the newly connected collection, unless the
        /// new collection has no selected items and the listbox/grid does, in which case the flow is
        /// reversed. The data holder sets the state. If both sides hold data, then the bound IList wins
        /// and dominates the helpless wpf control.
        /// </summary>
        /// <remarks>
        /// sushi.at, 08/06/2021.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// Thrown when one or more arguments have unsupported or illegal values.
        /// </exception>
        /// <param name="obs">
        /// The list to sync to.
        /// </param>
        /// <param name="selector">
        /// The grid or listbox.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private static void PushCollectionDataToSelectedItems(IList obs, DependencyObject selector)
        {
            if (selector is ListBox listBox)
            {
                if (obs.Count > 0)
                {
                    listBox.SelectedItems.Clear();
                    foreach (var ob in obs)
                    {
                        listBox.SelectedItems.Add(ob);
                    }
                }
                else
                {
                    foreach (var ob in listBox.SelectedItems)
                    {
                        obs.Add(ob);
                    }
                }

                return;
            }

            // Maybe other things will use the multiselector base
            if (selector is MultiSelector grid)
            {
                if (obs.Count > 0)
                {
                    grid.SelectedItems.Clear();
                    foreach (var ob in obs)
                    {
                        grid.SelectedItems.Add(ob);
                    }
                }
                else
                {
                    foreach (var ob in grid.SelectedItems)
                    {
                        obs.Add(ob);
                    }
                }

                return;
            }

            throw new ArgumentException("ListBoxes and Multiselectors (DataGrid), only");
        }
    }
}
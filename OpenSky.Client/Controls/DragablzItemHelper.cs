// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DragablzItemHelper.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Controls
{
    using System.Windows;

    using Dragablz;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Item helper that adds icon support to dragablz tabs.
    /// </summary>
    /// <remarks>
    /// sushi.at, 17/06/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public static class DragablzItemHelper
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The icon property.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.RegisterAttached(
                "Icon",
                typeof(object),
                typeof(DragablzItemHelper));

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the icon.
        /// </summary>
        /// <remarks>
        /// sushi.at, 17/06/2021.
        /// </remarks>
        /// <param name="item">
        /// The item.
        /// </param>
        /// <returns>
        /// The icon.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public static object GetIcon(DragablzItem item)
        {
            return item.GetValue(IconProperty);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Sets the icon.
        /// </summary>
        /// <remarks>
        /// sushi.at, 17/06/2021.
        /// </remarks>
        /// <param name="item">
        /// The item.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public static void SetIcon(DragablzItem item, object value)
        {
            item.SetValue(IconProperty, value);
        }
    }
}
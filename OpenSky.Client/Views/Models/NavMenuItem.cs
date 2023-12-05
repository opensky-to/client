﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NavMenuItem.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Views.Models
{
    using System;
    using System.Collections.ObjectModel;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Navigation menu item.
    /// </summary>
    /// <remarks>
    /// sushi.at, 17/06/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class NavMenuItem
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the children.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<NavMenuItem> Children { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the icon.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string Icon { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether this menu item is expanded.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool IsExpanded { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string Name { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the type of the page.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Type PageType { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the optional parameter to pass to the newly created page.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public object Parameter { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the tooltip.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string Tooltip { get; set; }
    }
}
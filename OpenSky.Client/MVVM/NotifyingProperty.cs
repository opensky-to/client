// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NotifyingProperty.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.MVVM
{
    using System;

    using JetBrains.Annotations;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// The NotifyingProperty class - represents a property of a viewmodel that can be wired into the
    /// notification system.
    /// </summary>
    /// <remarks>
    /// sushi.at, 11/03/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class NotifyingProperty
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyingProperty"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 11/03/2021.
        /// </remarks>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public NotifyingProperty([NotNull] string name, [NotNull] Type type, [CanBeNull] object value)
        {
            this.Name = name;
            this.Type = type;
            this.Value = value;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        /// -------------------------------------------------------------------------------------------------
        [NotNull]
        public string Name { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        /// -------------------------------------------------------------------------------------------------
        [NotNull]
        public Type Type { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        /// -------------------------------------------------------------------------------------------------
        [CanBeNull]
        public object Value { get; set; }
    }
}
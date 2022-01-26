// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StringValueAttribute.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Tools
{
    using System;

    using JetBrains.Annotations;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// This attribute is used to represent a string value for a value in an enum.
    /// </summary>
    /// <remarks>
    /// sushi.at, 19/03/2021.
    /// </remarks>
    /// <seealso cref="T:System.Attribute"/>
    /// -------------------------------------------------------------------------------------------------
    [AttributeUsage(AttributeTargets.Field)]
    public class StringValueAttribute : Attribute
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="StringValueAttribute"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 19/03/2021.
        /// </remarks>
        /// <param name="value">
        /// The string value to store.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public StringValueAttribute([NotNull] string value)
        {
            this.StringValue = value;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets gets the string value.
        /// </summary>
        /// <value>
        /// The string value.
        /// </value>
        /// -------------------------------------------------------------------------------------------------
        [NotNull]
        public string StringValue { get; protected set; }
    }
}
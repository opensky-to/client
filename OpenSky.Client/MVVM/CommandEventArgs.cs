// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CommandEventArgs.cs" company="OpenSky">
// sushi.at for OpenSky 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.MVVM
{
    using System;

    using JetBrains.Annotations;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// CommandEventArgs - simply holds the command parameter.
    /// </summary>
    /// <remarks>
    /// sushi.at, 11/03/2021.
    /// </remarks>
    /// <seealso cref="T:System.EventArgs"/>
    /// -------------------------------------------------------------------------------------------------
    public class CommandEventArgs : EventArgs
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the parameter.
        /// </summary>
        /// <value>
        /// The parameter.
        /// </value>
        /// -------------------------------------------------------------------------------------------------
        [CanBeNull]
        public object Parameter { get; set; }
    }
}
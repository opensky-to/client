// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CancelCommandEventArgs.cs" company="OpenSky">
// sushi.at for OpenSky 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.MVVM
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// CancelCommandEventArgs - just like above but allows the event to be cancelled.
    /// </summary>
    /// <remarks>
    /// sushi.at, 11/03/2021.
    /// </remarks>
    /// <seealso cref="T:OpenSky.Client.MVVM.CommandEventArgs"/>
    /// -------------------------------------------------------------------------------------------------
    public class CancelCommandEventArgs : CommandEventArgs
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="CancelCommandEventArgs"/> command
        /// should be cancelled.
        /// </summary>
        /// <value>
        /// <c>true</c> if cancel; otherwise, <c>false</c>.
        /// </value>
        /// -------------------------------------------------------------------------------------------------
        public bool Cancel { get; set; }
    }
}
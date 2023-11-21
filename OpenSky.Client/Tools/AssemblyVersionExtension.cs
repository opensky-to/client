// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AssemblyVersionExtension.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Tools
{
    using System;
    using System.Reflection;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Assembly version extensions.
    /// </summary>
    /// <remarks>
    /// sushi.at, 24/11/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public static class AssemblyVersionExtension
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// An Assembly extension method that gets version without revision.
        /// </summary>
        /// <remarks>
        /// sushi.at, 24/11/2021.
        /// </remarks>
        /// <param name="assembly">
        /// The assembly to act on.
        /// </param>
        /// <returns>
        /// The version without revision.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public static Version GetVersionWithoutRevision(this Assembly assembly)
        {
            var version = assembly != null ? assembly.GetName().Version : new Version("0.0.0");

            return new Version(version.Major, version.Minor, version.Build);
        }
    }
}

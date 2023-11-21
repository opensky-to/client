// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AircraftRegistryExtensions.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Tools
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Extension methods for removing simulator prefixes from aircraft registries.
    /// </summary>
    /// <remarks>
    /// sushi.at, 09/02/2022.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public static class AircraftRegistryExtensions
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// A string extension method that removes the simulation prefix from an aircraft registry.
        /// </summary>
        /// <remarks>
        /// sushi.at, 09/02/2022.
        /// </remarks>
        /// <param name="registry">
        /// The registry to act on.
        /// </param>
        /// <returns>
        /// The registry without the prefix.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public static string RemoveSimPrefix(this string registry)
        {
            if (registry is { Length: >= 3 } && registry[1] == '.')
            {
                return registry.Substring(2);
            }

            return registry;
        }
    }
}
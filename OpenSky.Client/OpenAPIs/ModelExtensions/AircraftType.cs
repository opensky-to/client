// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AircraftType.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace

namespace OpenSkyApi
{
    using System;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Aircraft type extensions.
    /// </summary>
    /// <remarks>
    /// sushi.at, 21/07/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public partial class AircraftType
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the name with variant star.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string NameWithVariant => $"{this.Name}{(this.HasVariants ? "*" : string.Empty)}";

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the vanilla/addon image.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ImageSource VanillaAddonImage => new BitmapImage(
            new Uri(
                $"pack://application:,,,/OpenSky.Client;component/Resources/{(this.IsVanilla ? "vanilla16.png" : "addon16.png")}"));

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <remarks>
        /// sushi.at, 21/07/2021.
        /// </remarks>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        /// <seealso cref="M:System.Object.ToString()"/>
        /// -------------------------------------------------------------------------------------------------
        public override string ToString()
        {
            return $"{this.Name}";
        }
    }
}
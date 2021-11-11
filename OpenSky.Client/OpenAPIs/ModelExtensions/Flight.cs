// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Flight.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace
namespace OpenSkyApi
{
    using System;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    public partial class Flight
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the flight paused/active image.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ImageSource FlightPausedImage => new BitmapImage(new Uri($"pack://application:,,,/OpenSky.Client;component/Resources/{(this.Paused.HasValue ? "pause16.png" : "departure16.png")}"));
    }
}
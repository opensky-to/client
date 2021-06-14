// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OpenSkyColors.cs" company="OpenSky">
// sushi.at for OpenSky 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client
{
    using System.Windows;
    using System.Windows.Media;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// OpenSky colors from style resource dictionary.
    /// </summary>
    /// <remarks>
    /// sushi.at, 24/03/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public static class OpenSkyColors
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes static members of the <see cref="OpenSkyColors"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 24/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        static OpenSkyColors()
        {
            OpenSkyTeal = Application.Current.Resources["OpenSkyTeal"] as Color? ?? Colors.HotPink;
            OpenSkyTealLight = Application.Current.Resources["OpenSkyTealLight"] as Color? ?? Colors.HotPink;
            OpenSkyRed = Application.Current.Resources["OpenSkyRed"] as Color? ?? Colors.HotPink;
            OpenSkyRedLight = Application.Current.Resources["OpenSkyRedLight"] as Color? ?? Colors.HotPink;
            OpenSkyDarkGrayHeader = Application.Current.Resources["OpenSkyDarkGrayHeader"] as Color? ?? Colors.HotPink;
            OpenSkyGroupboxBackground = Application.Current.Resources["OpenSkyGroupboxBackground"] as Color? ?? Colors.HotPink;
            OpenSkyWarningOrange = Application.Current.Resources["OpenSkyWarningOrange"] as Color? ?? Colors.HotPink;
            OpenSkySimBrief = Application.Current.Resources["OpenSkySimBrief"] as Color? ?? Colors.HotPink;
            OpenSkySimBriefText = Application.Current.Resources["OpenSkySimBriefText"] as Color? ?? Colors.HotPink;
            OpenSkyLightYellow = Application.Current.Resources["OpenSkyLightYellow"] as Color? ?? Colors.HotPink;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the OpenSky dark gray header.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static Color OpenSkyDarkGrayHeader { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the OpenSky groupbox background.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static Color OpenSkyGroupboxBackground { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the OpenSky light yellow.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static Color OpenSkyLightYellow { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the OpenSky red.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static Color OpenSkyRed { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the OpenSky red light.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static Color OpenSkyRedLight { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the OpenSky simbrief.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static Color OpenSkySimBrief { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the OpenSky simulation brief text.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static Color OpenSkySimBriefText { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the OpenSky teal.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static Color OpenSkyTeal { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the OpenSky teal light.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static Color OpenSkyTealLight { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the OpenSky warning orange.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static Color OpenSkyWarningOrange { get; }
    }
}
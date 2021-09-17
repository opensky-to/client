// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FontOverrides.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Themes
{
    using System.Windows;
    using System.Windows.Media;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// ModernWpf font overrides resource dictionary.
    /// </summary>
    /// <remarks>
    /// sushi.at, 27/07/2021.
    /// </remarks>
    /// <seealso cref="T:System.Windows.ResourceDictionary"/>
    /// -------------------------------------------------------------------------------------------------
    public class FontOverrides : ResourceDictionary
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The resource keys.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private static readonly object[] ResourceKeys =
        {
            SystemFonts.MessageFontFamilyKey,
            "ContentControlThemeFontFamily",
            "PivotHeaderItemFontFamily",
            "PivotTitleFontFamily"
        };

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The font family.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private FontFamily fontFamily;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the font family.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public FontFamily FontFamily
        {
            get => this.fontFamily;
            set
            {
                if (!Equals(this.fontFamily, value))
                {
                    this.fontFamily = value;

                    if (this.fontFamily != null)
                    {
                        foreach (var key in ResourceKeys)
                        {
                            this[key] = this.fontFamily;
                        }
                    }
                    else
                    {
                        foreach (var key in ResourceKeys)
                        {
                            this.Remove(key);
                        }
                    }
                }
            }
        }
    }
}
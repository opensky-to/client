// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OpenSkyPage.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Controls
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    using ModernWpf;
    using ModernWpf.Controls;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// OpenSky page user control.
    /// </summary>
    /// <remarks>
    /// sushi.at, 01/07/2021.
    /// </remarks>
    /// <seealso cref="T:System.Windows.Controls.UserControl"/>
    /// -------------------------------------------------------------------------------------------------
    public class OpenSkyPage : UserControl
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="OpenSkyPage"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 01/07/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public OpenSkyPage()
        {
            var uiThemeResources = new ThemeResources { RequestedTheme = ApplicationTheme.Dark };
            var darkDictionary = new ResourceDictionary();
            darkDictionary.MergedDictionaries.Add(
                new ColorPaletteResources
                {
                    TargetTheme = ApplicationTheme.Dark,
                    Accent = (Color?)ColorConverter.ConvertFromString("#05826c"),
                    AltHigh = (Color?)ColorConverter.ConvertFromString("#404040"),
                    BaseHigh = (Color?)ColorConverter.ConvertFromString("#FFC9C9C9")
                });
            uiThemeResources.ThemeDictionaries.Add("Dark", darkDictionary);

            var xamlControls = new XamlControlsResources();

            var vectorGraphics = new ResourceDictionary { Source = new Uri("/OpenSky.Client;component/Themes/VectorGraphics.xaml", UriKind.RelativeOrAbsolute) };
            var openSkyStyles = new ResourceDictionary { Source = new Uri("/OpenSky.Client;component/Themes/OpenSkyStyles.xaml", UriKind.RelativeOrAbsolute) };
            var gridFilter = new ResourceDictionary { Source = new Uri("/OpenSky.Client;component/Themes/GridFilter.xaml", UriKind.RelativeOrAbsolute) };

            this.Resources.MergedDictionaries.Add(uiThemeResources);
            this.Resources.MergedDictionaries.Add(xamlControls);
            this.Resources.MergedDictionaries.Add(vectorGraphics);
            this.Resources.MergedDictionaries.Add(openSkyStyles);
            this.Resources.MergedDictionaries.Add(gridFilter);

            this.FontFamily = new FontFamily(new Uri("pack://application:,,,/"), "./Fonts/#Montserrat");
            this.FontSize = 13;

            // ReSharper disable once PossibleNullReferenceException
            this.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#c2c2c2"));
        }
    }
}
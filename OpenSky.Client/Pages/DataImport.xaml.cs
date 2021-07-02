// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataImport.xaml.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Pages
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;

    using DataGridExtensions;

    /// -------------------------------------------------------------------------------------------------
    /// <content>
    /// Data import page.
    /// </content>
    /// -------------------------------------------------------------------------------------------------
    public partial class DataImport
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="DataImport"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 17/06/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public DataImport()
        {
            this.InitializeComponent();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Clears all filters on click.
        /// </summary>
        /// <remarks>
        /// sushi.at, 02/07/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Routed event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void ClearAllFiltersOnClick(object sender, RoutedEventArgs e)
        {
            this.DataImportsGrid.GetFilter().Clear();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Textbox text changed (auto scrolls to the end)
        /// </summary>
        /// <remarks>
        /// sushi.at, 02/07/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Text changed event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void TextBoxBaseOnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBoxBase textBox)
            {
                textBox.ScrollToEnd();
            }
        }
    }
}
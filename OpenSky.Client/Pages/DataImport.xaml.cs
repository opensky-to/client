// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataImport.xaml.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Pages
{
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;

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

        private void TextBoxBaseOnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBoxBase textBox)
            {
                textBox.ScrollToEnd();
            }
        }
    }
}
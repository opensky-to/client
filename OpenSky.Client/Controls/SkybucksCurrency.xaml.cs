// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SkybucksCurrency.xaml.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Controls
{
    using System.ComponentModel;
    using System.Windows;

    /// <summary>
    /// Skybucks currency control
    /// </summary>
    public partial class SkybucksCurrency
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The currency property.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static readonly DependencyProperty CurrencyProperty = DependencyProperty.Register("Currency", typeof(double?), typeof(SkybucksCurrency));

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="SkybucksCurrency"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 29/07/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public SkybucksCurrency()
        {
            this.InitializeComponent();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the currency value to display.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Bindable(true)]
        public double? Currency
        {
            get => (double?)this.GetValue(CurrencyProperty);
            set => this.SetValue(CurrencyProperty, value);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether the currency should be shown with fractions.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool CurrencyFractions { get; set; }
    }
}
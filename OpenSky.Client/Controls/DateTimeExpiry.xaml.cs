// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DateTimeExpiry.xaml.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Controls
{
    using System;
    using System.ComponentModel;
    using System.Windows;

    /// -------------------------------------------------------------------------------------------------
    /// <content>
    /// DateTime expiry control.
    /// </content>
    /// -------------------------------------------------------------------------------------------------
    public partial class DateTimeExpiry
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The date time property.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static readonly DependencyProperty DateTimeProperty = DependencyProperty.Register("DateTime", typeof(DateTime), typeof(DateTimeExpiry), new PropertyMetadata(DateTime.UtcNow));

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimeExpiry"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 13/12/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public DateTimeExpiry()
        {
            this.InitializeComponent();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the expiry DateTime.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Bindable(true)]
        public DateTime DateTime
        {
            get => (DateTime)this.GetValue(DateTimeProperty);
            set => this.SetValue(DateTimeProperty, value);
        }
    }
}
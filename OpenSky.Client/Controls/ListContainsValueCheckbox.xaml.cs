// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ListContainsValueCheckbox.xaml.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Controls
{
    using System.Collections;
    using System.ComponentModel;
    using System.Windows;

    /// -------------------------------------------------------------------------------------------------
    /// <content>
    /// List contains value checkbox control.
    /// </content>
    /// -------------------------------------------------------------------------------------------------
    public partial class ListContainsValueCheckbox
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The list property.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static readonly DependencyProperty ListProperty = DependencyProperty.Register("List", typeof(IList), typeof(ListContainsValueCheckbox));

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The value property.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(object), typeof(ListContainsValueCheckbox));

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="ListContainsValueCheckbox"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 19/12/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public ListContainsValueCheckbox()
        {
            this.InitializeComponent();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether the list contains the value (updates list when set).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool Contains
        {
            get
            {
                if (this.Value != null && this.List != null)
                {
                    return this.List.Contains(this.Value);
                }

                return false;
            }

            set
            {
                if (this.Value != null && this.List != null)
                {
                    if (!value)
                    {
                        this.List.Remove(this.Value);
                    }
                    else
                    {
                        this.List.Add(this.Value);
                    }
                }
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the list.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Bindable(true)]
        public IList List
        {
            get => (IList)this.GetValue(ListProperty);
            set => this.SetValue(ListProperty, value);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Bindable(true)]
        public object Value
        {
            get => this.GetValue(ValueProperty);
            set => this.SetValue(ValueProperty, value);
        }
    }
}
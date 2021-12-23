// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MinMaxDoubleFilterWithPopupControl.xaml.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Controls
{
    using System.Globalization;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;

    using DataGridExtensions;

    /// -------------------------------------------------------------------------------------------------
    /// <content>
    /// Filter with range min/max popup control control.
    /// </content>
    /// -------------------------------------------------------------------------------------------------
    public partial class MinMaxDoubleFilterWithPopupControl
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The caption property.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static readonly DependencyProperty CaptionProperty = DependencyProperty.Register(
            "Caption",
            typeof(string),
            typeof(MinMaxDoubleFilterWithPopupControl),
            new FrameworkPropertyMetadata("Specify the limits:", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The filter property.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static readonly DependencyProperty FilterProperty =
            DependencyProperty.Register(
                "Filter",
                typeof(IContentFilter),
                typeof(MinMaxDoubleFilterWithPopupControl),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (sender, _) => ((MinMaxDoubleFilterWithPopupControl)sender).FilterChanged(sender)));

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The is popup visible property.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static readonly DependencyProperty IsPopupVisibleProperty =
            DependencyProperty.Register("IsPopupVisible", typeof(bool), typeof(MinMaxDoubleFilterWithPopupControl), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The maximum property.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
            "Maximum",
            typeof(double),
            typeof(MinMaxDoubleFilterWithPopupControl),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (sender, _) => ((MinMaxDoubleFilterWithPopupControl)sender).RangeChanged()));

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The minimum property.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(
            "Minimum",
            typeof(double),
            typeof(MinMaxDoubleFilterWithPopupControl),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (sender, _) => ((MinMaxDoubleFilterWithPopupControl)sender).RangeChanged()));

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="MinMaxDoubleFilterWithPopupControl"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 23/12/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public MinMaxDoubleFilterWithPopupControl()
        {
            this.InitializeComponent();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the caption.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string Caption
        {
            get => (string)this.GetValue(CaptionProperty);
            set => this.SetValue(CaptionProperty, value);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the converter parameter.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public object ConverterParameter { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the filter.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public IContentFilter Filter
        {
            get => (IContentFilter)this.GetValue(FilterProperty);
            set => this.SetValue(FilterProperty, value);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether the popup is visible.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool IsPopupVisible
        {
            get => (bool)this.GetValue(IsPopupVisibleProperty);
            set => this.SetValue(IsPopupVisibleProperty, value);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the maximum.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double Maximum
        {
            get => (double)this.GetValue(MaximumProperty);
            set => this.SetValue(MaximumProperty, value);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the minimum.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double Minimum
        {
            get => (double)this.GetValue(MinimumProperty);
            set => this.SetValue(MinimumProperty, value);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the value converter.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public IValueConverter ValueConverter { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Filter changed.
        /// </summary>
        /// <remarks>
        /// sushi.at, 23/12/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void FilterChanged(object sender)
        {
            if (this.Filter is not MinMaxDoubleFilter filter)
            {
                return;
            }

            if (sender is not MinMaxDoubleFilterWithPopupControl)
            {
                this.Minimum = filter.Min;
                this.Maximum = filter.Max;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Filter text box on lost focus.
        /// </summary>
        /// <remarks>
        /// sushi.at, 23/12/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Routed event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void FilterTextBoxOnLostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (string.IsNullOrEmpty(textBox.Text))
                {
                    textBox.Text = "0";
                }
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Filter range changed.
        /// </summary>
        /// <remarks>
        /// sushi.at, 23/12/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void RangeChanged()
        {
            if (this.Maximum != 0 || this.Minimum != 0)
            {
                var maximum = this.Maximum;
                var minimum = this.Minimum;

                if (this.ValueConverter != null)
                {
                    var convertedMaximum = (double?)this.ValueConverter.ConvertBack($"{maximum}", typeof(double), this.ConverterParameter, CultureInfo.CurrentCulture);
                    var convertedMinimum = (double?)this.ValueConverter.ConvertBack($"{minimum}", typeof(double), this.ConverterParameter, CultureInfo.CurrentCulture);

                    if (convertedMaximum.HasValue)
                    {
                        maximum = convertedMaximum.Value;
                    }

                    if (convertedMinimum.HasValue)
                    {
                        minimum = convertedMinimum.Value;
                    }
                }

                this.Filter = new MinMaxDoubleFilter(minimum, maximum);
            }
            else
            {
                this.Filter = null;
            }
        }
    }

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// A minimum/maximum double filter.
    /// </summary>
    /// <remarks>
    /// sushi.at, 23/12/2021.
    /// </remarks>
    /// <seealso cref="T:DataGridExtensions.IContentFilter"/>
    /// -------------------------------------------------------------------------------------------------
    public class MinMaxDoubleFilter : IContentFilter
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="MinMaxDoubleFilter"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 23/12/2021.
        /// </remarks>
        /// <param name="min">
        /// The minimum.
        /// </param>
        /// <param name="max">
        /// The maximum.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public MinMaxDoubleFilter(double min, double max)
        {
            this.Min = min;
            this.Max = max;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the maximum.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double Max { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the minimum.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double Min { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Determines whether the specified value matches the condition of this filter.
        /// </summary>
        /// <remarks>
        /// sushi.at, 23/12/2021.
        /// </remarks>
        /// <param name="value">
        /// The content.
        /// </param>
        /// <returns>
        /// <c>true</c> if the specified value matches the condition; otherwise, <c>false</c>.
        /// </returns>
        /// <seealso cref="M:DataGridExtensions.IContentFilter.IsMatch(object?)"/>
        /// -------------------------------------------------------------------------------------------------
        public bool IsMatch(object value)
        {
            if (value == null)
            {
                return false;
            }

            if (!double.TryParse(value.ToString(), out var number))
            {
                return false;
            }

            return (this.Min == 0 || number >= this.Min) && (this.Max == 0 || number <= this.Max);
        }
    }
}
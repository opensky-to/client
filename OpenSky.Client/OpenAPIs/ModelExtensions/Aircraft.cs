// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Aircraft.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace

namespace OpenSkyApi
{
    using System.Globalization;
    using System.Windows;
    using System.Windows.Controls;

    using OpenSky.Client.Converters;
    using OpenSky.Client.Tools;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Aircraft extensions.
    /// </summary>
    /// <remarks>
    /// sushi.at, 29/10/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public partial class Aircraft
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <remarks>
        /// sushi.at, 31/10/2021.
        /// </remarks>
        /// <param name="obj">
        /// The object to compare with the current object.
        /// </param>
        /// <returns>
        /// <see langword="true" /> if the specified object  is equal to the current object; otherwise,
        /// <see langword="false" />.
        /// </returns>
        /// <seealso cref="M:System.Object.Equals(object)"/>
        /// -------------------------------------------------------------------------------------------------
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return this.Equals((Aircraft)obj);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <remarks>
        /// sushi.at, 31/10/2021.
        /// </remarks>
        /// <returns>
        /// A hash code for the current object.
        /// </returns>
        /// <seealso cref="M:System.Object.GetHashCode()"/>
        /// -------------------------------------------------------------------------------------------------
        public override int GetHashCode()
        {
            // ReSharper disable NonReadonlyMemberInGetHashCode
            return this.Registry != null ? this.Registry.GetHashCode() : 0;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <remarks>
        /// sushi.at, 29/10/2021.
        /// </remarks>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        /// <seealso cref="M:System.Object.ToString()"/>
        /// -------------------------------------------------------------------------------------------------
        public override string ToString()
        {
            if (string.IsNullOrEmpty(this.Registry))
            {
                return string.Empty;
            }

            var displayString = $"{this.Registry.RemoveSimPrefix()}";
            if (!string.IsNullOrEmpty(this.Name))
            {
                displayString += $" ({this.Name})";
            }

            if (this.Registry != "----")
            {
                displayString += $": {this.Type.Name}";
            }
            else
            {
                displayString += $"{this.Type.Name}";
            }

            return displayString;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Tests if this Aircraft is considered equal to another.
        /// </summary>
        /// <remarks>
        /// sushi.at, 31/10/2021.
        /// </remarks>
        /// <param name="other">
        /// The aircraft to compare to this object.
        /// </param>
        /// <returns>
        /// True if the objects are considered equal, false if they are not.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        protected bool Equals(Aircraft other)
        {
            return this.Registry == other.Registry;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the distance from the aircraft to the origin airport (used in flight planning page).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string DistanceInfo
        {
            get
            {
                var converter = new SettingsUnitConverter();
                if (this.Registry != "----")
                {
                    var info = $"[{this.AirportICAO}";
                    if (this.Distance > 0)
                    {
                        info += $", {converter.Convert(this.Distance, typeof(string), "distance|F0|true", CultureInfo.CurrentCulture)}";
                    }

                    return $"{info}] [{this.Type.Simulator}]";
                }

                return string.Empty;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the distance to the origin airport (used for sorting in flight planning page).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int Distance { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the total payload weight in lbs currently loaded onto the aircraft.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double TotalPayloadWeight
        {
            get
            {
                var totalWeight = 0.0;
                foreach (var payload in this.Payloads)
                {
                    totalWeight += payload.Weight;
                }

                return totalWeight;
            }
        }
    }

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Aircraft select or not select combobox item style selector.
    /// </summary>
    /// <remarks>
    /// sushi.at, 18/12/2021.
    /// </remarks>
    /// <seealso cref="T:System.Windows.Controls.StyleSelector"/>
    /// -------------------------------------------------------------------------------------------------
    public class AircraftSelectOrNotSelectComboItemStyleSelector : StyleSelector
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// When overridden in a derived class, returns a <see cref="T:System.Windows.Style" /> based on
        /// custom logic.
        /// </summary>
        /// <remarks>
        /// sushi.at, 18/12/2021.
        /// </remarks>
        /// <param name="item">
        /// The content.
        /// </param>
        /// <param name="container">
        /// The element to which the style will be applied.
        /// </param>
        /// <returns>
        /// Returns an application-specific style to apply; otherwise, <see langword="null" />.
        /// </returns>
        /// <seealso cref="M:System.Windows.Controls.StyleSelector.SelectStyle(object,DependencyObject)"/>
        /// -------------------------------------------------------------------------------------------------
        public override Style SelectStyle(object item, DependencyObject container)
        {
            if (item is Aircraft aircraft)
            {
                var style = new Style(typeof(ComboBoxItem)) { BasedOn = (Style)Application.Current.FindResource("DefaultComboBoxItemStyle") };
                if (aircraft.Registry == "----")
                {
                    style.Setters.Add(new Setter(UIElement.IsHitTestVisibleProperty, false));
                    style.Setters.Add(new Setter(UIElement.FocusableProperty, false));
                    style.Setters.Add(new Setter(Control.FontSizeProperty, 10.0));
                    style.Setters.Add(new Setter(Control.FontWeightProperty, FontWeights.Bold));
                    style.Setters.Add(new Setter(FrameworkElement.MarginProperty, new Thickness(-10, aircraft.Type.Name.Contains("other") ? 16 : 0, 0, 0)));
                }

                return style;
            }

            return base.SelectStyle(item, container);
        }
    }
}
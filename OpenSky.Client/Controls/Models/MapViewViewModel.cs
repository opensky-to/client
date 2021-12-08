// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MapViewViewModel.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Controls.Models
{
    using System;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Controls;

    using OpenSky.Client.MVVM;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Mapview view model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 04/11/2021.
    /// </remarks>
    /// <seealso cref="T:OpenSky.Client.MVVM.ViewModel"/>
    /// -------------------------------------------------------------------------------------------------
    public class MapViewViewModel : ViewModel
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// True to darken road map.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private bool darkenRoadMap = true;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The dark road map visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Visibility darkRoadMapVisibility = Visibility.Visible;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The last user map interaction date/time.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private DateTime lastUserMapInteraction = DateTime.MinValue;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The selected map mode.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private ComboBoxItem selectedMapMode;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether to the darken the road map.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool DarkenRoadMap
        {
            get => this.darkenRoadMap;

            set
            {
                if (Equals(this.darkenRoadMap, value))
                {
                    return;
                }

                this.darkenRoadMap = value;
                this.NotifyPropertyChanged();
                Debug.WriteLine($"Darken road map toggled {value}");

                if (this.SelectedMapMode.Content is string mode)
                {
                    this.DarkRoadMapVisibility = mode.Equals("Road", StringComparison.InvariantCultureIgnoreCase) && this.DarkenRoadMap ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the dark road map visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Visibility DarkRoadMapVisibility
        {
            get => this.darkRoadMapVisibility;

            set
            {
                if (Equals(this.darkRoadMapVisibility, value))
                {
                    return;
                }

                this.darkRoadMapVisibility = value;
                this.NotifyPropertyChanged();
            }
        }
        
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the date/time of the last user map interaction.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public DateTime LastUserMapInteraction
        {
            get => this.lastUserMapInteraction;

            set
            {
                if (Equals(this.lastUserMapInteraction, value))
                {
                    return;
                }

                this.lastUserMapInteraction = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the selected map mode.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ComboBoxItem SelectedMapMode
        {
            get => this.selectedMapMode;

            set
            {
                if (Equals(this.selectedMapMode, value))
                {
                    return;
                }

                this.selectedMapMode = value;
                this.NotifyPropertyChanged();
                Debug.WriteLine($"Map mode changed {value.Content}");

                if (this.SelectedMapMode.Content is string mode)
                {
                    this.DarkRoadMapVisibility = mode.Equals("Road", StringComparison.InvariantCultureIgnoreCase) && this.DarkenRoadMap ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }
    }
}
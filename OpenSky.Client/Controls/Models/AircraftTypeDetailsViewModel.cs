// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AircraftTypeDetailsViewModel.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Controls.Models
{
    using OpenSky.Client.MVVM;

    using OpenSkyApi;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Aircraft type details user control view model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 16/02/2022.
    /// </remarks>
    /// <seealso cref="OpenSky.Client.MVVM.ViewModel"/>
    /// -------------------------------------------------------------------------------------------------
    public class AircraftTypeDetailsViewModel : ViewModel
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The aircraft type.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private AircraftType type;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the aircraft type.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AircraftType Type
        {
            get => this.type;
        
            set
            {
                if(Equals(this.type, value))
                {
                   return;
                }
        
                this.type = value;
                this.NotifyPropertyChanged();
            }
        }
    }
}
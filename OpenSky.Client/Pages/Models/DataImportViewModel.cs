// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataImportViewModel.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Pages.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using OpenSky.Client.MVVM;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Data import view model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 01/07/2021.
    /// </remarks>
    /// <seealso cref="T:OpenSky.Client.MVVM.ViewModel"/>
    /// -------------------------------------------------------------------------------------------------
    public class DataImportViewModel : ViewModel
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="DataImportViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 01/07/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public DataImportViewModel()
        {
            // Create commands
            this.BrowseLittleNavmapMSFSCommand = new Command(this.BrowseLittleNavmapMSFS);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Browse for Little Navmap MSFS SQLite database file.
        /// </summary>
        /// <remarks>
        /// sushi.at, 01/07/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void BrowseLittleNavmapMSFS()
        {
            
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the browse little navmap msfs command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command BrowseLittleNavmapMSFSCommand { get; }
    }
}

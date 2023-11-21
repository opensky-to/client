// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AircraftTypeCategoryWithDescriptions.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.OpenAPIs.ModelExtensions
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;

    using OpenSky.Client.Tools;

    using OpenSkyApi;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Copy of the API enum, but still containing the description texts.
    /// </summary>
    /// <remarks>
    /// sushi.at, 25/07/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public enum AircraftTypeCategoryWithDescriptions
    {
        /// <summary>
        /// Single Engine Piston.
        /// </summary>
        [Description("Single Engine Piston")]
        SEP = 0,

        /// <summary>
        /// Multi Engine Piston.
        /// </summary>
        [Description("Multi Engine Piston")]
        MEP = 1,

        /// <summary>
        /// Single Engine Turboprop.
        /// </summary>
        [Description("Single Engine Turboprop")]
        SET = 2,

        /// <summary>
        /// Multi Engine Turboprop.
        /// </summary>
        [Description("Multi Engine Turboprop")]
        MET = 3,

        /// <summary>
        /// Small private and business jets.
        /// </summary>
        [Description("Small private and business jets")]
        JET = 4,

        /// <summary>
        /// Regional Airliner Jets.
        /// </summary>
        [Description("Regional Airliner Jets")]
        REG = 5,

        /// <summary>
        /// Narrow-Body Airliner.
        /// </summary>
        [Description("Narrow-Body Airliner")]
        NBA = 6,

        /// <summary>
        /// Wide-Body Airliner.
        /// </summary>
        [Description("Wide-Body Airliner")]
        WBA = 7,

        /// <summary>
        /// Helicopter.
        /// </summary>
        [Description("Helicopter")]
        HEL = 8
    }

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Aircraft type category combo item.
    /// </summary>
    /// <remarks>
    /// sushi.at, 25/07/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class AircraftTypeCategoryComboItem
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="AircraftTypeCategoryComboItem"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 25/07/2021.
        /// </remarks>
        /// <param name="category">
        /// The category.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private AircraftTypeCategoryComboItem(AircraftTypeCategoryWithDescriptions category)
        {
            this.AircraftTypeCategory = (AircraftTypeCategory)category;
            this.Description = category.GetDescription();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the aircraft type category.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AircraftTypeCategory AircraftTypeCategory { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the description text.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string Description { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets all aircraft type category combo items.
        /// </summary>
        /// <remarks>
        /// sushi.at, 25/07/2021.
        /// </remarks>
        /// <returns>
        /// The aircraft type category combo items.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public static IEnumerable<AircraftTypeCategoryComboItem> GetAircraftTypeCategoryComboItems()
        {
            return (from AircraftTypeCategoryWithDescriptions category in Enum.GetValues(typeof(AircraftTypeCategoryWithDescriptions)) select new AircraftTypeCategoryComboItem(category)).ToList();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <remarks>
        /// sushi.at, 25/07/2021.
        /// </remarks>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        /// <seealso cref="M:System.Object.ToString()"/>
        /// -------------------------------------------------------------------------------------------------
        public override string ToString()
        {
            return $"{this.AircraftTypeCategory} - {this.Description}";
        }
    }
}
﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AircraftType.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace

namespace OpenSkyApi
{
    using System;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Aircraft type extensions.
    /// </summary>
    /// <remarks>
    /// sushi.at, 21/07/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public partial class AircraftType
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="AircraftType"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 11/06/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public AircraftType()
        {
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="AircraftType"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 11/06/2021.
        /// </remarks>
        /// <param name="copyFrom">
        /// The other type to copy from.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public AircraftType(AircraftType copyFrom)
        {
            this.AtcModel = copyFrom.AtcModel;
            this.AtcType = copyFrom.AtcType;
            this.Category = copyFrom.Category;
            this.Comments = copyFrom.Comments;
            this.DetailedChecksDisabled = copyFrom.DetailedChecksDisabled;
            this.EmptyWeight = copyFrom.EmptyWeight;
            this.Enabled = copyFrom.Enabled;
            this.EngineCount = copyFrom.EngineCount;
            this.EngineType = copyFrom.EngineType;
            this.FlapsAvailable = copyFrom.FlapsAvailable;
            this.FuelTotalCapacity = copyFrom.FuelTotalCapacity;
            this.FuelWeightPerGallon = copyFrom.FuelWeightPerGallon;
            this.Id = copyFrom.Id;
            this.IsGearRetractable = copyFrom.IsGearRetractable;
            this.IsVanilla = copyFrom.IsVanilla;
            this.IncludeInWorldPopulation = copyFrom.IncludeInWorldPopulation;
            this.IsVariantOf = copyFrom.IsVariantOf;
            this.LastEditedByID = copyFrom.LastEditedByID;
            this.LastEditedByName = copyFrom.LastEditedByName;
            this.MaxGrossWeight = copyFrom.MaxGrossWeight;
            this.MinimumRunwayLength = copyFrom.MinimumRunwayLength;
            this.MaxPrice = copyFrom.MaxPrice;
            this.MinPrice = copyFrom.MinPrice;
            this.Name = copyFrom.Name;
            this.Manufacturer = copyFrom.Manufacturer;
            this.NeedsCoPilot = copyFrom.NeedsCoPilot;
            this.RequiresManualFuelling = copyFrom.RequiresManualFuelling;
            this.RequiresManualLoading = copyFrom.RequiresManualLoading;
            this.NextVersion = copyFrom.NextVersion;
            this.Simulator = copyFrom.Simulator;
            this.UploaderID = copyFrom.UploaderID;
            this.UploaderName = copyFrom.UploaderName;
            this.VersionNumber = copyFrom.VersionNumber;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the name with variant star.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------

        // ReSharper disable once InconsistentNaming
        public string NameWithVariant => $"{this.Name}{(this.HasVariants ? "*" : string.Empty)}";

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the vanilla/addon image.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ImageSource VanillaAddonImage => new BitmapImage(
            new Uri(
                $"pack://application:,,,/OpenSky.Client;component/Resources/{(this.IsVanilla ? "vanilla16.png" : "addon16.png")}"));

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <remarks>
        /// sushi.at, 03/06/2021.
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

            return this.Equals((AircraftType)obj);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <remarks>
        /// sushi.at, 03/06/2021.
        /// </remarks>
        /// <returns>
        /// A hash code for the current object.
        /// </returns>
        /// <seealso cref="M:System.Object.GetHashCode()"/>
        /// -------------------------------------------------------------------------------------------------
        public override int GetHashCode()
        {
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            return this.Id.GetHashCode();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <remarks>
        /// sushi.at, 21/07/2021.
        /// </remarks>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        /// <seealso cref="M:System.Object.ToString()"/>
        /// -------------------------------------------------------------------------------------------------
        public override string ToString()
        {
            return $"{this.Name} [v{this.VersionNumber}]";
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <remarks>
        /// sushi.at, 03/06/2021.
        /// </remarks>
        /// <param name="other">
        /// The aircraft type to compare to this object.
        /// </param>
        /// <returns>
        /// <see langword="true" /> if the specified object  is equal to the current object; otherwise,
        /// <see langword="false" />.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        protected bool Equals(AircraftType other)
        {
            return this.Id.Equals(other.Id);
        }
    }
}
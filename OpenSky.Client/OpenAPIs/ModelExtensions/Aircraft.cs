// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Aircraft.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace

namespace OpenSkyApi
{
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
            var displayString = $"{this.Registry}";
            if (!string.IsNullOrEmpty(this.Name))
            {
                displayString += $" ({this.Name})";
            }

            displayString += $": {this.Type.Name}";
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
    }
}
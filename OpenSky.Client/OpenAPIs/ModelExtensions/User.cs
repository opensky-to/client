// --------------------------------------------------------------------------------------------------------------------
// <copyright file="User.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace

namespace OpenSkyApi
{
    using System;
    using System.Linq;
    using System.Windows;
    using System.Windows.Media;

    using OpenSky.Client;

    public partial class User
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the admin role visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Visibility AdminRoleVisibility => this.Roles.Contains("Admin") ? Visibility.Visible : Visibility.Collapsed;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the color of the last login date string.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Brush LastLoginDateColor
        {
            get
            {
                if (this.LastLogin.HasValue && (DateTime.Now - this.LastLogin.Value.DateTime).TotalDays > 90)
                {
                    return new SolidColorBrush(OpenSkyColors.OpenSkyRedLight);
                }

                return new SolidColorBrush(Color.FromRgb(0xc2, 0xc2, 0xc2));
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the last login geo country code.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string LastLoginGeoCountryCode
        {
            get
            {
                if (!string.IsNullOrEmpty(this.LastLoginGeo) && this.LastLoginGeo.Contains('(') && this.LastLoginGeo.Contains(')'))
                {
                    return this.LastLoginGeo.Substring(this.LastLoginGeo.IndexOf('(') + 1, this.LastLoginGeo.IndexOf(')') - this.LastLoginGeo.IndexOf('(') - 1);
                }

                return null;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the moderator role visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Visibility ModeratorRoleVisibility => this.Roles.Contains("Moderator") ? Visibility.Visible : Visibility.Collapsed;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the color of the registered date string.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Brush RegisteredDateColor
        {
            get
            {
                if ((DateTime.Now - this.RegisteredOn.DateTime).TotalDays > 7 && !this.LastLogin.HasValue)
                {
                    return new SolidColorBrush(OpenSkyColors.OpenSkyRedLight);
                }

                return new SolidColorBrush(Color.FromRgb(0xc2, 0xc2, 0xc2));
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the user role visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Visibility UserRoleVisibility => this.Roles.Contains("User") ? Visibility.Visible : Visibility.Collapsed;
    }
}
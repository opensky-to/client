// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DocumentHeaderEx.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Controls
{
    using System;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Document header (text + icon)
    /// </summary>
    /// <remarks>
    /// sushi.at, 23/06/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class DocumentHeaderEx
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentHeaderEx"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 23/06/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public DocumentHeaderEx()
        {
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentHeaderEx"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 23/06/2021.
        /// </remarks>
        /// <param name="text">
        /// The text.
        /// </param>
        /// <param name="icon">
        /// The icon URI.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public DocumentHeaderEx(string text, string icon = null)
        {
            this.Text = text;

            if (!string.IsNullOrEmpty(icon))
            {
                this.Icon = new BitmapImage(new Uri(icon));
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentHeaderEx"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 23/06/2021.
        /// </remarks>
        /// <param name="text">
        /// The text.
        /// </param>
        /// <param name="icon">
        /// The icon.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public DocumentHeaderEx(string text, ImageSource icon)
        {
            this.Text = text;
            this.Icon = icon;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the icon.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ImageSource Icon { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string Text { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <remarks>
        /// sushi.at, 23/06/2021.
        /// </remarks>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        /// <seealso cref="M:System.Object.ToString()"/>
        /// -------------------------------------------------------------------------------------------------
        public override string ToString()
        {
            return this.Text;
        }
    }
}
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DocumentHeaderEx.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Controls
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    using JetBrains.Annotations;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Document header (text + icon)
    /// </summary>
    /// <remarks>
    /// sushi.at, 23/06/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class DocumentHeaderEx : INotifyPropertyChanged
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The icon.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private ImageSource icon;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The header text.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string text;

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
        /// Occurs when a property value changes.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public event PropertyChangedEventHandler PropertyChanged;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the icon.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ImageSource Icon
        {
            get => this.icon;

            set
            {
                if (Equals(this.icon, value))
                {
                    return;
                }

                this.icon = value;
                this.OnPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string Text
        {
            get => this.text;

            set
            {
                if (Equals(this.text, value))
                {
                    return;
                }

                this.text = value;
                this.OnPropertyChanged();
            }
        }

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

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Executes the property changed action.
        /// </summary>
        /// <remarks>
        /// sushi.at, 31/10/2021.
        /// </remarks>
        /// <param name="propertyName">
        /// (Optional) Name of the property.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
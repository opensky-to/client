// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DockItemEx.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Controls
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows;

    using JetBrains.Annotations;

    using Syncfusion.Windows.Tools.Controls;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Dock item extensions for property changed notifications.
    /// </summary>
    /// <remarks>
    /// sushi.at, 22/06/2021.
    /// </remarks>
    /// <seealso cref="T:Syncfusion.Windows.Tools.Controls.DockItem"/>
    /// <seealso cref="T:System.ComponentModel.INotifyPropertyChanged"/>
    /// -------------------------------------------------------------------------------------------------
    public class DockItemEx : DockItem, INotifyPropertyChanged
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The document header property.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static readonly DependencyProperty DocumentHeaderProperty = DependencyProperty.Register("DocumentHeader", typeof(DocumentHeaderEx), typeof(DockItemEx));

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public event PropertyChangedEventHandler PropertyChanged;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the content.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public new FrameworkElement Content
        {
            get => (FrameworkElement)this.GetValue(ContentProperty);

            set
            {
                if (Equals((FrameworkElement)this.GetValue(ContentProperty), value))
                {
                    return;
                }

                this.SetValue(ContentProperty, value);
                this.OnPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the document header.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public DocumentHeaderEx DocumentHeader
        {
            get => (DocumentHeaderEx)this.GetValue(DocumentHeaderProperty);

            set
            {
                if (Equals((DocumentHeaderEx)this.GetValue(DocumentHeaderProperty), value))
                {
                    return;
                }

                this.SetValue(DocumentHeaderProperty, value);
                this.OnPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Executes the property changed action.
        /// </summary>
        /// <remarks>
        /// sushi.at, 22/06/2021.
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
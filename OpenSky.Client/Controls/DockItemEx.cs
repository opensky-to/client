// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DockItemEx.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Controls
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Windows.Media;

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

        ///// -------------------------------------------------------------------------------------------------
        ///// <summary>
        ///// Gets or sets the header.
        ///// </summary>
        ///// -------------------------------------------------------------------------------------------------
        //public new string Header
        //{
        //    get => (string)this.GetValue(HeaderProperty);

        //    set
        //    {
        //        if (Equals((string)this.GetValue(HeaderProperty), value))
        //        {
        //            return;
        //        }

        //        this.SetValue(HeaderProperty, value);
        //        this.OnPropertyChanged();
        //    }
        //}

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The document header property.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static readonly DependencyProperty DocumentHeaderProperty = DependencyProperty.Register("DocumentHeader", typeof(DocumentHeaderEx), typeof(DockItemEx));

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
        /// Gets or sets the icon.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public new Brush Icon
        {
            get => (Brush)this.GetValue(IconProperty);

            set
            {
                if (Equals((Brush)this.GetValue(IconProperty), value))
                {
                    return;
                }

                this.SetValue(IconProperty, value);
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
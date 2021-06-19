// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TabItemEx.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Controls
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    using ModernWpf.Controls;
    using ModernWpf.Controls.Primitives;

    using OpenSky.Client.Converters;

    public class TabItemEx : TabItem
    {
        private readonly NavigationViewBitmapIconConverter converter = new ();

        public TabItemEx(object header, object content, bool isSelected = false, object icon = null)
        {
            this.Header = header;
            this.Content = content;
            this.IsSelected = isSelected;
            this.Icon = icon;
        }

        public object Icon
        {
            get => TabItemHelper.GetIcon(this);

            set
            {
                if (value is string s)
                {
                    TabItemHelper.SetIcon(this, this.converter.Convert(s, typeof(BitmapIcon), null, CultureInfo.CurrentCulture));
                }
                else if (value is BitmapIcon icon)
                {
                    TabItemHelper.SetIcon(this, new BitmapIcon{UriSource = icon.UriSource, ShowAsMonochrome = false});
                }
                else
                {
                    TabItemHelper.SetIcon(this, value);
                }
            }
        }
    }
}

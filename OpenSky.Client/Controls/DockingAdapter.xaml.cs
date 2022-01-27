// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DockingAdapter.xaml.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Controls
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    using OpenSky.Client.Tools;

    using Syncfusion.SfSkinManager;
    using Syncfusion.Themes.MaterialDark.WPF;
    using Syncfusion.Windows.Tools.Controls;

    /// -------------------------------------------------------------------------------------------------
    /// <content>
    /// MVVM Docking adapter for docking manager.
    /// </content>
    /// -------------------------------------------------------------------------------------------------
    public partial class DockingAdapter
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Active document dependency property.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static readonly DependencyProperty ActiveDocumentProperty = DependencyProperty.Register("ActiveDocument", typeof(DockItemEx), typeof(DockingAdapter), new PropertyMetadata(null, OnActiveDocumentChanged));

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Items source dependency property.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(IList), typeof(DockingAdapter), new PropertyMetadata(null));

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="DockingAdapter"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 22/06/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public DockingAdapter()
        {
            // ReSharper disable PossibleNullReferenceException
            var themeSettings = new MaterialDarkThemeSettings
            {
                PrimaryBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#05826c")),
                PrimaryForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC9C9C9")),
                PrimaryColorForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC9C9C9")),

                BodyFontSize = 14,
                HeaderFontSize = 18,
                SubHeaderFontSize = 17,
                TitleFontSize = 17,
                SubTitleFontSize = 16,
                BodyAltFontSize = 14,
                FontFamily = new FontFamily("Montserrat"),
            };

            // ReSharper restore PossibleNullReferenceException
            SfSkinManager.RegisterThemeSettings("MaterialDark", themeSettings);

            this.InitializeComponent();
            SfSkinManager.SetTheme(this.PART_DockingManager, new Theme("MaterialDark"));
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the active document.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public DockItemEx ActiveDocument
        {
            get => (DockItemEx)this.GetValue(ActiveDocumentProperty);
            set => this.SetValue(ActiveDocumentProperty, value);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the items source.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public IList ItemsSource
        {
            get => (IList)this.GetValue(ItemsSourceProperty);
            set => this.SetValue(ItemsSourceProperty, value);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Find data template for type and element.
        /// </summary>
        /// <remarks>
        /// sushi.at, 22/06/2021.
        /// </remarks>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <param name="element">
        /// The element.
        /// </param>
        /// <returns>
        /// The found data template or NULL if no template was found.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public DataTemplate FindDataTemplate(Type type, FrameworkElement element)
        {
            if (element.TryFindResource(type) is DataTemplate dataTemplate)
            {
                return dataTemplate;
            }

            if (type.BaseType != null && type.BaseType != typeof(object))
            {
                dataTemplate = this.FindDataTemplate(type.BaseType, element);
                if (dataTemplate != null)
                {
                    return dataTemplate;
                }
            }

            foreach (var interfaceType in type.GetInterfaces())
            {
                dataTemplate = this.FindDataTemplate(interfaceType, element);
                if (dataTemplate != null)
                {
                    return dataTemplate;
                }
            }

            return null;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Invoked whenever the effective value of any dependency property on this
        /// <see cref="T:System.Windows.FrameworkElement" /> has been updated. The specific dependency
        /// property that changed is reported in the arguments parameter. Overrides
        /// <see cref="M:System.Windows.DependencyObject.OnPropertyChanged(System.Windows.DependencyPropertyChangedEventArgs)" />.
        /// </summary>
        /// <remarks>
        /// sushi.at, 22/06/2021.
        /// </remarks>
        /// <param name="e">
        /// The event data that describes the property that changed, as well as old and new values.
        /// </param>
        /// <seealso cref="M:System.Windows.FrameworkElement.OnPropertyChanged(DependencyPropertyChangedEventArgs)"/>
        /// -------------------------------------------------------------------------------------------------
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property.Name == "ItemsSource")
            {
                if (e.OldValue is INotifyCollectionChanged oldcollection)
                {
                    // Remove event listener
                    oldcollection.CollectionChanged -= this.CollectionChanged;
                }

                if (e.NewValue != null)
                {
                    // Process items in new source
                    if (this.PART_DockingManager.DocContainer != null)
                    {
                        ((DocumentContainer)this.PART_DockingManager.DocContainer).AddTabDocumentAtLast = true;
                    }

                    var count = 0;
                    foreach (var item in (IList)e.NewValue)
                    {
                        if (item is DockItemEx element)
                        {
                            var control = new ContentControl { Content = element.Content };
                            DockingManager.SetHeader(control, element.DocumentHeader);
                            DockingManager.SetCanDock(control, element.CanDock);
                            DockingManager.SetCanClose(control, element.CanClose);
                            DockingManager.SetCanFloat(control, element.CanFloat);
                            if (element.State == DockState.Document)
                            {
                                DockingManager.SetState(control, DockState.Document);
                            }
                            else
                            {
                                if (count != 0)
                                {
                                    DockingManager.SetTargetNameInDockedMode(control, $"item{count - 1}");
                                    DockingManager.SetSideInDockedMode(control, DockSide.Right);
                                }

                                DockingManager.SetDesiredWidthInDockedMode(control, 200);
                                control.Name = $"item{count++}";
                            }

                            element.PropertyChanged += (_, args) =>
                            {
                                if (args.PropertyName == nameof(DockItemEx.DocumentHeader))
                                {
                                    DockingManager.SetHeader(control, element.DocumentHeader);
                                    DockingManager.SelectTab(control);
                                }

                                if (args.PropertyName == nameof(DockItemEx.Content))
                                {
                                    control.Content = element.Content;
                                }
                            };

                            this.PART_DockingManager.Children.Add(control);
                        }
                    }

                    // Add collection event changed listener
                    if (e.NewValue is INotifyCollectionChanged newcollection)
                    {
                        newcollection.CollectionChanged += this.CollectionChanged;
                    }
                }
            }

            base.OnPropertyChanged(e);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Active document changed, find the actual content control for it and make it active.
        /// </summary>
        /// <remarks>
        /// sushi.at, 22/06/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="args">
        /// Event information to send to registered event handlers.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private static void OnActiveDocumentChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (sender is DockingAdapter adapter)
            {
                foreach (FrameworkElement element in adapter.PART_DockingManager.Children)
                {
                    if (element is ContentControl control)
                    {
                        if (args.NewValue is DockItemEx dockItem)
                        {
                            if (Equals(control.Content, dockItem.Content))
                            {
                                adapter.PART_DockingManager.ActiveWindow = control;
                                break;
                            }
                        }
                        else if (control.Content == args.NewValue)
                        {
                            adapter.PART_DockingManager.ActiveWindow = control;
                            break;
                        }
                    }
                }
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Items source collection changed.
        /// </summary>
        /// <remarks>
        /// sushi.at, 22/06/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Notify collection changed event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    if (item is DockItemEx dockItem)
                    {
                        var control = (from ContentControl element in this.PART_DockingManager.Children
                                       where Equals(element.Content, dockItem.Content)
                                       select element).FirstOrDefault();
                        if (control != null)
                        {
                            this.PART_DockingManager.Children.Remove(control);
                            if (this.ItemsSource.Count == 0)
                            {
                                this.ActiveDocument = null;
                            }
                        }
                    }
                    else
                    {
                        var control = (from ContentControl element in this.PART_DockingManager.Children
                                       where element.Content == item
                                       select element).FirstOrDefault();
                        this.PART_DockingManager.Children.Remove(control);
                        if (this.ItemsSource.Count == 0)
                        {
                            this.ActiveDocument = null;
                        }
                    }
                }
            }

            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    if (item is DockItemEx element)
                    {
                        var control = new ContentControl { Content = element.Content };
                        DockingManager.SetHeader(control, element.DocumentHeader);
                        DockingManager.SetCanDock(control, element.CanDock);
                        DockingManager.SetCanClose(control, element.CanClose);
                        DockingManager.SetCanFloat(control, element.CanFloat);
                        if (element.State == DockState.Document)
                        {
                            DockingManager.SetState(control, DockState.Document);
                        }

                        element.PropertyChanged += (_, args) =>
                        {
                            if (args.PropertyName == nameof(DockItemEx.DocumentHeader))
                            {
                                DockingManager.SetHeader(control, element.DocumentHeader);
                                this.PART_DockingManager.ActiveWindow = null;

                                new Thread(
                                    () =>
                                    {
                                        UpdateGUIDelegate reselect = () => this.PART_DockingManager.ActiveWindow = control;
                                        this.Dispatcher.BeginInvoke(reselect);
                                    }).Start();
                            }

                            if (args.PropertyName == nameof(DockItemEx.Content))
                            {
                                control.Content = element.Content;
                            }
                        };

                        this.PART_DockingManager.Children.Add(control);
                    }
                }
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Docking manager active window changed.
        /// </summary>
        /// <remarks>
        /// sushi.at, 22/06/2021.
        /// </remarks>
        /// <param name="d">
        /// A DependencyObject to process.
        /// </param>
        /// <param name="e">
        /// Dependency property changed event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void PART_DockingManager_ActiveWindowChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is ContentControl contentControl)
            {
                foreach (var item in this.ItemsSource)
                {
                    if (item is DockItemEx dockItem)
                    {
                        if (Equals(dockItem.Content, contentControl.Content))
                        {
                            this.ActiveDocument = dockItem;
                            break;
                        }
                    }
                }
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Docking manager tab close button clicked.
        /// </summary>
        /// <remarks>
        /// sushi.at, 22/06/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Close button event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void PART_DockingManager_CloseButtonClick(object sender, CloseButtonEventArgs e)
        {
            if (e.TargetItem is ContentControl contentControl)
            {
                foreach (var item in this.ItemsSource)
                {
                    if (item is DockItemEx dockItem)
                    {
                        if (Equals(dockItem.Content, contentControl.Content))
                        {
                            if (dockItem.Content is OpenSkyPage page)
                            {
                                page.CloseButtonClick(sender, e);
                                if (e.Cancel)
                                {
                                    // Page didn't want to be closed
                                    return;
                                }
                            }

                            this.ItemsSource.Remove(dockItem);
                            break;
                        }
                    }
                }
            }

            if (this.ItemsSource.Count == 0)
            {
                this.ActiveDocument = null;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Docking manager loaded.
        /// </summary>
        /// <remarks>
        /// sushi.at, 22/06/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Routed event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void PART_DockingManager_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.PART_DockingManager.DocContainer is DocumentContainer docContainer)
            {
                docContainer.AddTabDocumentAtLast = true;
            }

            this.PART_DockingManager.DocumentCloseButtonType = CloseButtonType.Individual;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Docking manager children collection changed.
        /// </summary>
        /// <remarks>
        /// sushi.at, 25/11/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Notify collection changed event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void PART_DockingManager_OnChildrenCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (var remove in e.OldItems)
                {
                    if (remove is ContentControl contentControl)
                    {
                        // Check if we still have that page in our collection
                        foreach (var item in this.ItemsSource)
                        {
                            if (item is DockItemEx dockItem)
                            {
                                if (Equals(dockItem.Content, contentControl.Content))
                                {
                                    this.ItemsSource.Remove(dockItem);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Docking manager close all tabs.
        /// </summary>
        /// <remarks>
        /// sushi.at, 23/06/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Close tab event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void PART_DockingManager_OnCloseAllTabs(object sender, CloseTabEventArgs e)
        {
            Debug.WriteLine(e.ClosingTabItems);
            foreach (var closingTabItem in e.ClosingTabItems)
            {
                if (closingTabItem is ContentControl { Content: ContentPresenter { Content: ContentControl contentControl } })
                    foreach (var item in this.ItemsSource)
                    {
                        if (item is DockItemEx dockItem)
                        {
                            if (Equals(dockItem.Content, contentControl.Content))
                            {
                                this.ItemsSource.Remove(dockItem);
                                break;
                            }
                        }
                    }
            }

            if (this.ItemsSource.Count == 0)
            {
                this.ActiveDocument = null;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Docking manager close all other tabs.
        /// </summary>
        /// <remarks>
        /// sushi.at, 23/06/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Close tab event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void PART_DockingManager_OnCloseOtherTabs(object sender, CloseTabEventArgs e)
        {
            Debug.WriteLine(e.ClosingTabItems);
            foreach (var closingTabItem in e.ClosingTabItems)
            {
                if (closingTabItem is ContentControl { Content: ContentPresenter { Content: ContentControl contentControl } })
                    foreach (var item in this.ItemsSource)
                    {
                        if (item is DockItemEx dockItem)
                        {
                            if (Equals(dockItem.Content, contentControl.Content))
                            {
                                this.ItemsSource.Remove(dockItem);
                                break;
                            }
                        }
                    }
            }

            if (this.ItemsSource.Count == 0)
            {
                this.ActiveDocument = null;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Docking manager document/tab state changed.
        /// </summary>
        /// <remarks>
        /// sushi.at, 23/06/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Dock state event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void PART_DockingManager_OnDockStateChanged(FrameworkElement sender, DockStateEventArgs e)
        {
            if (sender is ContentControl contentControl)
            {
                foreach (var item in this.ItemsSource)
                {
                    if (item is DockItemEx dockItem)
                    {
                        if (Equals(dockItem.Content, contentControl.Content))
                        {
                            dockItem.State = e.NewState;
                            break;
                        }
                    }
                }
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Docking manager window (pulled out tab) closing.
        /// </summary>
        /// <remarks>
        /// sushi.at, 22/06/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Window closing event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void PART_DockingManager_OnWindowClosing(object sender, WindowClosingEventArgs e)
        {
            if (e.TargetItem is ContentControl contentControl)
            {
                foreach (var item in this.ItemsSource)
                {
                    if (item is DockItemEx dockItem)
                    {
                        if (Equals(dockItem.Content, contentControl.Content))
                        {
                            this.ItemsSource.Remove(dockItem);
                            break;
                        }
                    }
                }
            }
        }
    }
}
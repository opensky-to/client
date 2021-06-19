// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainViewModel.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Views.Models
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;

    using Dragablz;
    using Dragablz.Dockablz;

    using ModernWpf.Controls;
    using ModernWpf.Controls.Primitives;

    using OpenSky.Client.Controls;
    using OpenSky.Client.Converters;
    using OpenSky.Client.MVVM;
    using OpenSky.Client.Pages;
    using OpenSky.Client.Tools;

    using Layout = Dragablz.Dockablz.Layout;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Main window view model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 15/06/2021.
    /// </remarks>
    /// <seealso cref="T:OpenSky.Client.MVVM.ViewModel"/>
    /// -------------------------------------------------------------------------------------------------
    public class MainViewModel : ViewModel
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The selected navigation menu item.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private NavMenuItem selectedNavMenuItem;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The selected page.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private TabItemEx selectedPage;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="MainViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 17/06/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public MainViewModel()
        {
            this.InterTabClient = new OpenSkyInterTabClient(this);

            var welcome = new NavMenuItem { Name = "Welcome", Icon = "/Resources/OpenSkyLogo16.png", PageType = typeof(Welcome) };
            this.NavigationItems.Add(welcome);

            var tools = new NavMenuItem { Name = "Tools", Icon = "/Resources/tools16.png" };
            var dataImport = new NavMenuItem { Name = "Data import", Icon = "/Resources/dataimport16.png", PageType = typeof(DataImport) };
            var airportManager = new NavMenuItem { Name = "Airport manager" };
            var aircraftManager = new NavMenuItem { Name = "Aircraft manager" };
            tools.Children = new ObservableCollection<NavMenuItem> { aircraftManager, airportManager, dataImport };
            this.NavigationItems.Add(tools);

            var settings = new NavMenuItem { Name = "Settings", Icon = "/Resources/settings16.png", PageType = typeof(Settings) };
            this.NavigationFooterItems.Add(settings);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="MainViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 17/06/2021.
        /// </remarks>
        /// <param name="items">
        /// The initial page tab items to add.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        //public MainViewModel(params TabItem[] items)
        //{
        //    this.PageItems = new ObservableCollection<TabItem>(items);
        //}

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the tab partition.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static Guid TabPartition => new("2AE89D18-F236-4D20-9605-6C03319038E6");

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the clone tab factory.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Func<TabItem> CloneTabFactory
        {
            get
            {
                return () =>
                {
                    // todo implement icons for both those cases
                    if (this.SelectedPage != null)
                    {
                        return new TabItemEx(this.SelectedPage.Header, Activator.CreateInstance(this.SelectedPage.Content.GetType()), false, this.SelectedPage.Icon);
                    }

                    return new TabItemEx("Welcome", new Welcome(), true, "/Resources/OpenSkyLogo16.png");
                };
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the closing floating item handler.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ClosingFloatingItemCallback ClosingFloatingItemHandler => ClosingFloatingItemHandlerImpl;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the closing tab item handler.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ItemActionCallback ClosingTabItemHandler => ClosingTabItemHandlerImpl;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the inter tab client.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public IInterTabClient InterTabClient { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the navigation footer items.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<NavMenuItem> NavigationFooterItems { get; } = new();

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the navigation items.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<NavMenuItem> NavigationItems { get; } = new();

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the page items.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<TabItemEx> PageItems { get; } = new();

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the selected navigation menu item.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public NavMenuItem SelectedNavMenuItem
        {
            get => this.selectedNavMenuItem;

            set
            {
                if (Equals(this.selectedNavMenuItem, value))
                {
                    return;
                }

                var origValue = this.selectedNavMenuItem;
                this.selectedNavMenuItem = value;

                if (value.PageType != null)
                {
                    if (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.LeftShift))
                    {
                        this.SelectedPage.Header = value.Name;
                        this.SelectedPage.Content = Activator.CreateInstance(value.PageType);
                        this.SelectedPage.Icon = value.Icon;

                        // Force content refresh of selected tab by switching to another
                        // todo there has to be a better way to do this
                        this.SelectedPage.IsSelected = false;
                        var x = new TabItemEx(null, null, true);
                        this.PageItems.Add(x);
                        this.PageItems.Remove(x);
                        this.SelectedPage.IsSelected = true;
                    }
                    else
                    {
                        // Ctrl or shift was held down, so add a new tab at the end
                        var tabItem = new TabItemEx(value.Name, Activator.CreateInstance(value.PageType), false, value.Icon);
                        this.PageItems.Add(tabItem);

                        new Thread(
                            () =>
                            {
                                Thread.Sleep(100);
                                this.selectedNavMenuItem = origValue;
                                this.NotifyPropertyChanged();
                            }).Start();
                    }
                }
                else
                {
                    // This is a category item, move selected back to current tab
                    new Thread(
                        () =>
                        {
                            Thread.Sleep(100);
                            this.selectedNavMenuItem = origValue;
                            this.NotifyPropertyChanged();
                        }).Start();
                }

                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the selected page.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public TabItemEx SelectedPage
        {
            get => this.selectedPage;

            set
            {
                if (Equals(this.selectedPage, value))
                {
                    return;
                }

                this.selectedPage = value;
                this.NotifyPropertyChanged();

                if (value != null)
                {
                    this.SelectMatchingNavigationItem(this.NavigationItems, (string)value.Header);
                    this.SelectMatchingNavigationItem(this.NavigationFooterItems, (string)value.Header);
                }
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the tool items.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<TabItemEx> ToolItems { get; } = new();

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Shows the welcome page.
        /// </summary>
        /// <remarks>
        /// sushi.at, 17/06/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public void ShowWelcomePage()
        {
            this.SelectMatchingNavigationItem(this.NavigationItems, "Welcome");
            var tabItem = new TabItemEx("Welcome", new Welcome(), true, "/Resources/OpenSkyLogo16.png");
            this.PageItems.Add(tabItem);
        }

        /// <summary>
        /// Callback to handle floating toolbar/MDI window closing.
        /// </summary>        
        private static void ClosingFloatingItemHandlerImpl(ItemActionCallbackArgs<Layout> args)
        {
            //in here you can dispose stuff or cancel the close

            //here's your view model: 
            //var disposable = args.DragablzItem.DataContext as IDisposable;
            //if (disposable != null)
            //    disposable.Dispose();

            //here's how you can cancel stuff:
            //args.Cancel(); 
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Callback to handle tab closing.
        /// </summary>
        /// <remarks>
        /// sushi.at, 17/06/2021.
        /// </remarks>
        /// <param name="args">
        /// The arguments.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private static void ClosingTabItemHandlerImpl(ItemActionCallbackArgs<TabablzControl> args)
        {
            //in here you can dispose stuff or cancel the close

            //here's your view model:
            //var viewModel = args.DragablzItem.DataContext as HeaderedItemViewModel;
            //Debug.Assert(viewModel != null);

            //here's how you can cancel stuff:
            //args.Cancel(); 
        }

        private void SelectMatchingNavigationItem(IEnumerable<NavMenuItem> menuItems, string name)
        {
            foreach (var navigationItem in menuItems)
            {
                if (navigationItem.Name == name)
                {
                    // Don't call the property here to avoid creating a new tab
                    this.selectedNavMenuItem = navigationItem;
                    this.NotifyPropertyChanged(nameof(this.SelectedNavMenuItem));
                    return;
                }

                if (navigationItem.Children?.Count > 0)
                {
                    this.SelectMatchingNavigationItem(navigationItem.Children, name);
                }
            }
        }
    }
}
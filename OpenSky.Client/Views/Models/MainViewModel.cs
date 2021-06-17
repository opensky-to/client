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
    using System.Threading;
    using System.Windows.Input;

    using Dragablz;
    using Dragablz.Dockablz;

    using OpenSky.Client.MVVM;
    using OpenSky.Client.Pages;

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
                    }
                    else
                    {
                        // Ctrl or shift was held down, so add a new tab at the end
                        this.PageItems.Add(new HeaderedItemViewModel(value.Name, Activator.CreateInstance(value.PageType)));
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

        public Func<HeaderedItemViewModel> Factory
        {
            get
            {
                return () =>
                {
                    if (this.SelectedPage != null)
                    {
                        return new HeaderedItemViewModel(this.SelectedPage.Header, Activator.CreateInstance(this.SelectedPage.Content.GetType()));
                    }

                    return new HeaderedItemViewModel("Welcome", new Welcome(), true);
                };
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The selected page.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private HeaderedItemViewModel selectedPage;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the selected page.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public HeaderedItemViewModel SelectedPage
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

                if (navigationItem.Children.Count > 0)
                {
                    this.SelectMatchingNavigationItem(navigationItem.Children, name);
                }
            }
        }

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
            var welcome = new NavMenuItem { Name = "Welcome", Icon = "home", PageType = typeof(Welcome) };
            this.NavigationItems.Add(welcome);

            var tools = new NavMenuItem { Name = "Tools" };
            var dataImport = new NavMenuItem { Name = "Data import", PageType = typeof(DataImport) };
            var airportManager = new NavMenuItem { Name = "Airport manager" };
            var aircraftManager = new NavMenuItem { Name = "Aircraft manager" };
            tools.Children.Add(aircraftManager);
            tools.Children.Add(airportManager);
            tools.Children.Add(dataImport);
            this.NavigationItems.Add(tools);

            var settings = new NavMenuItem { Name = "Settings", Icon = "settings", PageType = typeof(Settings) };
            this.NavigationFooterItems.Add(settings);
        }

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
            this.PageItems.Add(new HeaderedItemViewModel("Welcome", new Welcome(), true));
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
        public MainViewModel(params HeaderedItemViewModel[] items)
        {
            this.PageItems = new ObservableCollection<HeaderedItemViewModel>(items);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the tab partition.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static Guid TabPartition => new("2AE89D18-F236-4D20-9605-6C03319038E6");

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
        public IInterTabClient InterTabClient { get; } = new OpenSkyInterTabClient();

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the page items.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<HeaderedItemViewModel> PageItems { get; } = new();

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the tool items.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<HeaderedItemViewModel> ToolItems { get; } = new();

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

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the navigation items.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<NavMenuItem> NavigationItems { get; } = new();

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the navigation footer items.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<NavMenuItem> NavigationFooterItems { get; } = new();
    }
}
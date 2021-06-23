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
    using System.Windows;
    using System.Windows.Input;

    using OpenSky.Client.Controls;
    using OpenSky.Client.MVVM;
    using OpenSky.Client.Pages;

    using Syncfusion.Windows.Tools.Controls;

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
        /// The active document.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private DockItemEx activeDocument;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The last navigation item invoked Date/Time.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private DateTime navigationItemLastActivated = DateTime.MinValue;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The navigation item last activated lock.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly object navigationItemLastActivatedLock = new();

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The selected navigation menu item.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private NavMenuItem selectedNavMenuItem;

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
        /// Gets or sets the active document.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public DockItemEx ActiveDocument
        {
            get => this.activeDocument;

            set
            {
                if (Equals(this.activeDocument, value))
                {
                    return;
                }

                this.activeDocument = value;
                this.NotifyPropertyChanged();

                if (value != null)
                {
                    this.SelectMatchingNavigationItem(this.NavigationItems, value.Header);
                    this.SelectMatchingNavigationItem(this.NavigationFooterItems, value.Header);
                }
                else
                {
                    this.SelectedNavMenuItem = null;
                }
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the dock items.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<DockItemEx> DockItems { get; } = new();

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

                this.selectedNavMenuItem = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Navigation item was invoked.
        /// </summary>
        /// <remarks>
        /// sushi.at, 23/06/2021.
        /// </remarks>
        /// <param name="item">
        /// The item.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public void NavigationItemInvoked(NavMenuItem item)
        {
            if (item != null)
            {
                lock (this.navigationItemLastActivatedLock)
                {
                    if ((DateTime.Now - this.navigationItemLastActivated).TotalMilliseconds < 100)
                    {
                        // Don't double activate within 100ms
                        return;
                    }

                    this.navigationItemLastActivated = DateTime.Now;

                    var origNavItem = this.selectedNavMenuItem;
                    var origActiveWindow = this.ActiveDocument;

                    if (item.PageType != null)
                    {
                        var iconUri = !string.IsNullOrEmpty(item.Icon) ? $"pack://application:,,,/OpenSky.Client;component/{item.Icon}" : null;
                        if (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.LeftShift))
                        {
                            if (this.ActiveDocument is { State: DockState.Document })
                            {
                                this.ActiveDocument.Header = item.Name;
                                this.ActiveDocument.DocumentHeader = new DocumentHeaderEx(item.Name, iconUri);
                                this.ActiveDocument.Content = (FrameworkElement)Activator.CreateInstance(item.PageType);

                                this.SelectMatchingNavigationItem(this.NavigationItems, this.ActiveDocument.Header);
                                this.SelectMatchingNavigationItem(this.NavigationFooterItems, this.ActiveDocument.Header);
                            }
                            else
                            {
                                var dockItem = new DockItemEx
                                {
                                    Header = item.Name,
                                    DocumentHeader = new DocumentHeaderEx(item.Name, iconUri),
                                    State = DockState.Document,
                                    Content = (FrameworkElement)Activator.CreateInstance(item.PageType),

                                    //CanDock = false
                                };
                                this.DockItems.Add(dockItem);
                            }
                        }
                        else if (Keyboard.IsKeyDown(Key.LeftCtrl))
                        {
                            // Ctrl was held down, so add a new tab at the end
                            var dockItem = new DockItemEx
                            {
                                Header = item.Name,
                                DocumentHeader = new DocumentHeaderEx(item.Name, iconUri),
                                State = DockState.Document,
                                Content = (FrameworkElement)Activator.CreateInstance(item.PageType),

                                //CanDock = false
                            };
                            this.DockItems.Add(dockItem);
                            this.ActiveDocument = origActiveWindow;

                            new Thread(
                                () =>
                                {
                                    Thread.Sleep(100);
                                    this.selectedNavMenuItem = origNavItem;
                                    this.NotifyPropertyChanged();
                                }).Start();
                        }
                        else
                        {
                            // Shift was held down, create a new window and add tab to it
                            var dockItem = new DockItemEx
                            {
                                Header = item.Name,
                                DocumentHeader = new DocumentHeaderEx(item.Name, iconUri),
                                State = DockState.Document,
                                Content = (FrameworkElement)Activator.CreateInstance(item.PageType),

                                //CanDock = false
                            };
                            var newWindow = new Main();
                            newWindow.Show();
                            if (newWindow.DataContext is MainViewModel vm)
                            {
                                vm.DockItems.Add(dockItem);
                            }

                            if (this.ActiveDocument != null)
                            {
                                this.SelectMatchingNavigationItem(this.NavigationItems, this.ActiveDocument.Header);
                                this.SelectMatchingNavigationItem(this.NavigationFooterItems, this.ActiveDocument.Header);
                            }
                        }
                    }
                    else
                    {
                        // This is a category item, move selected back to current tab
                        new Thread(
                            () =>
                            {
                                Thread.Sleep(100);
                                this.selectedNavMenuItem = origNavItem;
                                this.NotifyPropertyChanged();
                            }).Start();
                    }
                }
            }
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
            var dockItem = new DockItemEx
            {
                Header = "Welcome",
                DocumentHeader = new DocumentHeaderEx("Welcome", "pack://application:,,,/OpenSky.Client;component/Resources/OpenSkyLogo16.png"),
                State = DockState.Document,
                Content = new Welcome(),

                //CanDock = false
            };

            this.DockItems.Add(dockItem);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Select matching navigation item.
        /// </summary>
        /// <remarks>
        /// sushi.at, 23/06/2021.
        /// </remarks>
        /// <param name="menuItems">
        /// The menu items.
        /// </param>
        /// <param name="name">
        /// The name of the item to search for.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
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
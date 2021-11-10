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
    using OpenSky.Client.Tools;

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
            // Build main menu
            var welcome = new NavMenuItem { Name = "Welcome", Icon = "/Resources/OpenSkyLogo16.png", PageType = typeof(Welcome) };
            this.NavigationItems.Add(welcome);

            var plans = new NavMenuItem { Name = "Flight plans", Icon = "/Resources/plan16.png", PageType = typeof(FlightPlans) };
            this.NavigationItems.Add(plans);

            var myFleet = new NavMenuItem { Name = "My fleet", Icon = "/Resources/aircraft16.png", PageType = typeof(MyFleet) };
            this.NavigationItems.Add(myFleet);

            var aircraftMarket = new NavMenuItem { Name = "Aircraft market", Icon = "/Resources/aircraftmarket16.png", PageType = typeof(AircraftMarket) };
            this.NavigationItems.Add(aircraftMarket);

            var tools = new NavMenuItem { Name = "Tools", Icon = "/Resources/tools16.png", Children = new ObservableCollection<NavMenuItem>() };
            if (UserSessionService.Instance.IsAdmin)
            {
                var dataImport = new NavMenuItem { Name = "Data import", Icon = "/Resources/dataimport16.png", PageType = typeof(DataImport) };
                tools.Children.Add(dataImport);

                var worldPopulation = new NavMenuItem { Name = "World population", Icon = "/Resources/world16.png", PageType = typeof(WorldPopulation) };
                tools.Children.Add(worldPopulation);
            }

            if (UserSessionService.Instance.IsModerator)
            {
                // todo add moderator only navigation items, once we have some :)
            }

            if (tools.Children.Count > 0)
            {
                this.NavigationItems.Add(tools);
            }

            var settings = new NavMenuItem { Name = "Settings", Icon = "/Resources/settings16.png", PageType = typeof(Settings) };
            this.NavigationFooterItems.Add(settings);

            // Create commands
            this.OpenInNewTabCommand = new Command(this.OpenInNewTab);
            this.OpenInNewWindowCommand = new Command(this.OpenInNewWindow);
        }


        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the open in new tab command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command OpenInNewTabCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the open in new window command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command OpenInNewWindowCommand { get; }

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
        /// Open menu item in new tab.
        /// </summary>
        /// <remarks>
        /// sushi.at, 25/06/2021.
        /// </remarks>
        /// <param name="parameter">
        /// The command parameter.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void OpenInNewTab(object parameter)
        {
            if (parameter is NavMenuItem item)
            {
                this.NavigationItemInvoked(item, true, false);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Open menu item in new window.
        /// </summary>
        /// <remarks>
        /// sushi.at, 25/06/2021.
        /// </remarks>
        /// <param name="parameter">
        /// The command parameter.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void OpenInNewWindow(object parameter)
        {
            if (parameter is NavMenuItem item)
            {
                this.NavigationItemInvoked(item, false, true);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Navigation menu item invoked.
        /// </summary>
        /// <remarks>
        /// sushi.at, 25/06/2021.
        /// </remarks>
        /// <param name="item">
        /// The item.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public void NavigationItemInvoked(NavMenuItem item)
        {
            this.NavigationItemInvoked(item, Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl), Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift));
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
        /// <param name="ctrl">
        /// True if CTRL key was pressed.
        /// </param>
        /// <param name="shift">
        /// True if SHIFT key was pressed.
        /// </param>
        /// <param name="switchToNewTab">
        /// (Optional) True to switch to new tab.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public void NavigationItemInvoked(NavMenuItem item, bool ctrl, bool shift, bool switchToNewTab = false)
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
                        if (!ctrl && !shift)
                        {
                            if (this.ActiveDocument is { State: DockState.Document })
                            {
                                this.ActiveDocument.Header = item.Name;
                                this.ActiveDocument.DocumentHeader = new DocumentHeaderEx(item.Name, iconUri);
                                this.ActiveDocument.Content = (FrameworkElement)Activator.CreateInstance(item.PageType);
                                if (this.ActiveDocument.Content is OpenSkyPage page)
                                {
                                    page.DockItem = this.ActiveDocument;
                                    if (item.Parameter != null)
                                    {
                                        page.PassPageParameter(item.Parameter);
                                    }
                                }

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
                                if (dockItem.Content is OpenSkyPage page)
                                {
                                    page.DockItem = dockItem;
                                    page.DockItem = this.ActiveDocument;
                                    if (item.Parameter != null)
                                    {
                                        page.PassPageParameter(item.Parameter);
                                    }
                                }
                            }
                        }
                        else if (ctrl)
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
                            if (dockItem.Content is OpenSkyPage page)
                            {
                                page.DockItem = dockItem;
                                page.DockItem = this.ActiveDocument;
                                if (item.Parameter != null)
                                {
                                    page.PassPageParameter(item.Parameter);
                                }
                            }

                            if (!switchToNewTab)
                            {
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
                                this.ActiveDocument = dockItem;
                            }
                        }
                        else
                        {
                            // Shift was held down, create a new window and add tab to it, pass this to the dispatcher as this operation takes a bit and would otherwise exceed 100ms
                            UpdateGUIDelegate createNewWindow = () =>
                            {
                                var dockItem = new DockItemEx
                                {
                                    Header = item.Name,
                                    DocumentHeader = new DocumentHeaderEx(item.Name, iconUri),
                                    State = DockState.Document,
                                    Content = (FrameworkElement)Activator.CreateInstance(item.PageType),

                                    //CanDock = false
                                };
                                if (dockItem.Content is OpenSkyPage page)
                                {
                                    page.DockItem = dockItem;
                                    page.DockItem = this.ActiveDocument;
                                    if (item.Parameter != null)
                                    {
                                        page.PassPageParameter(item.Parameter);
                                    }
                                }

                                var newWindow = new Main();
                                if (newWindow.DataContext is MainViewModel vm)
                                {
                                    vm.DockItems.Add(dockItem);
                                }

                                newWindow.Show();

                                if (this.ActiveDocument != null)
                                {
                                    this.SelectMatchingNavigationItem(this.NavigationItems, this.ActiveDocument.Header);
                                    this.SelectMatchingNavigationItem(this.NavigationFooterItems, this.ActiveDocument.Header);
                                }
                            };
                            Application.Current.Dispatcher.BeginInvoke(createNewWindow);
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
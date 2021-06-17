// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainViewModel.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Views.Models
{
    using System;
    using System.Collections.ObjectModel;

    using Dragablz;
    using Dragablz.Dockablz;

    using OpenSky.Client.MVVM;

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
        /// Initializes a new instance of the <see cref="MainViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 17/06/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public MainViewModel()
        {
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
    }
}
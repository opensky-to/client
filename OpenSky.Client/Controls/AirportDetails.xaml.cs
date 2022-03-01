// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AirportDetails.xaml.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Controls
{
    using System.ComponentModel;
    using System.Threading;
    using System.Windows;

    using OpenSky.Client.Controls.Models;
    using OpenSky.Client.Tools;

    using OpenSkyApi;

    /// -------------------------------------------------------------------------------------------------
    /// <content>
    /// Airport details user control.
    /// </content>
    /// -------------------------------------------------------------------------------------------------
    public partial class AirportDetails
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// (Immutable) The airport icao property.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static readonly DependencyProperty AirportICAOProperty = DependencyProperty.Register("AirportICAO", typeof(string), typeof(AirportDetails));

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// (Immutable) The airport property.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static readonly DependencyProperty AirportProperty = DependencyProperty.Register("Airport", typeof(Airport), typeof(AirportDetails));

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="AirportDetails"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 01/03/2022.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public AirportDetails()
        {
            this.InitializeComponent();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the airport.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Bindable(true)]
        public Airport Airport
        {
            get => (Airport)this.GetValue(AirportProperty);
            set => this.SetValue(AirportProperty, value);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the airport icao.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Bindable(true)]
        public string AirportICAO
        {
            get => (string)this.GetValue(AirportICAOProperty);
            set => this.SetValue(AirportICAOProperty, value);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether the detailed runway information is shown.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool ShowDetailedRunwayInformation { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Invoked whenever the effective value of any dependency property on this
        /// <see cref="T:System.Windows.FrameworkElement" /> has been updated. The specific dependency
        /// property that changed is reported in the arguments parameter. Overrides
        /// <see cref="M:System.Windows.DependencyObject.OnPropertyChanged(System.Windows.DependencyPropertyChangedEventArgs)" />.
        /// 
        /// </summary>
        /// <remarks>
        /// sushi.at, 01/03/2022.
        /// </remarks>
        /// <param name="e">
        /// The event data that describes the property that changed, as well as old and new values.
        /// </param>
        /// <seealso cref="System.Windows.FrameworkElement.OnPropertyChanged(DependencyPropertyChangedEventArgs)"/>
        /// -------------------------------------------------------------------------------------------------
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (this.DataContext is AirportDetailsViewModel viewModel)
            {
                if (e.Property == AirportProperty)
                {
                    viewModel.Airport = e.NewValue as Airport;
                }

                if (e.Property == AirportICAOProperty)
                {
                    if (string.IsNullOrEmpty(e.NewValue as string))
                    {
                        viewModel.Airport = null;
                        return;
                    }

                    if (!viewModel.LoadAirportCommand.IsExecuting)
                    {
                        viewModel.LoadAirportCommand.DoExecute(e.NewValue as string);
                    }
                    else
                    {
                        new Thread(
                                () =>
                                {
                                    while (viewModel.LoadAirportCommand.IsExecuting)
                                    {
                                        Thread.Sleep(100);
                                    }

                                    UpdateGUIDelegate loadAirport = () => viewModel.LoadAirportCommand.DoExecute(e.NewValue as string);
                                    Application.Current.Dispatcher.BeginInvoke(loadAirport);
                                })
                            { Name = "AirportDetails.WaitForLoadAirportCommand" }.Start();
                    }
                }
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Airport details on loaded.
        /// </summary>
        /// <remarks>
        /// sushi.at, 01/03/2022.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Routed event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void AirportDetailsOnLoaded(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is AirportDetailsViewModel viewModel)
            {
                viewModel.ViewReference = this;
            }
        }
    }
}
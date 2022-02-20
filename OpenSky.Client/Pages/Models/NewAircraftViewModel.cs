// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NewAircraftViewModel.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Pages.Models
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Windows;

    using OpenSky.Client.Controls;
    using OpenSky.Client.Controls.Models;
    using OpenSky.Client.MVVM;
    using OpenSky.Client.Tools;
    using OpenSky.Client.Views;

    using OpenSkyApi;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// New aircraft view model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 14/02/2022.
    /// </remarks>
    /// <seealso cref="OpenSky.Client.MVVM.ViewModel"/>
    /// -------------------------------------------------------------------------------------------------
    public class NewAircraftViewModel : ViewModel
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The account balances.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private AccountBalances accountBalances;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The aircraft type search string.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string aircraftTypeSearch;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// List of all aircraft types received from the API, with no filters applied.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private ICollection<AircraftType> allAircraftTypes;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The delivery cost per aircraft.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private int deliveryCostPerAircraft;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The factory ferry airport icao.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string factoryFerryAirportICAO;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The grand total.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private int grandTotal;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The loading text.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string loadingText;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// True if manufacturer ferry is checked.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private bool manufacturerFerryChecked;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The manufacturer ferry cost per nautical mile.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private int manufacturerFerryCostPerNm;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// True if manufacturer home delivery is checked.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private bool manufacturerHomeChecked = true;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The number of aircraft to purchase.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private int numberOfAircraft = 1;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// True if outsource ferry is checked.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private bool outsourceFerryChecked;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The selected aircraft type.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private AircraftType selectedAircraftType;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The signature.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string signature;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The sold stamp visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Visibility soldStampVisibility = Visibility.Collapsed;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The volume discount (per aircraft).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private int volumeDiscount;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="NewAircraftViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 14/02/2022.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public NewAircraftViewModel()
        {
            // Initialize data structures
            this.AircraftTypes = new ObservableCollection<AircraftType>();

            // Add initial values

            // Create commands
            this.RefreshBalancesCommand = new AsynchronousCommand(this.RefreshBalances);
            this.RefreshTypesCommand = new AsynchronousCommand(this.RefreshTypes);
            this.SignPurchaseCommand = new AsynchronousCommand(this.SignPurchase);

            // Fire off initial commands
            this.RefreshTypesCommand.DoExecute(null);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the account balances.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AccountBalances AccountBalances
        {
            get => this.accountBalances;

            set
            {
                if (Equals(this.accountBalances, value))
                {
                    return;
                }

                this.accountBalances = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets a list of types of aircraft.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<AircraftType> AircraftTypes { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the aircraft type search string.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string AircraftTypeSearch
        {
            get => this.aircraftTypeSearch;

            set
            {
                if (Equals(this.aircraftTypeSearch, value))
                {
                    return;
                }

                this.aircraftTypeSearch = value;
                this.NotifyPropertyChanged();

                this.AircraftTypes.Clear();
                foreach (var aircraftType in this.allAircraftTypes.OrderBy(t => t.Name))
                {
                    if (string.IsNullOrEmpty(this.AircraftTypeSearch) || aircraftType.Name.ToLowerInvariant().Contains(this.AircraftTypeSearch.ToLowerInvariant()) ||
                        aircraftType.Manufacturer.Name.ToLowerInvariant().Contains(this.AircraftTypeSearch.ToLowerInvariant()))
                    {
                        this.AircraftTypes.Add(aircraftType);
                    }
                }

                if (string.IsNullOrEmpty(value))
                {
                    this.SelectedAircraftType = null;
                }
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the name of the buyer.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string BuyerName => UserSessionService.Instance.Username;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the delivery cost per aircraft.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int DeliveryCostPerAircraft
        {
            get => this.deliveryCostPerAircraft;
            set
            {
                if (value == this.deliveryCostPerAircraft)
                {
                    return;
                }

                this.deliveryCostPerAircraft = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the factory ferry airport icao.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string FactoryFerryAirportICAO
        {
            get => this.factoryFerryAirportICAO;
            set
            {
                if (Equals(this.factoryFerryAirportICAO, value))
                {
                    return;
                }

                this.factoryFerryAirportICAO = value;
                this.NotifyPropertyChanged();
                this.CalculateGrandTotal();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the grand total.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int GrandTotal
        {
            get => this.grandTotal;
            set
            {
                if (value == this.grandTotal)
                {
                    return;
                }

                this.grandTotal = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the loading text.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string LoadingText
        {
            get => this.loadingText;

            set
            {
                if (Equals(this.loadingText, value))
                {
                    return;
                }

                this.loadingText = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether the manufacturer ferry option is checked.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool ManufacturerFerryChecked
        {
            get => this.manufacturerFerryChecked;
            set
            {
                if (value == this.manufacturerFerryChecked)
                {
                    return;
                }

                this.manufacturerFerryChecked = value;
                this.NotifyPropertyChanged();
                this.CalculateGrandTotal();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the manufacturer ferry cost per nautical mile.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int ManufacturerFerryCostPerNm
        {
            get => this.manufacturerFerryCostPerNm;
            set
            {
                if (value == this.manufacturerFerryCostPerNm)
                {
                    return;
                }

                this.manufacturerFerryCostPerNm = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether the manufacturer home option is checked.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool ManufacturerHomeChecked
        {
            get => this.manufacturerHomeChecked;
            set
            {
                if (value == this.manufacturerHomeChecked)
                {
                    return;
                }

                this.manufacturerHomeChecked = value;
                this.NotifyPropertyChanged();
                this.CalculateGrandTotal();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the number of aircraft to purchase.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int NumberOfAircraft
        {
            get => this.numberOfAircraft;
            set
            {
                if (value == this.numberOfAircraft)
                {
                    return;
                }

                this.numberOfAircraft = value;
                this.NotifyPropertyChanged();
                this.CalculateGrandTotal();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether the outsource ferry option is checked.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool OutsourceFerryChecked
        {
            get => this.outsourceFerryChecked;
            set
            {
                if (value == this.outsourceFerryChecked)
                    return;
                this.outsourceFerryChecked = value;
                this.NotifyPropertyChanged();
                this.CalculateGrandTotal();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the refresh balances command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand RefreshBalancesCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the refresh types command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand RefreshTypesCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the sales agreement information text.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string SalesAgreementInfoText => $"This Sales Agreement (this \"Agreement\") is entered into as of the {DateTime.Today.Day.Ordinal()} day of {DateTime.Today:MMMM} {DateTime.Today.Year}, by and among/between:";

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the selected aircraft type.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AircraftType SelectedAircraftType
        {
            get => this.selectedAircraftType;

            set
            {
                if (Equals(this.selectedAircraftType, value))
                {
                    return;
                }

                this.selectedAircraftType = value;
                this.NotifyPropertyChanged();
                if (value != null)
                {
                    this.CalculateGrandTotal();
                }
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the signature.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string Signature
        {
            get => this.signature;

            set
            {
                if (Equals(this.signature, value))
                {
                    return;
                }

                this.signature = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the sign purchase command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand SignPurchaseCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the sold stamp visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Visibility SoldStampVisibility
        {
            get => this.soldStampVisibility;

            set
            {
                if (Equals(this.soldStampVisibility, value))
                {
                    return;
                }

                this.soldStampVisibility = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the volume discount.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int VolumeDiscount
        {
            get => this.volumeDiscount;
            set
            {
                if (value == this.volumeDiscount)
                    return;
                this.volumeDiscount = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Calculates the grand total (and necessary steps in between).
        /// </summary>
        /// <remarks>
        /// sushi.at, 18/02/2022.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void CalculateGrandTotal()
        {
            this.ManufacturerFerryCostPerNm = this.SelectedAircraftType?.Category switch
            {
                AircraftTypeCategory.SEP => 15,
                AircraftTypeCategory.MEP => 25,
                AircraftTypeCategory.SET => 30,
                AircraftTypeCategory.MET => 50,
                AircraftTypeCategory.JET => 100,
                AircraftTypeCategory.HEL => 100,
                AircraftTypeCategory.REG => 150,
                AircraftTypeCategory.NBA => 250,
                AircraftTypeCategory.WBA => 400,
                _ => 0
            };

            if (this.ManufacturerHomeChecked)
            {
                this.DeliveryCostPerAircraft = 0;
            }

            if (this.ManufacturerFerryChecked)
            {
                this.DeliveryCostPerAircraft = this.ManufacturerFerryCostPerNm; // todo work out distance and multiply
            }

            if (this.OutsourceFerryChecked)
            {
                // todo implement once ferry job type exists
            }

            if (this.SelectedAircraftType != null && this.NumberOfAircraft >= 1)
            {
                var discount = this.NumberOfAircraft switch
                {
                    >= 50 => 0.25,
                    >= 10 => 0.1,
                    >= 3 => 0.05,
                    _ => 0
                };

                this.VolumeDiscount = (int)(this.SelectedAircraftType.MaxPrice * discount);
                this.GrandTotal = (this.SelectedAircraftType.MaxPrice - this.VolumeDiscount + this.DeliveryCostPerAircraft) * this.NumberOfAircraft;
            }
            else
            {
                this.VolumeDiscount = 0;
                this.GrandTotal = 0;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Refresh account balances.
        /// </summary>
        /// <remarks>
        /// sushi.at, 26/01/2022.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void RefreshBalances()
        {
            this.LoadingText = "Refreshing account balances...";
            try
            {
                var result = OpenSkyService.Instance.GetAccountBalancesAsync().Result;
                if (!result.IsError)
                {
                    this.AccountBalances = result.Data;
                }
                else
                {
                    this.RefreshBalancesCommand.ReportProgress(
                        () =>
                        {
                            Debug.WriteLine("Error refreshing account balances: " + result.Message);
                            if (!string.IsNullOrEmpty(result.ErrorDetails))
                            {
                                Debug.WriteLine(result.ErrorDetails);
                            }

                            var notification = new OpenSkyNotification("Error refreshing account balances", result.Message, MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                            notification.SetErrorColorStyle();
                            Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.ViewReference, this.RefreshBalancesCommand, "Error refreshing account balances");
            }
            finally
            {
                this.LoadingText = null;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Refresh aircraft types.
        /// </summary>
        /// <remarks>
        /// sushi.at, 21/07/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void RefreshTypes()
        {
            this.LoadingText = "Refreshing aircraft types...";
            try
            {
                var result = OpenSkyService.Instance.GetAircraftTypesAsync().Result;
                if (!result.IsError)
                {
                    this.RefreshTypesCommand.ReportProgress(
                        () =>
                        {
                            this.allAircraftTypes = result.Data;

                            this.AircraftTypes.Clear();
                            foreach (var aircraftType in this.allAircraftTypes.OrderBy(t => t.Name))
                            {
                                if (string.IsNullOrEmpty(this.AircraftTypeSearch) || aircraftType.Name.Contains(this.AircraftTypeSearch) || aircraftType.Manufacturer.Name.Contains(this.AircraftTypeSearch))
                                {
                                    this.AircraftTypes.Add(aircraftType);
                                }
                            }
                        });
                }
                else
                {
                    this.RefreshTypesCommand.ReportProgress(
                        () =>
                        {
                            Debug.WriteLine("Error refreshing aircraft types: " + result.Message);
                            if (!string.IsNullOrEmpty(result.ErrorDetails))
                            {
                                Debug.WriteLine(result.ErrorDetails);
                            }

                            var notification = new OpenSkyNotification("Error refreshing aircraft types", result.Message, MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                            notification.SetErrorColorStyle();
                            Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.ViewReference, this.RefreshTypesCommand, "Error refreshing aircraft types");
            }
            finally
            {
                this.LoadingText = null;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Sign the purchase agreement.
        /// </summary>
        /// <remarks>
        /// sushi.at, 18/02/2022.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void SignPurchase()
        {
            // TODO
        }
    }
}
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
    using System.Device.Location;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Windows;

    using OpenSky.Client.Controls;
    using OpenSky.Client.Controls.Models;
    using OpenSky.Client.MVVM;
    using OpenSky.Client.OpenAPIs.ModelExtensions;
    using OpenSky.Client.Tools;
    using OpenSky.Client.Views;

    using OpenSkyApi;

    using TomsToolbox.Essentials;

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
        /// The country search string.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string countrySearch;

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
        /// The registration country.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Country? registrationCountry;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The selected aircraft type.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private AircraftType selectedAircraftType;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The selected delivery location.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private AircraftManufacturerDeliveryLocation selectedDeliveryLocation;

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
            this.Countries = new ObservableCollection<CountryComboItem>();
            this.FactoryFerryAirports = new ObservableCollection<string>();

            // Add initial values
            foreach (var countryItem in CountryComboItem.GetCountryComboItems())
            {
                this.Countries.Add(countryItem);
            }

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
        /// Gets the countries.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<CountryComboItem> Countries { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the country search string.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string CountrySearch
        {
            get => this.countrySearch;

            set
            {
                if (Equals(this.countrySearch, value))
                {
                    return;
                }

                this.countrySearch = value;
                this.NotifyPropertyChanged();

                this.Countries.Clear();
                foreach (var country in CountryComboItem.GetCountryComboItems())
                {
                    if (string.IsNullOrEmpty(this.CountrySearch) || country.ToString().ToLowerInvariant().Contains(this.CountrySearch))
                    {
                        this.Countries.Add(country);
                    }
                }
            }
        }

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

                if (!string.IsNullOrEmpty(value))
                {
                    this.ManufacturerFerryChecked = true;

                    // Search for matching airports
                    this.FactoryFerryAirports.Clear();
                    var airportPackage = AirportPackageClientHandler.GetPackage();
                    if (airportPackage != null)
                    {
                        this.FactoryFerryAirports.AddRange(
                            airportPackage.Airports
                                          .Where(
                                              a => a.ICAO.ToLowerInvariant().Contains(value.ToLowerInvariant()) || a.Name.ToLowerInvariant().Contains(value.ToLowerInvariant()) ||
                                                   (a.City != null && a.City.ToLowerInvariant().Contains(value.ToLowerInvariant()))).Select(a => $"{a.ICAO}: {a.Name}{(string.IsNullOrWhiteSpace(a.City) ? string.Empty : $" / {a.City}")}"));
                    }
                }
                else
                {
                    // Restore full list of airports
                    this.FactoryFerryAirports.Clear();
                    var airportPackage = AirportPackageClientHandler.GetPackage();
                    if (airportPackage != null)
                    {
                        this.FactoryFerryAirports.AddRange(airportPackage.Airports.Select(a => $"{a.ICAO}: {a.Name}{(string.IsNullOrWhiteSpace(a.City) ? string.Empty : $" / {a.City}")}"));
                    }
                }

                this.CalculateGrandTotal();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the factory ferry airports.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<string> FactoryFerryAirports { get; }

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
        /// Gets or sets the registration country.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Country? RegistrationCountry
        {
            get => this.registrationCountry;

            set
            {
                if (Equals(this.registrationCountry, value))
                {
                    return;
                }

                this.registrationCountry = value;
                this.NotifyPropertyChanged();
            }
        }

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
                    this.SelectedDeliveryLocation = value.DeliveryLocations.FirstOrDefault();
                }
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the selected delivery location.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AircraftManufacturerDeliveryLocation SelectedDeliveryLocation
        {
            get => this.selectedDeliveryLocation;

            set
            {
                if (Equals(this.selectedDeliveryLocation, value))
                {
                    return;
                }

                this.selectedDeliveryLocation = value;
                this.NotifyPropertyChanged();
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
                this.DeliveryCostPerAircraft = 0;
                if (this.SelectedDeliveryLocation != null && !string.IsNullOrEmpty(this.FactoryFerryAirportICAO))
                {
                    var airportCache = AirportPackageClientHandler.GetPackage();
                    if (airportCache != null)
                    {
                        var sourceAirport = airportCache.Airports.SingleOrDefault(a => a.ICAO == this.SelectedDeliveryLocation.AirportICAO);
                        var destinationAirport = airportCache.Airports.SingleOrDefault(a => a.ICAO == this.FactoryFerryAirportICAO.Split(':')[0]);

                        if (sourceAirport != null && destinationAirport != null)
                        {
                            var distance = new GeoCoordinate(sourceAirport.Latitude, sourceAirport.Longitude).GetDistanceTo(new GeoCoordinate(destinationAirport.Latitude, destinationAirport.Longitude)) / 1852.0;

                            this.DeliveryCostPerAircraft = (int)(this.ManufacturerFerryCostPerNm * distance);
                        }
                    }
                }
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
                            this.allAircraftTypes = result.Data.Where(t => !t.IsHistoric).ToList();

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
            var errorMessages = string.Empty;
            if (this.SelectedAircraftType == null)
            {
                errorMessages += "No aircraft type selected!\r\n";
            }

            if (this.SelectedDeliveryLocation == null)
            {
                errorMessages += "No delivery location selected/available!\r\n";
            }

            if (this.NumberOfAircraft < 1)
            {
                errorMessages += "Invalid number of aircraft!\r\n";
            }

            if (this.RegistrationCountry == null)
            {
                errorMessages += "Country of registration not selected!\r\n";
            }

            if (this.ManufacturerFerryChecked && string.IsNullOrEmpty(this.FactoryFerryAirportICAO))
            {
                errorMessages += "Ferry flight destination airport not selected!\r\n";
            }

            if (!string.IsNullOrEmpty(errorMessages))
            {
                this.SignPurchaseCommand.ReportProgress(
                    () =>
                    {
                        var notification = new OpenSkyNotification("Error purchasing aircraft", errorMessages.TrimEnd('\r', '\n'), MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                        notification.SetErrorColorStyle();
                        Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                    });
                return;
            }

            this.SignPurchaseCommand.CanExecute = false;

            foreach (var car in UserSessionService.Instance.Username.ToCharArray())
            {
                this.Signature += car;
                Thread.Sleep(300);
            }

            try
            {
                var purchase = new PurchaseNewAircraft
                {
                    TypeID = this.SelectedAircraftType?.Id ?? Guid.Empty,
                    NumberOfAircraft = this.NumberOfAircraft,
                    DeliveryAirportICAO = this.SelectedDeliveryLocation?.AirportICAO,
                    Country = this.RegistrationCountry ?? Country.AT,
                    ForAirline = false // todo enable once we add airlines for real
                };
                if (this.ManufacturerHomeChecked)
                {
                    purchase.DeliveryOption = NewAircraftDeliveryOption.ManufacturerDeliveryAirport;
                }
                else if (this.ManufacturerFerryChecked)
                {
                    purchase.DeliveryOption = NewAircraftDeliveryOption.ManufacturerFerry;
                    purchase.FerryAirportICAO = this.FactoryFerryAirportICAO.Split(':')[0];
                }
                else if (this.OutsourceFerryChecked)
                {
                    purchase.DeliveryOption = NewAircraftDeliveryOption.OutsourceFerry;
                }
                else
                {
                    purchase.DeliveryOption = NewAircraftDeliveryOption.ManufacturerDeliveryAirport;
                }

                var result = OpenSkyService.Instance.PurchaseNewAircraftAsync(purchase).Result;
                if (!result.IsError)
                {
                    this.SoldStampVisibility = Visibility.Visible;

                    Thread.Sleep(1500);
                    this.SignPurchaseCommand.ReportProgress(
                        () =>
                        {
                            var messageBox = new OpenSkyMessageBox("Purchase aircraft", result.Message, MessageBoxButton.OK, ExtendedMessageBoxImage.Check, 30);
                            Main.ShowMessageBoxInSaveViewAs(this.ViewReference, messageBox);
                        });

                    return;
                }

                this.SignPurchaseCommand.ReportProgress(
                    () =>
                    {
                        Debug.WriteLine("Error purchasing aircraft: " + result.Message);
                        if (!string.IsNullOrEmpty(result.ErrorDetails))
                        {
                            Debug.WriteLine(result.ErrorDetails);
                        }

                        var notification = new OpenSkyNotification("Error purchasing aircraft", result.Message, MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                        notification.SetErrorColorStyle();
                        Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                    });
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.ViewReference, this.SignPurchaseCommand, "Error purchasing aircraft");
            }

            this.Signature = string.Empty;
            this.SignPurchaseCommand.CanExecute = true;
        }
    }
}
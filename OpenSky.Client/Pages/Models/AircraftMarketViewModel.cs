// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AircraftMarketViewModel.cs" company="OpenSky">
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
    /// Aircraft market view model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 19/07/2021.
    /// </remarks>
    /// <seealso cref="T:OpenSky.Client.MVVM.ViewModel"/>
    /// -------------------------------------------------------------------------------------------------
    public class AircraftMarketViewModel : ViewModel
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The account balances.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private AccountBalances accountBalances;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// True if "aircraft category" is checked.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private bool aircraftCategoryChecked;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// True if "aircraft manufacturer" is checked.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private bool aircraftManufacturerChecked;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// True if "aircraft name" is checked.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private bool aircraftNameChecked;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The aircraft to purchase.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Aircraft aircraftToPurchase;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The selected aircraft type category, or NULL for all.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private AircraftTypeCategoryComboItem aircraftTypeCategory;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The current airport ICAO code.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string airportIcao;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// List of all aircraft types received from the API, with no filters applied.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private ICollection<AircraftType> allAircraftTypes;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// True if "all aircraft types" is checked.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private bool allAircraftTypesChecked = true;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// True if "at airport" is checked.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private bool atAirportChecked = true;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The available aircraft groupbox header.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string availableAircraftHeader = "Available Aircraft";

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The currently selected country, or NULL if no country is selected.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string country;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// True if "country" is checked.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private bool countryChecked;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The loading text.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string loadingText;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The location column visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Visibility locationColumnVisibility = Visibility.Collapsed;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The current manufacturer search/filter string.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string manufacturer;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The current aircraft name search/filter string.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string name;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The NM search radius around the selected airport.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private int nmRadius = 100;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// True if "only vanilla" is checked.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private bool onlyVanillaChecked;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The purchase aircraft variants visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Visibility purchaseAircraftVariantsVisibility = Visibility.Collapsed;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The purchase aircraft visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Visibility purchaseAircraftVisibility = Visibility.Collapsed;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// True if "purchase and rent" checked.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private bool purchaseAndRentChecked = true;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// True if "purchase" is checked.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private bool purchaseChecked;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// True if "rent" is checked.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private bool rentChecked;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The selected aircraft.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Aircraft selectedAircraft;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The selected purchase aircraft variant.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private AircraftType selectedPurchaseAircraftVariant;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The signature.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string signature;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The currently selected simulator, or NULL for all simulators.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Simulator? simulator;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The sold stamp visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Visibility soldStampVisibility = Visibility.Collapsed;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// True if "within nm of airport" is checked.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private bool withinNmAirportChecked;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="AircraftMarketViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 19/07/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public AircraftMarketViewModel()
        {
            // Initialize data structures
            this.TypeCategories = new ObservableCollection<AircraftTypeCategoryComboItem>();
            this.Manufacturers = new ObservableCollection<string>();
            this.AircraftTypes = new ObservableCollection<AircraftType>();
            this.Countries = new ObservableCollection<CountryComboItem>();
            this.Simulators = new ObservableCollection<Simulator>();
            this.Aircraft = new ObservableCollection<Aircraft>();
            this.Aircraft.CollectionChanged += (_, _) => { this.AvailableAircraftHeader = $"Available Aircraft ({this.Aircraft.Count})"; };
            this.PurchaseAircraftVariants = new ObservableCollection<AircraftType>();

            // Add initial values
            foreach (var categoryItem in AircraftTypeCategoryComboItem.GetAircraftTypeCategoryComboItems())
            {
                this.TypeCategories.Add(categoryItem);
            }

            foreach (var countryItem in CountryComboItem.GetCountryComboItems())
            {
                this.Countries.Add(countryItem);
            }

            foreach (Simulator sim in Enum.GetValues(typeof(Simulator)))
            {
                this.Simulators.Add(sim);
            }

            if (Properties.Settings.Default.DefaultSimulator != -1)
            {
                this.Simulator = (Simulator)Properties.Settings.Default.DefaultSimulator;
            }

            // Create commands
            this.RefreshTypesCommand = new AsynchronousCommand(this.RefreshTypes);
            this.RefreshBalancesCommand = new AsynchronousCommand(this.RefreshBalances);
            this.ClearCategoryCommand = new Command(this.ClearCategory);
            this.ClearSimulatorCommand = new Command(this.ClearSimulator);
            this.ResetSearchCommand = new Command(this.ResetSearch);
            this.SearchCommand = new AsynchronousCommand(this.Search);
            this.PurchaseCommand = new Command(this.PurchaseAircraft);
            this.CancelPurchaseAircraftCommand = new Command(this.CancelPurchaseAircraft);
            this.GetPurchaseAircraftVariantsCommand = new AsynchronousCommand(this.GetPurchaseAircraftVariants);
            this.SignPurchaseCommand = new AsynchronousCommand(this.SignPurchase);

            // todo implement renting when available

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
        /// Gets the aircraft list.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<Aircraft> Aircraft { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether "aircraft category" is checked.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool AircraftCategoryChecked
        {
            get => this.aircraftCategoryChecked;

            set
            {
                if (Equals(this.aircraftCategoryChecked, value))
                {
                    return;
                }

                this.aircraftCategoryChecked = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether "aircraft manufacturer" is checked.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool AircraftManufacturerChecked
        {
            get => this.aircraftManufacturerChecked;

            set
            {
                if (Equals(this.aircraftManufacturerChecked, value))
                {
                    return;
                }

                this.aircraftManufacturerChecked = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether "aircraft name" is checked.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool AircraftNameChecked
        {
            get => this.aircraftNameChecked;

            set
            {
                if (Equals(this.aircraftNameChecked, value))
                {
                    return;
                }

                this.aircraftNameChecked = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the aircraft to purchase.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Aircraft AircraftToPurchase
        {
            get => this.aircraftToPurchase;

            set
            {
                if (Equals(this.aircraftToPurchase, value))
                {
                    return;
                }

                this.aircraftToPurchase = value;
                this.NotifyPropertyChanged();
                this.PurchaseAircraftVisibility = value != null ? Visibility.Visible : Visibility.Collapsed;
                this.SoldStampVisibility = Visibility.Collapsed;
                this.Signature = string.Empty;
                if (value != null)
                {
                    this.GetPurchaseAircraftVariantsCommand.DoExecute(null);
                }

                this.SignPurchaseCommand.CanExecute = value != null;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the selected aircraft type category, or NULL for all.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AircraftTypeCategoryComboItem AircraftTypeCategory
        {
            get => this.aircraftTypeCategory;

            set
            {
                if (Equals(this.aircraftTypeCategory, value))
                {
                    return;
                }

                this.aircraftTypeCategory = value;
                this.NotifyPropertyChanged();

                if (value != null)
                {
                    this.AircraftCategoryChecked = true;

                    // Filter manufacturers and types to the new category
                    this.Manufacturers.Clear();
                    foreach (var typeManufacturer in this.allAircraftTypes.Where(t => t.Category == value.AircraftTypeCategory).Select(t => t.Manufacturer.Name).Distinct().OrderBy(m => m))
                    {
                        this.Manufacturers.Add(typeManufacturer);
                    }

                    this.AircraftTypes.Clear();
                    foreach (var aircraftType in this.allAircraftTypes.Where(t => t.Category == value.AircraftTypeCategory).OrderBy(t => t.Name))
                    {
                        if (!this.Simulator.HasValue || aircraftType.Simulator == this.Simulator.Value)
                        {
                            this.AircraftTypes.Add(aircraftType);
                        }
                    }

                    // Check if the current manufacturer is still in the filtered list
                    if (!string.IsNullOrEmpty(this.Manufacturer) && !this.Manufacturers.Any(m => m.ToLowerInvariant().Contains(this.Manufacturer.ToLowerInvariant())))
                    {
                        this.Manufacturer = string.Empty;
                    }

                    // Check if the current name is still in the filtered list
                    if (!string.IsNullOrEmpty(this.Name) && !this.AircraftTypes.Any(t => t.Name.ToLowerInvariant().Contains(this.Name.ToLowerInvariant())))
                    {
                        this.Name = string.Empty;
                    }
                }
                else
                {
                    // Restore the full list of manufacturers and aircraft types
                    this.Manufacturers.Clear();
                    foreach (var typeManufacturer in this.allAircraftTypes.Select(t => t.Manufacturer.Name).Distinct().OrderBy(m => m))
                    {
                        this.Manufacturers.Add(typeManufacturer);
                    }

                    this.AircraftTypes.Clear();
                    foreach (var aircraftType in this.allAircraftTypes.OrderBy(t => t.Name))
                    {
                        if (!this.Simulator.HasValue || aircraftType.Simulator == this.Simulator.Value)
                        {
                            this.AircraftTypes.Add(aircraftType);
                        }
                    }
                }
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
        /// Gets or sets the current airport ICAO code.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string AirportIcao
        {
            get => this.airportIcao;

            set
            {
                if (Equals(this.airportIcao, value))
                {
                    return;
                }

                this.airportIcao = value;
                this.NotifyPropertyChanged();

                if (!string.IsNullOrEmpty(value))
                {
                    this.AtAirportChecked = true;
                }
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether "all aircraft types" is checked.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool AllAircraftTypesChecked
        {
            get => this.allAircraftTypesChecked;

            set
            {
                if (Equals(this.allAircraftTypesChecked, value))
                {
                    return;
                }

                this.allAircraftTypesChecked = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether "at airport" is checked.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool AtAirportChecked
        {
            get => this.atAirportChecked;

            set
            {
                if (Equals(this.atAirportChecked, value))
                {
                    return;
                }

                this.atAirportChecked = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the available aircraft groupbox header.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string AvailableAircraftHeader
        {
            get => this.availableAircraftHeader;

            set
            {
                if (Equals(this.availableAircraftHeader, value))
                {
                    return;
                }

                this.availableAircraftHeader = value;
                this.NotifyPropertyChanged();
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
        /// Gets the cancel purchase aircraft command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command CancelPurchaseAircraftCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the clear category command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command ClearCategoryCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the clear simulator command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command ClearSimulatorCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the countries.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<CountryComboItem> Countries { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the currently selected country, or NULL if no country is selected.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string Country
        {
            get => this.country;

            set
            {
                if (Equals(this.country, value))
                {
                    return;
                }

                this.country = value;
                this.NotifyPropertyChanged();

                if (!string.IsNullOrEmpty(value))
                {
                    // Filter countries list to match current country text
                    this.Countries.Clear();
                    foreach (var countryItem in CountryComboItem.GetCountryComboItems().Where(c => c.ToString().ToLowerInvariant().Contains(value.ToLowerInvariant())))
                    {
                        this.Countries.Add(countryItem);
                    }

                    this.CountryChecked = true;
                }
                else
                {
                    // Restore all countries
                    this.Countries.Clear();
                    foreach (var countryItem in CountryComboItem.GetCountryComboItems())
                    {
                        this.Countries.Add(countryItem);
                    }
                }
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether "country" is checked.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool CountryChecked
        {
            get => this.countryChecked;

            set
            {
                if (Equals(this.countryChecked, value))
                {
                    return;
                }

                this.countryChecked = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the get purchase aircraft variants command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand GetPurchaseAircraftVariantsCommand { get; }

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
        /// Gets or sets the location column visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Visibility LocationColumnVisibility
        {
            get => this.locationColumnVisibility;

            set
            {
                if (Equals(this.locationColumnVisibility, value))
                {
                    return;
                }

                this.locationColumnVisibility = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the current manufacturer search/filter string.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string Manufacturer
        {
            get => this.manufacturer;

            set
            {
                if (Equals(this.manufacturer, value))
                {
                    return;
                }

                this.manufacturer = value;
                this.NotifyPropertyChanged();

                if (!string.IsNullOrEmpty(value))
                {
                    this.AircraftManufacturerChecked = true;

                    // Filter list of manufacturers to match current matching text
                    this.Manufacturers.Clear();
                    foreach (var typeManufacturer in this.allAircraftTypes.Where(t => this.AircraftTypeCategory == null || t.Category == this.AircraftTypeCategory.AircraftTypeCategory).Select(t => t.Manufacturer.Name).Distinct()
                                                         .Where(m => m.ToLowerInvariant().Contains(value.ToLowerInvariant())).OrderBy(m => m))
                    {
                        this.Manufacturers.Add(typeManufacturer);
                    }

                    // Filter aircraft types to this manufacturer
                    this.AircraftTypes.Clear();
                    foreach (var aircraftType in this.allAircraftTypes.Where(t => t.Manufacturer.Name.ToLowerInvariant().Contains(value.ToLowerInvariant())).OrderBy(t => t.Name))
                    {
                        if (this.AircraftTypeCategory != null && this.AircraftTypeCategory.AircraftTypeCategory != aircraftType.Category)
                        {
                            continue;
                        }

                        if (!this.Simulator.HasValue || aircraftType.Simulator == this.Simulator.Value)
                        {
                            this.AircraftTypes.Add(aircraftType);
                        }
                    }

                    // Check if the current name is still in the filtered list
                    if (!string.IsNullOrEmpty(this.Name) && !this.AircraftTypes.Any(t => t.Name.ToLowerInvariant().Contains(this.Name.ToLowerInvariant())))
                    {
                        this.Name = string.Empty;
                    }
                }
                else
                {
                    // Restore full list of manufacturers (except where other filters already removed them)
                    this.Manufacturers.Clear();
                    foreach (var typeManufacturer in this.allAircraftTypes.Where(t => this.AircraftTypeCategory == null || t.Category == this.AircraftTypeCategory.AircraftTypeCategory).Select(t => t.Manufacturer.Name).Distinct().OrderBy(m => m))
                    {
                        this.Manufacturers.Add(typeManufacturer);
                    }

                    // Restore the full list of aircraft types (except where other filters already removed them)
                    this.AircraftTypes.Clear();
                    foreach (var aircraftType in this.allAircraftTypes.OrderBy(t => t.Name))
                    {
                        if (this.AircraftTypeCategory != null && this.AircraftTypeCategory.AircraftTypeCategory != aircraftType.Category)
                        {
                            continue;
                        }

                        if (!this.Simulator.HasValue || aircraftType.Simulator == this.Simulator.Value)
                        {
                            this.AircraftTypes.Add(aircraftType);
                        }
                    }
                }
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the manufacturers list.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<string> Manufacturers { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the current aircraft name search/filter string.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string Name
        {
            get => this.name;

            set
            {
                if (Equals(this.name, value))
                {
                    return;
                }

                this.name = value;
                this.NotifyPropertyChanged();

                if (!string.IsNullOrEmpty(value))
                {
                    this.AircraftNameChecked = true;

                    // Filter aircraft types to match the current name
                    this.AircraftTypes.Clear();
                    foreach (var aircraftType in this.allAircraftTypes.Where(t => t.Name.ToLowerInvariant().Contains(value.ToLowerInvariant())).OrderBy(t => t.Name))
                    {
                        if (this.AircraftTypeCategory != null && this.AircraftTypeCategory.AircraftTypeCategory != aircraftType.Category)
                        {
                            continue;
                        }

                        if (!string.IsNullOrEmpty(this.Manufacturer) && !aircraftType.Manufacturer.Name.ToLowerInvariant().Contains(this.Manufacturer.ToLowerInvariant()))
                        {
                            continue;
                        }

                        if (!this.Simulator.HasValue || aircraftType.Simulator == this.Simulator.Value)
                        {
                            this.AircraftTypes.Add(aircraftType);
                        }
                    }
                }
                else
                {
                    // Restore the full list of aircraft types (except where other filters already removed them)
                    this.AircraftTypes.Clear();
                    foreach (var aircraftType in this.allAircraftTypes.OrderBy(t => t.Name))
                    {
                        if (this.AircraftTypeCategory != null && this.AircraftTypeCategory.AircraftTypeCategory != aircraftType.Category)
                        {
                            continue;
                        }

                        if (!string.IsNullOrEmpty(this.Manufacturer) && !aircraftType.Manufacturer.Name.ToLowerInvariant().Contains(this.Manufacturer.ToLowerInvariant()))
                        {
                            continue;
                        }

                        if (!this.Simulator.HasValue || aircraftType.Simulator == this.Simulator.Value)
                        {
                            this.AircraftTypes.Add(aircraftType);
                        }
                    }
                }
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the NM search radius around the selected airport.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int NmRadius
        {
            get => this.nmRadius;

            set
            {
                if (Equals(this.nmRadius, value))
                {
                    return;
                }

                this.nmRadius = value;
                this.NotifyPropertyChanged();

                // todo this.WithinNmAirportChecked = true;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether "only vanilla" is checked.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool OnlyVanillaChecked
        {
            get => this.onlyVanillaChecked;

            set
            {
                if (Equals(this.onlyVanillaChecked, value))
                {
                    return;
                }

                this.onlyVanillaChecked = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the purchase aircraft variants.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<AircraftType> PurchaseAircraftVariants { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the purchase aircraft variants visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Visibility PurchaseAircraftVariantsVisibility
        {
            get => this.purchaseAircraftVariantsVisibility;

            set
            {
                if (Equals(this.purchaseAircraftVariantsVisibility, value))
                {
                    return;
                }

                this.purchaseAircraftVariantsVisibility = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the purchase aircraft visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Visibility PurchaseAircraftVisibility
        {
            get => this.purchaseAircraftVisibility;

            set
            {
                if (Equals(this.purchaseAircraftVisibility, value))
                {
                    return;
                }

                this.purchaseAircraftVisibility = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether "purchase and rent" checked.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool PurchaseAndRentChecked
        {
            get => this.purchaseAndRentChecked;

            set
            {
                if (Equals(this.purchaseAndRentChecked, value))
                {
                    return;
                }

                this.purchaseAndRentChecked = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether "purchase" is checked.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool PurchaseChecked
        {
            get => this.purchaseChecked;

            set
            {
                if (Equals(this.purchaseChecked, value))
                {
                    return;
                }

                this.purchaseChecked = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the purchase command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command PurchaseCommand { get; }

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
        /// Gets or sets a value indicating whether "rent" is checked.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool RentChecked
        {
            get => this.rentChecked;

            set
            {
                if (Equals(this.rentChecked, value))
                {
                    return;
                }

                this.rentChecked = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the reset search command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command ResetSearchCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the sales agreement information text.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string SalesAgreementInfoText => $"This Sales Agreement (this \"Agreement\") is entered into as of the {DateTime.Today.Day.Ordinal()} day of {DateTime.Today:MMMM} {DateTime.Today.Year}, by and among/between:";

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the search command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand SearchCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the selected aircraft.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Aircraft SelectedAircraft
        {
            get => this.selectedAircraft;

            set
            {
                if (Equals(this.selectedAircraft, value))
                {
                    return;
                }

                this.selectedAircraft = value;
                this.NotifyPropertyChanged();
                this.PurchaseCommand.CanExecute = value?.PurchasePrice.HasValue == true;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the selected purchase aircraft variant.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AircraftType SelectedPurchaseAircraftVariant
        {
            get => this.selectedPurchaseAircraftVariant;

            set
            {
                if (Equals(this.selectedPurchaseAircraftVariant, value))
                {
                    return;
                }

                this.selectedPurchaseAircraftVariant = value;
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
        /// Gets or sets the currently selected simulator, or NULL for all simulators.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Simulator? Simulator
        {
            get => this.simulator;

            set
            {
                if (Equals(this.simulator, value))
                {
                    return;
                }

                this.simulator = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the simulators.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<Simulator> Simulators { get; }

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
        /// Gets the aircraft type categories.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<AircraftTypeCategoryComboItem> TypeCategories { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether "within nm of airport" is checked.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool WithinNmAirportChecked
        {
            get => this.withinNmAirportChecked;

            set
            {
                if (Equals(this.withinNmAirportChecked, value))
                {
                    return;
                }

                this.withinNmAirportChecked = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Cancel purchase aircraft.
        /// </summary>
        /// <remarks>
        /// sushi.at, 23/11/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void CancelPurchaseAircraft()
        {
            this.AircraftToPurchase = null;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Clear the aircraft type category.
        /// </summary>
        /// <remarks>
        /// sushi.at, 25/07/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void ClearCategory()
        {
            this.AircraftTypeCategory = null;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Clears the simulator.
        /// </summary>
        /// <remarks>
        /// sushi.at, 25/07/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void ClearSimulator()
        {
            this.Simulator = null;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the available variants of the aircraft the player is currently buying.
        /// </summary>
        /// <remarks>
        /// sushi.at, 23/11/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void GetPurchaseAircraftVariants()
        {
            if (this.AircraftToPurchase == null)
            {
                return;
            }

            this.LoadingText = "Retrieving aircraft variants...";
            try
            {
                var result = OpenSkyService.Instance.GetVariantsOfTypeAsync(this.AircraftToPurchase.TypeID).Result;
                if (!result.IsError)
                {
                    this.GetPurchaseAircraftVariantsCommand.ReportProgress(
                        () =>
                        {
                            this.PurchaseAircraftVariants.Clear();
                            this.PurchaseAircraftVariants.AddRange(result.Data);

                            this.PurchaseAircraftVariantsVisibility = this.PurchaseAircraftVariants.Count > 1 ? Visibility.Visible : Visibility.Collapsed;

                            if (this.PurchaseAircraftVariants.Count > 0)
                            {
                                // Select the base type
                                this.SelectedPurchaseAircraftVariant = this.PurchaseAircraftVariants[0];
                            }
                        });
                }
                else
                {
                    this.GetPurchaseAircraftVariantsCommand.ReportProgress(
                        () =>
                        {
                            Debug.WriteLine("Error retrieving variants for aircraft: " + result.Message);
                            if (!string.IsNullOrEmpty(result.ErrorDetails))
                            {
                                Debug.WriteLine(result.ErrorDetails);
                            }

                            var notification = new OpenSkyNotification("Error retrieving variants for aircraft", result.Message, MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                            notification.SetErrorColorStyle();
                            Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.ViewReference, this.GetPurchaseAircraftVariantsCommand, "Error retrieving variants for aircraft.");
            }
            finally
            {
                this.LoadingText = null;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Processes aircraft search result and applies custom filters.
        /// </summary>
        /// <remarks>
        /// sushi.at, 25/07/2021.
        /// </remarks>
        /// <param name="aircraft">
        /// The aircraft list to filter.
        /// </param>
        /// <returns>
        /// The aircraft from the original collection matching the currently selected filter options.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        private IEnumerable<Aircraft> ProcessAircraftSearchResult(IEnumerable<Aircraft> aircraft)
        {
            var filtered = new List<Aircraft>();
            foreach (var ac in aircraft)
            {
                if (this.AircraftCategoryChecked && this.AircraftTypeCategory != null && ac.Type.Category != this.AircraftTypeCategory.AircraftTypeCategory)
                {
                    continue;
                }

                if (this.AircraftManufacturerChecked && !ac.Type.Manufacturer.Name.ToLowerInvariant().Contains(this.Manufacturer.ToLowerInvariant()))
                {
                    continue;
                }

                if (this.AircraftNameChecked && !ac.Type.Name.ToLowerInvariant().Contains(this.Name.ToLowerInvariant()))
                {
                    continue;
                }

                if (this.PurchaseChecked && !ac.PurchasePrice.HasValue)
                {
                    continue;
                }

                if (this.RentChecked && !ac.RentPrice.HasValue)
                {
                    continue;
                }

                if (this.PurchaseAndRentChecked && !ac.PurchasePrice.HasValue && !ac.RentPrice.HasValue)
                {
                    continue;
                }

                if (this.OnlyVanillaChecked && !ac.Type.IsVanilla)
                {
                    continue;
                }

                if (this.Simulator.HasValue && ac.Type.Simulator != this.Simulator.Value)
                {
                    continue;
                }

                filtered.Add(ac);
            }

            return filtered;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Purchase the selected aircraft.
        /// </summary>
        /// <remarks>
        /// sushi.at, 28/07/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void PurchaseAircraft()
        {
            if (this.SelectedAircraft == null)
            {
                return;
            }

            this.AircraftToPurchase = this.SelectedAircraft;
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

                            this.Manufacturers.Clear();
                            foreach (var typeManufacturer in result.Data.Select(t => t.Manufacturer.Name).Distinct().OrderBy(m => m))
                            {
                                this.Manufacturers.Add(typeManufacturer);
                            }

                            this.AircraftTypes.Clear();
                            foreach (var aircraftType in result.Data.OrderBy(t => t.Name))
                            {
                                if (!this.Simulator.HasValue || aircraftType.Simulator == this.Simulator.Value)
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
        /// Reset the search form.
        /// </summary>
        /// <remarks>
        /// sushi.at, 25/07/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void ResetSearch()
        {
            this.AircraftTypeCategory = null;
            this.Manufacturer = string.Empty;
            this.Name = string.Empty;
            this.AllAircraftTypesChecked = true;

            this.PurchaseChecked = true;
            this.RentChecked = true;

            this.AirportIcao = string.Empty;
            this.NmRadius = 100;
            this.Country = null;
            this.AtAirportChecked = true;

            this.OnlyVanillaChecked = true;
            this.Simulator = null;

            this.Aircraft.Clear();
            this.AvailableAircraftHeader = "Available Aircraft";
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Searches for aircraft matching our search criteria.
        /// </summary>
        /// <remarks>
        /// sushi.at, 25/07/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void Search()
        {
            this.LoadingText = "Searching for aircraft...";
            try
            {
                AircraftIEnumerableApiResponse result = null;
                if (this.AtAirportChecked)
                {
                    // This one is easy as one airport will never contain huge amounts of aircraft
                    if (string.IsNullOrEmpty(this.AirportIcao))
                    {
                        this.SearchCommand.ReportProgress(
                            () =>
                            {
                                var notification = new OpenSkyNotification("Error searching for aircraft", "No airport ICAO code specified.", MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 10);
                                notification.SetErrorColorStyle();
                                Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                            });
                        return;
                    }

                    this.LocationColumnVisibility = Visibility.Collapsed;
                    result = OpenSkyService.Instance.GetAircraftAtAirportAsync(this.AirportIcao).Result;
                }

                if (this.WithinNmAirportChecked)
                {
                    if (string.IsNullOrEmpty(this.AirportIcao))
                    {
                        this.SearchCommand.ReportProgress(
                            () =>
                            {
                                var notification = new OpenSkyNotification("Error searching for aircraft", "No airport ICAO code specified.", MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 10);
                                notification.SetErrorColorStyle();
                                Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                            });
                        return;
                    }

                    if (this.AllAircraftTypesChecked)
                    {
                        ExtendedMessageBoxResult? answer = null;
                        this.SearchCommand.ReportProgress(
                            () =>
                            {
                                var messageBox = new OpenSkyMessageBox(
                                    "Aircraft search",
                                    "Performing a radius aircraft search for all types would potentially return a large number of results, of which only the first 100 will be displayed here. You should use more specific search criteria this type of search.\r\n\r\nAre you sure you want to continue?",
                                    MessageBoxButton.YesNo,
                                    ExtendedMessageBoxImage.Question);
                                messageBox.Closed += (_, _) => { answer = messageBox.Result; };
                                Main.ShowMessageBoxInSaveViewAs(this.ViewReference, messageBox);
                            });
                        while (answer == null && !SleepScheduler.IsShutdownInProgress)
                        {
                            Thread.Sleep(500);
                        }

                        if (answer != ExtendedMessageBoxResult.Yes)
                        {
                            return;
                        }
                    }

                    var searchCriteria = new AircraftSearchAroundAirport
                    {
                        AirportICAO = this.AirportIcao,
                        Radius = this.NmRadius,
                        MaxResults = 100,
                        FilterByCategory = false,
                        OnlyVanilla = this.OnlyVanillaChecked
                    };

                    if (this.AircraftCategoryChecked)
                    {
                        searchCriteria.Category = this.AircraftTypeCategory.AircraftTypeCategory;
                        searchCriteria.FilterByCategory = true;
                    }

                    if (this.AircraftManufacturerChecked)
                    {
                        searchCriteria.Manufacturer = this.Manufacturer;
                    }

                    if (this.AircraftNameChecked)
                    {
                        searchCriteria.Name = this.Name;
                    }

                    this.LocationColumnVisibility = Visibility.Visible;
                    result = OpenSkyService.Instance.SearchAircraftAroundAirportAsync(searchCriteria).Result;
                }

                if (this.CountryChecked)
                {
                    // Country search needs a bit of preparation to build search criteria from selections
                    if (string.IsNullOrEmpty(this.Country))
                    {
                        this.SearchCommand.ReportProgress(
                            () =>
                            {
                                var notification = new OpenSkyNotification("Error searching for aircraft", "No country selected.", MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 10);
                                notification.SetErrorColorStyle();
                                Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                            });
                        return;
                    }

                    var countryCode = this.Country.Split('-')[0].Trim();
                    if (!Enum.TryParse(countryCode, out Country parsedCountry))
                    {
                        this.SearchCommand.ReportProgress(
                            () =>
                            {
                                var notification = new OpenSkyNotification("Error searching for aircraft", "Could not parse country string.", MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 10);
                                notification.SetErrorColorStyle();
                                Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                            });
                        return;
                    }

                    var searchCriteria = new AircraftSearchInCountry
                    {
                        Country = parsedCountry,
                        MaxResults = 100,
                        FilterByCategory = false,
                        OnlyVanilla = this.OnlyVanillaChecked
                    };

                    if (this.AllAircraftTypesChecked)
                    {
                        ExtendedMessageBoxResult? answer = null;
                        this.SearchCommand.ReportProgress(
                            () =>
                            {
                                var messageBox = new OpenSkyMessageBox(
                                    "Aircraft search",
                                    "Performing a country wide aircraft search for all types would potentially return a large number of results, of which only the first 100 will be displayed here. You should use more specific search criteria this type of search.\r\n\r\nAre you sure you want to continue?",
                                    MessageBoxButton.YesNo,
                                    ExtendedMessageBoxImage.Question);
                                messageBox.Closed += (_, _) => { answer = messageBox.Result; };
                                Main.ShowMessageBoxInSaveViewAs(this.ViewReference, messageBox);
                            });
                        while (answer == null && !SleepScheduler.IsShutdownInProgress)
                        {
                            Thread.Sleep(500);
                        }

                        if (answer != ExtendedMessageBoxResult.Yes)
                        {
                            return;
                        }
                    }

                    if (this.AircraftCategoryChecked)
                    {
                        searchCriteria.Category = this.AircraftTypeCategory.AircraftTypeCategory;
                        searchCriteria.FilterByCategory = true;
                    }

                    if (this.AircraftManufacturerChecked)
                    {
                        searchCriteria.Manufacturer = this.Manufacturer;
                    }

                    if (this.AircraftNameChecked)
                    {
                        searchCriteria.Name = this.Name;
                    }

                    this.LocationColumnVisibility = Visibility.Visible;
                    result = OpenSkyService.Instance.SearchAircraftInCountryAsync(searchCriteria).Result;
                }

                if (result != null)
                {
                    if (!result.IsError)
                    {
                        this.SearchCommand.ReportProgress(
                            () =>
                            {
                                this.Aircraft.Clear();
                                foreach (var aircraft in this.ProcessAircraftSearchResult(result.Data))
                                {
                                    this.Aircraft.Add(aircraft);
                                }
                            });
                    }
                    else
                    {
                        this.SearchCommand.ReportProgress(
                            () =>
                            {
                                Debug.WriteLine("Error searching for aircraft: " + result.Message);
                                if (!string.IsNullOrEmpty(result.ErrorDetails))
                                {
                                    Debug.WriteLine(result.ErrorDetails);
                                }

                                var notification = new OpenSkyNotification("Error searching for aircraft", result.Message, MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                                notification.SetErrorColorStyle();
                                Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                            });
                    }
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.ViewReference, this.SearchCommand, "Error searching for aircraft");
            }
            finally
            {
                this.LoadingText = null;
            }

            this.SearchCommand.ReportProgress(() => this.RefreshBalancesCommand.DoExecute(null));
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Sign the purchase agreement and purchase the aircraft.
        /// </summary>
        /// <remarks>
        /// sushi.at, 23/11/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void SignPurchase()
        {
            if (this.AircraftToPurchase == null)
            {
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
                var result = OpenSkyService.Instance.PurchaseAircraftAsync(
                    new PurchaseAircraft
                    {
                        Registry = this.AircraftToPurchase.Registry,
                        ForAirline = false,
                        VariantID = this.SelectedPurchaseAircraftVariant?.Id ?? Guid.Empty
                    }).Result;

                if (!result.IsError)
                {
                    this.SoldStampVisibility = Visibility.Visible;

                    Thread.Sleep(3000);
                    this.SignPurchaseCommand.ReportProgress(
                        () =>
                        {
                            this.CancelPurchaseAircraftCommand.DoExecute(null);
                            this.SearchCommand.DoExecute(null);
                            this.SignPurchaseCommand.CanExecute = true;
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
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AircraftMarketViewModel.cs" company="OpenSky">
// OpenSky project 2021
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

    using OpenSky.Client.MVVM;
    using OpenSky.Client.OpenAPIs.ModelExtensions;
    using OpenSky.Client.Tools;

    using OpenSkyApi;

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
        private bool onlyVanillaChecked = true;

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
        /// The currently selected simulator, or NULL for all simulators.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Simulator? simulator;

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

            // Create commands
            this.RefreshTypesCommand = new AsynchronousCommand(this.RefreshTypes);
            this.ClearCategoryCommand = new Command(this.ClearCategory);
            this.ClearSimulatorCommand = new Command(this.ClearSimulator);
            this.ResetSearchCommand = new Command(this.ResetSearch);
            this.SearchCommand = new AsynchronousCommand(this.Search);
            this.PurchaseCommand = new AsynchronousCommand(this.PurchaseAircraft);
            // todo implement renting when available

            // Fire off initial commands
            this.RefreshTypesCommand.DoExecute(null);
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
                    foreach (var typeManufacturer in this.allAircraftTypes.Where(t => t.Category == value.AircraftTypeCategory).Select(t => t.Manufacturer).Distinct().OrderBy(m => m))
                    {
                        this.Manufacturers.Add(typeManufacturer);
                    }

                    this.AircraftTypes.Clear();
                    foreach (var aircraftType in this.allAircraftTypes.Where(t => t.Category == value.AircraftTypeCategory).OrderBy(t => t.Name))
                    {
                        this.AircraftTypes.Add(aircraftType);
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
                    foreach (var typeManufacturer in this.allAircraftTypes.Select(t => t.Manufacturer).Distinct().OrderBy(m => m))
                    {
                        this.Manufacturers.Add(typeManufacturer);
                    }

                    this.AircraftTypes.Clear();
                    foreach (var aircraftType in this.allAircraftTypes.OrderBy(t => t.Name))
                    {
                        this.AircraftTypes.Add(aircraftType);
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
                    foreach (var typeManufacturer in this.allAircraftTypes.Where(t => this.AircraftTypeCategory == null || t.Category == this.AircraftTypeCategory.AircraftTypeCategory).Select(t => t.Manufacturer).Distinct()
                                                         .Where(m => m.ToLowerInvariant().Contains(value.ToLowerInvariant())).OrderBy(m => m))
                    {
                        this.Manufacturers.Add(typeManufacturer);
                    }

                    // Filter aircraft types to this manufacturer
                    this.AircraftTypes.Clear();
                    foreach (var aircraftType in this.allAircraftTypes.Where(t => t.Manufacturer.ToLowerInvariant().Contains(value.ToLowerInvariant())).OrderBy(t => t.Name))
                    {
                        if (this.AircraftTypeCategory != null && this.AircraftTypeCategory.AircraftTypeCategory != aircraftType.Category)
                        {
                            continue;
                        }

                        this.AircraftTypes.Add(aircraftType);
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
                    foreach (var typeManufacturer in this.allAircraftTypes.Where(t => this.AircraftTypeCategory == null || t.Category == this.AircraftTypeCategory.AircraftTypeCategory).Select(t => t.Manufacturer).Distinct().OrderBy(m => m))
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

                        this.AircraftTypes.Add(aircraftType);
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

                        if (!string.IsNullOrEmpty(this.Manufacturer) && !aircraftType.Manufacturer.ToLowerInvariant().Contains(this.Manufacturer.ToLowerInvariant()))
                        {
                            continue;
                        }

                        this.AircraftTypes.Add(aircraftType);
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

                        if (!string.IsNullOrEmpty(this.Manufacturer) && !aircraftType.Manufacturer.ToLowerInvariant().Contains(this.Manufacturer.ToLowerInvariant()))
                        {
                            continue;
                        }

                        this.AircraftTypes.Add(aircraftType);
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
        public AsynchronousCommand PurchaseCommand { get; }

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

                if (this.AircraftManufacturerChecked && !ac.Type.Manufacturer.ToLowerInvariant().Contains(this.Manufacturer.ToLowerInvariant()))
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

            MessageBoxResult? answer = null;
            this.PurchaseCommand.ReportProgress(
                () =>
                {
                    answer = ModernWpf.MessageBox.Show(
                        $"Are you sure you want to purchase this aircraft?\r\n\r\nRegistration: {this.SelectedAircraft.Registry}\r\nType: {this.SelectedAircraft.Type.Name}\r\nLocation: {this.SelectedAircraft.AirportICAO}\r\nPrice: {this.SelectedAircraft.PurchasePrice:C0}",
                        "Aircraft purchase",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);
                },
                true);
            if (answer is not MessageBoxResult.Yes)
            {
                return;
            }

            this.LoadingText = "Purchasing aircraft...";
            try
            {
                var result = OpenSkyService.Instance.PurchaseAircraftAsync(this.SelectedAircraft.Registry).Result;
                if (!result.IsError)
                {
                    this.PurchaseCommand.ReportProgress(
                        () => { ModernWpf.MessageBox.Show(result.Message, "Aircraft purchase", MessageBoxButton.OK, MessageBoxImage.Information); });
                }
                else
                {
                    this.PurchaseCommand.ReportProgress(
                        () =>
                        {
                            Debug.WriteLine("Error purchasing aircraft: " + result.Message);
                            if (!string.IsNullOrEmpty(result.ErrorDetails))
                            {
                                Debug.WriteLine(result.ErrorDetails);
                            }

                            ModernWpf.MessageBox.Show(result.Message, "Error purchasing aircraft", MessageBoxButton.OK, MessageBoxImage.Error);
                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.PurchaseCommand, "Error purchasing aircraft");
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
                            foreach (var typeManufacturer in result.Data.Select(t => t.Manufacturer).Distinct().OrderBy(m => m))
                            {
                                this.Manufacturers.Add(typeManufacturer);
                            }

                            this.AircraftTypes.Clear();
                            foreach (var aircraftType in result.Data.OrderBy(t => t.Name))
                            {
                                this.AircraftTypes.Add(aircraftType);
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

                            ModernWpf.MessageBox.Show(result.Message, "Error refreshing aircraft types", MessageBoxButton.OK, MessageBoxImage.Error);
                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.RefreshTypesCommand, "Error refreshing aircraft types");
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
                        this.SearchCommand.ReportProgress(() => ModernWpf.MessageBox.Show("No airport ICAO code specified.", "Error searching for aircraft", MessageBoxButton.OK, MessageBoxImage.Error));
                        return;
                    }

                    this.LocationColumnVisibility = Visibility.Collapsed;
                    result = OpenSkyService.Instance.GetAircraftAtAirportAsync(this.AirportIcao).Result;
                }

                if (this.CountryChecked)
                {
                    // Country search needs a bit of preparation to build search criteria from selections
                    if (string.IsNullOrEmpty(this.Country))
                    {
                        this.SearchCommand.ReportProgress(() => ModernWpf.MessageBox.Show("No country selected.", "Error searching for aircraft", MessageBoxButton.OK, MessageBoxImage.Error));
                        return;
                    }

                    var countryCode = this.Country.Split('-')[0].Trim();
                    if (!Enum.TryParse(countryCode, out Country parsedCountry))
                    {
                        this.SearchCommand.ReportProgress(() => ModernWpf.MessageBox.Show("Could not parse country string.", "Error searching for aircraft", MessageBoxButton.OK, MessageBoxImage.Error));
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
                        MessageBoxResult? answer = null;
                        this.SearchCommand.ReportProgress(
                            () =>
                            {
                                answer = ModernWpf.MessageBox.Show(
                                    "Performing a country wide aircraft search for all types would potentially return a large number of results, of which only the first 100 will be displayed here. You should use more specific search criteria this type of search.\r\n\r\nAre you sure you want to continue?",
                                    "Aircraft search",
                                    MessageBoxButton.YesNo,
                                    MessageBoxImage.Question);
                            },
                            true);
                        if (answer is not MessageBoxResult.Yes)
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

                                ModernWpf.MessageBox.Show(result.Message, "Error searching for aircraft", MessageBoxButton.OK, MessageBoxImage.Error);
                            });
                    }
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.SearchCommand, "Error searching for aircraft");
            }
            finally
            {
                this.LoadingText = null;
            }
        }
    }
}
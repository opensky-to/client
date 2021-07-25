// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AircraftViewModel.cs" company="OpenSky">
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
    /// Aircraft view model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 19/07/2021.
    /// </remarks>
    /// <seealso cref="T:OpenSky.Client.MVVM.ViewModel"/>
    /// -------------------------------------------------------------------------------------------------
    public class AircraftViewModel : ViewModel
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
        /// True if "purchase" is checked.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private bool purchaseChecked = true;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// True if "rent" is checked.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private bool rentChecked = true;

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
        /// Initializes a new instance of the <see cref="AircraftViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 19/07/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public AircraftViewModel()
        {
            // Initialize data structures
            this.TypeCategories = new ObservableCollection<AircraftTypeCategoryComboItem>();
            this.Manufacturers = new ObservableCollection<string>();
            this.AircraftTypes = new ObservableCollection<AircraftType>();
            this.Countries = new ObservableCollection<CountryComboItem>();
            this.Simulators = new ObservableCollection<Simulator>();
            this.Aircraft = new ObservableCollection<Aircraft>();

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
        /// Gets the simulators.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<Simulator> Simulators { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the countries.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<CountryComboItem> Countries { get; }

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
        /// Gets the clear simulator command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command ClearSimulatorCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the clear category command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command ClearCategoryCommand { get; }

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
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the aircraft type categories.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<AircraftTypeCategoryComboItem> TypeCategories { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets a list of types of aircraft.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<AircraftType> AircraftTypes { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the manufacturers list.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<string> Manufacturers { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// List of all aircraft types received from the API, with no filters applied.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private ICollection<AircraftType> allAircraftTypes;

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
            this.LoadingText = "Refreshing aircraft types";
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
                if (this.AtAirportChecked)
                {
                    // This one is easy as one airport will never contain huge amounts of aircraft
                    if (string.IsNullOrEmpty(this.AirportIcao))
                    {
                        this.SearchCommand.ReportProgress(() => ModernWpf.MessageBox.Show("No airport ICAO code specified.", "Error searching for aircraft", MessageBoxButton.OK, MessageBoxImage.Error));
                        return;
                    }

                    var result = OpenSkyService.Instance.GetAircraftAtAirportAsync(this.AirportIcao).Result;
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
                if (this.AircraftCategoryChecked && ac.Type.Category != this.AircraftTypeCategory.AircraftTypeCategory)
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
                    if (this.Manufacturer != null && !this.Manufacturers.Any(m => m.ToLowerInvariant().Contains(this.Manufacturer.ToLowerInvariant())))
                    {
                        this.Manufacturer = string.Empty;
                    }

                    // Check if the current name is still in the filtered list
                    if (this.Name != null && !this.AircraftTypes.Any(t => t.Name.ToLowerInvariant().Contains(this.Name.ToLowerInvariant())))
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
                    this.CountryChecked = true;
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

                    // Filter aircraft types to this manufacturer
                    this.AircraftTypes.Clear();
                    if (this.AircraftTypeCategory != null)
                    {
                        foreach (var aircraftType in this.allAircraftTypes.Where(t => t.Category == this.AircraftTypeCategory.AircraftTypeCategory && t.Manufacturer.ToLowerInvariant().Contains(value.ToLowerInvariant())).OrderBy(t => t.Name))
                        {
                            this.AircraftTypes.Add(aircraftType);
                        }
                    }
                    else
                    {
                        foreach (var aircraftType in this.allAircraftTypes.Where(t => t.Manufacturer.ToLowerInvariant().Contains(value.ToLowerInvariant())).OrderBy(t => t.Name))
                        {
                            this.AircraftTypes.Add(aircraftType);
                        }
                    }

                    // Check if the current name is still in the filtered list
                    if (!this.AircraftTypes.Any(t => t.Name.ToLowerInvariant().Contains(this.Name.ToLowerInvariant())))
                    {
                        this.Name = string.Empty;
                    }
                }
                else
                {
                    if (this.AircraftTypeCategory != null)
                    {
                        // Restore the list of aircraft types matching the still selected category
                        this.AircraftTypes.Clear();
                        foreach (var aircraftType in this.allAircraftTypes.Where(t => t.Category == this.AircraftTypeCategory.AircraftTypeCategory).OrderBy(t => t.Name))
                        {
                            this.AircraftTypes.Add(aircraftType);
                        }
                    }
                    else
                    {
                        // Restore the full list of aircraft types
                        this.AircraftTypes.Clear();
                        foreach (var aircraftType in this.allAircraftTypes.OrderBy(t => t.Name))
                        {
                            this.AircraftTypes.Add(aircraftType);
                        }
                    }
                }
            }
        }

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
                this.WithinNmAirportChecked = true;
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
    }
}
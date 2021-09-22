// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SettingsViewModel.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Pages.Models
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Windows;

    using JetBrains.Annotations;

    using OpenSky.Client.Models.Enums;
    using OpenSky.Client.MVVM;
    using OpenSky.Client.Tools;

    using OpenSkyApi;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Settings page view model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 29/06/2021.
    /// </remarks>
    /// <seealso cref="T:OpenSky.Client.MVVM.ViewModel"/>
    /// -------------------------------------------------------------------------------------------------
    public class SettingsViewModel : ViewModel
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Information string describing the airport package file.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string airportPackageFileInfo = "???";

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The fuel unit.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private FuelUnit fuelUnit;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Are there changes to the settings to be saved?
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private bool isDirty;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The weight unit.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private WeightUnit weightUnit;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 29/06/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public SettingsViewModel()
        {
            // Create data structures
            this.WeightUnits = new ObservableCollection<WeightUnit>();
            this.FuelUnits = new ObservableCollection<FuelUnit>();

            // Add initial values
            foreach (WeightUnit unit in Enum.GetValues(typeof(WeightUnit)))
            {
                this.WeightUnits.Add(unit);
            }

            foreach (FuelUnit unit in Enum.GetValues(typeof(FuelUnit)))
            {
                this.FuelUnits.Add(unit);
            }

            // Create command first so that IsDirty can set the CanExecute property
            this.SaveSettingsCommand = new Command(this.SaveSettings, false);
            this.RestoreDefaultsCommand = new Command(this.RestoreDefaults);
            this.LoginOpenSkyUserCommand = new Command(this.LoginOpenSkyUser, !this.UserSession.IsUserLoggedIn);
            this.LogoutOpenSkyUserCommand = new AsynchronousCommand(this.LogoutOpenSkyUser, this.UserSession.IsUserLoggedIn);
            this.RefreshAirportPackageInfoCommand = new AsynchronousCommand(this.RefreshAirportPackageInfo);
            this.DownloadAirportPackageCommand = new AsynchronousCommand(this.DownloadAirportPackage);

            // Load settings
            Properties.Settings.Default.Reload();
            this.WeightUnit = (WeightUnit)Properties.Settings.Default.WeightUnit;
            this.FuelUnit = (FuelUnit)Properties.Settings.Default.FuelUnit;

            // Make sure we are notified if the UserSession service changes user logged in status
            this.UserSession.PropertyChanged += this.UserSessionPropertyChanged;

            // No changes, just us loading
            this.IsDirty = false;

            // Load the initial airport package file info
            this.RefreshAirportPackageInfoCommand.DoExecute(null);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the information string describing the airport package file.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string AirportPackageFileInfo
        {
            get => this.airportPackageFileInfo;

            set
            {
                if (Equals(this.airportPackageFileInfo, value))
                {
                    return;
                }

                this.airportPackageFileInfo = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the download airport package command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand DownloadAirportPackageCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the fuel unit.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public FuelUnit FuelUnit
        {
            get => this.fuelUnit;

            set
            {
                if (Equals(this.fuelUnit, value))
                {
                    return;
                }

                this.fuelUnit = value;
                this.NotifyPropertyChanged();
                this.IsDirty = true;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the fuel units.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<FuelUnit> FuelUnits { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether there are changes to the settings to be saved.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool IsDirty
        {
            get => this.isDirty;

            set
            {
                if (Equals(this.isDirty, value))
                {
                    return;
                }

                this.isDirty = value;
                this.NotifyPropertyChanged();
                this.SaveSettingsCommand.CanExecute = value;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the login OpenSky user command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command LoginOpenSkyUserCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the logout OpenSky user command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand LogoutOpenSkyUserCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the refresh airport package information command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand RefreshAirportPackageInfoCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the restore defaults command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [NotNull]
        public Command RestoreDefaultsCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the save settings command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [NotNull]
        public Command SaveSettingsCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the user session.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public UserSessionService UserSession => UserSessionService.Instance;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the weight unit.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public WeightUnit WeightUnit
        {
            get => this.weightUnit;

            set
            {
                if (Equals(this.weightUnit, value))
                {
                    return;
                }

                this.weightUnit = value;
                this.NotifyPropertyChanged();
                this.IsDirty = true;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the weight units.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<WeightUnit> WeightUnits { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Download airport package.
        /// </summary>
        /// <remarks>
        /// sushi.at, 21/09/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void DownloadAirportPackage()
        {
            try
            {
                if (AirportPackageClientHandler.IsPackageUpToDate())
                {
                    this.DownloadAirportPackageCommand.ReportProgress(() => ModernWpf.MessageBox.Show("The package file is up-to-date, no download is required.", "Airport package", MessageBoxButton.OK, MessageBoxImage.Information));
                }
                else
                {
                    AirportPackageClientHandler.DownloadPackage();
                    this.DownloadAirportPackageCommand.ReportProgress(() => ModernWpf.MessageBox.Show("Successfully downloaded new package file.", "Airport package", MessageBoxButton.OK, MessageBoxImage.Information));
                    this.RefreshAirportPackageInfoCommand.DoExecute(null);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                this.DownloadAirportPackageCommand.ReportProgress(() => ModernWpf.MessageBox.Show($"Error downloading airport package file.\r\n\r\n{ex.Message}", "Airport package", MessageBoxButton.OK, MessageBoxImage.Error));
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Login OpenSky user.
        /// </summary>
        /// <remarks>
        /// sushi.at, 01/06/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void LoginOpenSkyUser()
        {
            Process.Start(Properties.Settings.Default.OpenSkyTokenUrl);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Logout OpenSky user.
        /// </summary>
        /// <remarks>
        /// sushi.at, 01/06/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void LogoutOpenSkyUser()
        {
            try
            {
                var result = OpenSkyService.Instance.RevokeTokenAsync(new RevokeToken { Token = this.UserSession.RefreshToken }).Result;
                if (result.IsError)
                {
                    this.LogoutOpenSkyUserCommand.ReportProgress(
                        () =>
                        {
                            Debug.WriteLine("Error revoking application token: " + result.Message);
                            if (!string.IsNullOrEmpty(result.ErrorDetails))
                            {
                                Debug.WriteLine(result.ErrorDetails);
                            }

                            ModernWpf.MessageBox.Show(result.Message, "Error revoking application token", MessageBoxButton.OK, MessageBoxImage.Error);
                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.LogoutOpenSkyUserCommand, "Error revoking application token", false);
            }

            this.UserSession.Logout();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Refreshes the airport package info string.
        /// </summary>
        /// <remarks>
        /// sushi.at, 21/09/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void RefreshAirportPackageInfo()
        {
            try
            {
                if (AirportPackageClientHandler.IsPackageAvailable())
                {
                    if (AirportPackageClientHandler.IsPackageUpToDate())
                    {
                        var package = AirportPackageClientHandler.GetPackage();
                        this.AirportPackageFileInfo = $"The package file is up-to-date.\r\nCurrent file: {package?.Airports.Count.ToString() ?? "???"} airports, hash {package?.Hash ?? "???"}";
                    }
                    else
                    {
                        var package = AirportPackageClientHandler.GetPackage();
                        this.AirportPackageFileInfo = $"A newer package file is available, please download it using the button on the right.\r\nCurrent file: {package?.Airports.Count.ToString() ?? "???"} airports, hash {package?.Hash ?? "???"}";
                    }
                }
                else
                {
                    this.AirportPackageFileInfo = "The file is missing, please download it using the button on the right.";
                }
            }
            catch (Exception ex)
            {
                this.AirportPackageFileInfo = $"ERROR: {ex.Message}";
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Restore default settings (except keys and users).
        /// </summary>
        /// <remarks>
        /// sushi.at, 23/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void RestoreDefaults()
        {
            var answer = ModernWpf.MessageBox.Show("Are you sure you want to restore all default settings except for keys and users?", "Restore settings?", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (answer == MessageBoxResult.Yes)
            {
                Debug.WriteLine("Resetting settings to defaults...");

                this.WeightUnit = WeightUnit.lbs;
                this.FuelUnit = FuelUnit.gal;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Saves the settings.
        /// </summary>
        /// <remarks>
        /// sushi.at, 23/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void SaveSettings()
        {
            Debug.WriteLine("Saving user settings...");
            try
            {
                Properties.Settings.Default.WeightUnit = (int)this.WeightUnit;
                Properties.Settings.Default.FuelUnit = (int)this.FuelUnit;

                Properties.Settings.Default.Save();
                this.IsDirty = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error saving settings: " + ex);
                ModernWpf.MessageBox.Show(ex.Message, "Error saving settings", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// User session property changed.
        /// </summary>
        /// <remarks>
        /// sushi.at, 01/06/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Property changed event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void UserSessionPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(this.UserSession.IsUserLoggedIn))
            {
                // Update login/logout buttons
                UpdateGUIDelegate updateCommands = () =>
                {
                    this.LoginOpenSkyUserCommand.CanExecute = !this.UserSession.IsUserLoggedIn;
                    this.LogoutOpenSkyUserCommand.CanExecute = this.UserSession.IsUserLoggedIn;
                };
                Application.Current.Dispatcher.BeginInvoke(updateCommands);

                // Check if we have the current client airport package file, after silently trying to download it
                if (this.UserSession.IsUserLoggedIn)
                {
                    try
                    {
                        AirportPackageClientHandler.DownloadPackage();
                    }
                    catch
                    {
                        // Ignore
                    }

                    this.RefreshAirportPackageInfoCommand.DoExecute(null);
                }
            }
        }
    }
}
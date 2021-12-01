// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SettingsViewModel.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Pages.Models
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Windows;
    using System.Windows.Media.Imaging;

    using JetBrains.Annotations;

    using Microsoft.Win32;

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
        /// The Bing maps key.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string bingMapsKey;

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
        /// The loading text.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string loadingText;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The profile image.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private BitmapImage profileImage;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The simBrief username.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string simBriefUsername;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The UTC offset.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private double utcOffset;

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
            this.UtcOffsets = new SortedSet<double>();

            // Populate UTC offsets from time zones
            foreach (var timeZone in TimeZoneInfo.GetSystemTimeZones())
            {
                this.UtcOffsets.Add(timeZone.BaseUtcOffset.TotalHours);
            }

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
            this.SaveSettingsCommand = new AsynchronousCommand(this.SaveSettings, false);
            this.RestoreDefaultsCommand = new Command(this.RestoreDefaults);
            this.LoginOpenSkyUserCommand = new Command(this.LoginOpenSkyUser, !this.UserSession.IsUserLoggedIn);
            this.LogoutOpenSkyUserCommand = new AsynchronousCommand(this.LogoutOpenSkyUser, this.UserSession.IsUserLoggedIn);
            this.RefreshAirportPackageInfoCommand = new AsynchronousCommand(this.RefreshAirportPackageInfo);
            this.DownloadAirportPackageCommand = new AsynchronousCommand(this.DownloadAirportPackage);
            this.ChangePasswordCommand = new Command(this.ChangePassword, this.UserSession.IsUserLoggedIn);
            this.UpdateProfileImageCommand = new AsynchronousCommand(this.UpdateProfileImage, this.UserSession.IsUserLoggedIn);

            // Load settings
            Properties.Settings.Default.Reload();
            this.WeightUnit = (WeightUnit)Properties.Settings.Default.WeightUnit;
            this.FuelUnit = (FuelUnit)Properties.Settings.Default.FuelUnit;
            this.UtcOffset = Properties.Settings.Default.DefaultUTCOffset;
            this.BingMapsKey = UserSessionService.Instance.LinkedAccounts?.BingMapsKey;
            this.SimBriefUsername = UserSessionService.Instance.LinkedAccounts?.SimbriefUsername;

            // Load profile image
            if (UserSessionService.Instance.AccountOverview?.ProfileImage?.Length > 0)
            {
                var image = new BitmapImage();
                using (var mem = new MemoryStream(UserSessionService.Instance.AccountOverview?.ProfileImage))
                {
                    image.BeginInit();
                    image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.UriSource = null;
                    image.StreamSource = mem;
                    image.EndInit();
                }

                image.Freeze();
                this.ProfileImage = image;
            }
            else
            {
                this.ProfileImage = new BitmapImage(new Uri("pack://application:,,,/OpenSky.Client;component/Resources/profile200.png"));
            }

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
        /// Gets or sets the Bing maps key.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string BingMapsKey
        {
            get => this.bingMapsKey;

            set
            {
                if (Equals(this.bingMapsKey, value))
                {
                    return;
                }

                this.bingMapsKey = value;
                this.NotifyPropertyChanged();
                this.IsDirty = true;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the change password command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command ChangePasswordCommand { get; }

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
        /// Gets or sets the profile image.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public BitmapImage ProfileImage
        {
            get => this.profileImage;

            set
            {
                if (Equals(this.profileImage, value))
                {
                    return;
                }

                this.profileImage = value;
                this.NotifyPropertyChanged();
            }
        }

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
        public AsynchronousCommand SaveSettingsCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the simBrief username.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string SimBriefUsername
        {
            get => this.simBriefUsername;

            set
            {
                if (Equals(this.simBriefUsername, value))
                {
                    return;
                }

                this.simBriefUsername = value;
                this.NotifyPropertyChanged();
                this.IsDirty = true;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the update profile image command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand UpdateProfileImageCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the user session.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public UserSessionService UserSession => UserSessionService.Instance;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the UTC offset.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double UtcOffset
        {
            get => this.utcOffset;

            set
            {
                if (Equals(this.utcOffset, value))
                {
                    return;
                }

                this.utcOffset = value;
                this.NotifyPropertyChanged();
                this.IsDirty = true;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the UTC offsets.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public SortedSet<double> UtcOffsets { get; }

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
        /// Change password (opens page in browser).
        /// </summary>
        /// <remarks>
        /// sushi.at, 03/11/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void ChangePassword()
        {
            Process.Start(Properties.Settings.Default.OpenSkyChangePasswordUrl);
        }

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
            this.NotifyPropertyChanged(nameof(this.UserSession));

            this.LogoutOpenSkyUserCommand.ReportProgress(
                () =>
                {
                    var wasDirty = this.IsDirty;
                    this.BingMapsKey = null;
                    this.SimBriefUsername = null;
                    this.ProfileImage = new BitmapImage(new Uri("pack://application:,,,/OpenSky.Client;component/Resources/profile200.png"));
                    this.IsDirty = wasDirty;
                });
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
            if (this.UserSession.IsUserLoggedIn)
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
            else
            {
                this.AirportPackageFileInfo = string.Empty;
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
            this.LoadingText = "Saving settings...";

            // Save local settings
            try
            {
                Properties.Settings.Default.WeightUnit = (int)this.WeightUnit;
                Properties.Settings.Default.FuelUnit = (int)this.FuelUnit;
                Properties.Settings.Default.DefaultUTCOffset = this.UtcOffset;

                Properties.Settings.Default.Save();

                this.SaveSettingsCommand.ReportProgress(() => this.IsDirty = false);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error saving settings: " + ex);
                this.SaveSettingsCommand.ReportProgress(
                    () =>
                        ModernWpf.MessageBox.Show(ex.Message, "Error saving settings", MessageBoxButton.OK, MessageBoxImage.Error));
            }

            // Save server-side settings
            try
            {
                var linkedAccounts = new LinkedAccounts
                {
                    BingMapsKey = this.BingMapsKey,
                    SimbriefUsername = this.SimBriefUsername
                };

                var result = OpenSkyService.Instance.UpdateLinkedAccountsAsync(linkedAccounts).Result;
                if (!result.IsError)
                {
                    _ = UserSessionService.Instance.RefreshLinkedAccounts();
                }
                else
                {
                    this.SaveSettingsCommand.ReportProgress(
                        () =>
                        {
                            Debug.WriteLine("Error saving settings: " + result.Message);
                            if (!string.IsNullOrEmpty(result.ErrorDetails))
                            {
                                Debug.WriteLine(result.ErrorDetails);
                            }

                            ModernWpf.MessageBox.Show(result.Message, "Error saving settings", MessageBoxButton.OK, MessageBoxImage.Error);
                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.SaveSettingsCommand, "Error saving settings.");
            }

            this.LoadingText = null;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Updates the profile image.
        /// </summary>
        /// <remarks>
        /// sushi.at, 25/11/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void UpdateProfileImage()
        {
            bool? answer = null;
            string fileName = null;
            this.UpdateProfileImageCommand.ReportProgress(
                () =>
                {
                    var openDialog = new OpenFileDialog
                    {
                        Title = "Select new profile image",
                        CheckFileExists = true,
                        Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg"
                    };

                    answer = openDialog.ShowDialog();
                    if (answer == true)
                    {
                        fileName = openDialog.FileName;
                    }
                },
                true);

            if (answer != true || string.IsNullOrEmpty(fileName))
            {
                return;
            }

            try
            {
                var result = OpenSkyService.Instance.UploadProfileImageAsync(new FileParameter(File.OpenRead(fileName), fileName, fileName.ToLowerInvariant().EndsWith(".png") ? "image/png" : "image/jpeg")).Result;
                if (result.IsError)
                {
                    this.UpdateProfileImageCommand.ReportProgress(
                        () =>
                        {
                            Debug.WriteLine("Error updating profile image: " + result.Message);
                            if (!string.IsNullOrEmpty(result.ErrorDetails))
                            {
                                Debug.WriteLine(result.ErrorDetails);
                            }

                            ModernWpf.MessageBox.Show(result.Message, "Error updating profile image", MessageBoxButton.OK, MessageBoxImage.Error);
                        });
                }
                else
                {
                    _ = UserSessionService.Instance.RefreshUserAccountOverview().Result;

                    // Load profile image
                    if (UserSessionService.Instance.AccountOverview?.ProfileImage?.Length > 0)
                    {
                        var image = new BitmapImage();
                        using (var mem = new MemoryStream(UserSessionService.Instance.AccountOverview?.ProfileImage))
                        {
                            image.BeginInit();
                            image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                            image.CacheOption = BitmapCacheOption.OnLoad;
                            image.UriSource = null;
                            image.StreamSource = mem;
                            image.EndInit();
                        }

                        image.Freeze();
                        this.ProfileImage = image;
                    }
                    else
                    {
                        this.ProfileImage = new BitmapImage(new Uri("pack://application:,,,/OpenSky.AgentMSFS;component/Resources/profile200.png"));
                    }
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.UpdateProfileImageCommand, "Error updating profile image.");
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
                    this.ChangePasswordCommand.CanExecute = this.UserSession.IsUserLoggedIn;
                    this.UpdateProfileImageCommand.CanExecute = this.UserSession.IsUserLoggedIn;
                };
                Application.Current.Dispatcher.BeginInvoke(updateCommands);

                if (this.UserSession.IsUserLoggedIn)
                {
                    try
                    {
                        UpdateGUIDelegate updateUserSettings = () =>
                        {
                            try
                            {
                                // Load profile image
                                if (UserSessionService.Instance.AccountOverview?.ProfileImage?.Length > 0)
                                {
                                    var image = new BitmapImage();
                                    using (var mem = new MemoryStream(UserSessionService.Instance.AccountOverview?.ProfileImage))
                                    {
                                        image.BeginInit();
                                        image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                                        image.CacheOption = BitmapCacheOption.OnLoad;
                                        image.UriSource = null;
                                        image.StreamSource = mem;
                                        image.EndInit();
                                    }

                                    image.Freeze();
                                    this.ProfileImage = image;
                                }

                                var wasDirty = this.IsDirty;
                                this.BingMapsKey = UserSessionService.Instance.LinkedAccounts?.BingMapsKey;
                                this.SimBriefUsername = UserSessionService.Instance.LinkedAccounts?.SimbriefUsername;
                                this.IsDirty = wasDirty;
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine("Error updating user settings after login: " + ex);
                            }
                        };
                        Application.Current.Dispatcher.BeginInvoke(updateUserSettings);

                        this.NotifyPropertyChanged(nameof(this.UserSession));

                        // Check if we have the current client airport package file, after silently trying to download it
                        AirportPackageClientHandler.DownloadPackage();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Error processing user after-login: " + ex);
                    }

                    this.RefreshAirportPackageInfoCommand.DoExecute(null);
                }
            }
        }
    }
}
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SettingsViewModel.cs" company="OpenSky">
// OpenSky project 2021-2022
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

    using OpenSky.Client.Controls;
    using OpenSky.Client.Controls.Models;
    using OpenSky.Client.Models.Enums;
    using OpenSky.Client.MVVM;
    using OpenSky.Client.Tools;
    using OpenSky.Client.Views;

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
        /// The distance unit.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private DistanceUnit distanceUnit;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The short distance unit.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private ShortDistanceUnit shortDistanceUnit;

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
        /// The currently selected simulator, or NULL for all simulators.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Simulator? simulator;

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
        /// True to automatically launch agent when starting a flight.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private bool autoLaunchAgent;

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
            this.DistanceUnits = new ObservableCollection<DistanceUnit>();
            this.ShortDistanceUnits = new ObservableCollection<ShortDistanceUnit>();
            this.UtcOffsets = new SortedSet<double>();
            this.Simulators = new ObservableCollection<Simulator>();

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

            foreach (DistanceUnit unit in Enum.GetValues(typeof(DistanceUnit)))
            {
                this.DistanceUnits.Add(unit);
            }

            foreach (ShortDistanceUnit unit in Enum.GetValues(typeof(ShortDistanceUnit)))
            {
                this.ShortDistanceUnits.Add(unit);
            }

            foreach (Simulator sim in Enum.GetValues(typeof(Simulator)))
            {
                this.Simulators.Add(sim);
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
            this.ClearSimulatorCommand = new Command(this.ClearSimulator);

            // Load settings
            Properties.Settings.Default.Reload();
            this.WeightUnit = (WeightUnit)Properties.Settings.Default.WeightUnit;
            this.FuelUnit = (FuelUnit)Properties.Settings.Default.FuelUnit;
            this.DistanceUnit = (DistanceUnit)Properties.Settings.Default.DistanceUnit;
            this.ShortDistanceUnit = (ShortDistanceUnit)Properties.Settings.Default.ShortDistanceUnit;
            this.UtcOffset = Properties.Settings.Default.DefaultUTCOffset;
            this.BingMapsKey = UserSessionService.Instance.LinkedAccounts?.BingMapsKey;
            this.SimBriefUsername = UserSessionService.Instance.LinkedAccounts?.SimbriefUsername;
            this.AutoLaunchAgent = Properties.Settings.Default.AutoLaunchAgent;
            if (Properties.Settings.Default.DefaultSimulator != -1)
            {
                this.Simulator = (Simulator)Properties.Settings.Default.DefaultSimulator;
            }

            // Load profile image
            if (UserSessionService.Instance.AccountOverview?.ProfileImage?.Length > 0)
            {
                var image = new BitmapImage();
                
                // ReSharper disable once AssignNullToNotNullAttribute
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
        /// Gets or sets a value indicating whether to automatically launch the agent.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool AutoLaunchAgent
        {
            get => this.autoLaunchAgent;

            set
            {
                if (Equals(this.autoLaunchAgent, value))
                {
                    return;
                }

                this.autoLaunchAgent = value;
                this.NotifyPropertyChanged();
                this.IsDirty = true;
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
        /// Gets the clear simulator command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command ClearSimulatorCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the distance unit.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public DistanceUnit DistanceUnit
        {
            get => this.distanceUnit;

            set
            {
                if (Equals(this.distanceUnit, value))
                {
                    return;
                }

                this.distanceUnit = value;
                this.NotifyPropertyChanged();
                this.IsDirty = true;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the short distance unit.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ShortDistanceUnit ShortDistanceUnit
        {
            get => this.shortDistanceUnit;

            set
            {
                if (Equals(this.distanceUnit, value))
                {
                    return;
                }

                this.shortDistanceUnit = value;
                this.NotifyPropertyChanged();
                this.IsDirty = true;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the distance units.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<DistanceUnit> DistanceUnits { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the short distance units.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<ShortDistanceUnit> ShortDistanceUnits { get; }

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
                this.IsDirty = true;
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
        /// Clears the simulator.
        /// </summary>
        /// <remarks>
        /// sushi.at, 09/02/2022.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void ClearSimulator()
        {
            this.Simulator = null;
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
                    this.DownloadAirportPackageCommand.ReportProgress(
                        () =>
                        {
                            var notification = new OpenSkyNotification("Airport package", "The package file is up-to-date, no download is required.", MessageBoxButton.OK, ExtendedMessageBoxImage.Information, 10);
                            Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                        });
                }
                else
                {
                    AirportPackageClientHandler.DownloadPackage();
                    this.DownloadAirportPackageCommand.ReportProgress(
                        () =>
                        {
                            var notification = new OpenSkyNotification("Airport package", "Successfully downloaded new package file.", MessageBoxButton.OK, ExtendedMessageBoxImage.Check, 10);
                            Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                        });
                    this.RefreshAirportPackageInfoCommand.DoExecute(null);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                this.DownloadAirportPackageCommand.ReportProgress(
                    () =>
                    {
                        var notification = new OpenSkyNotification(
                            new ErrorDetails { DetailedMessage = "Error downloading airport package file.", Exception = ex },
                            "Airport package",
                            "Error downloading airport package file.",
                            ExtendedMessageBoxImage.Error,
                            30);
                        notification.SetErrorColorStyle();
                        Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                    });
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

                            var notification = new OpenSkyNotification("Error revoking application token", result.Message, MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                            notification.SetErrorColorStyle();
                            Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.ViewReference, this.LogoutOpenSkyUserCommand, "Error revoking application token", false);
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
            var messageBox = new OpenSkyMessageBox(
                "Restore settings?",
                "Are you sure you want to restore all default settings except for keys and users?",
                MessageBoxButton.YesNo,
                ExtendedMessageBoxImage.Question);
            messageBox.Closed += (_, _) =>
            {
                if (messageBox.Result == ExtendedMessageBoxResult.Yes)
                {
                    Debug.WriteLine("Resetting settings to defaults...");

                    this.WeightUnit = WeightUnit.lbs;
                    this.FuelUnit = FuelUnit.gal;
                }
            };
            Main.ShowMessageBoxInSaveViewAs(this.ViewReference, messageBox);
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
                Properties.Settings.Default.DistanceUnit = (int)this.DistanceUnit;
                Properties.Settings.Default.ShortDistanceUnit = (int)this.ShortDistanceUnit;
                Properties.Settings.Default.DefaultUTCOffset = this.UtcOffset;
                Properties.Settings.Default.DefaultSimulator = this.Simulator.HasValue ? (int)this.Simulator.Value : -1;
                Properties.Settings.Default.AutoLaunchAgent = this.AutoLaunchAgent;

                Properties.Settings.Default.Save();

                this.SaveSettingsCommand.ReportProgress(() => this.IsDirty = false);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error saving settings: " + ex);
                this.SaveSettingsCommand.ReportProgress(
                    () =>
                    {
                        var notification = new OpenSkyNotification(new ErrorDetails { DetailedMessage = ex.Message, Exception = ex }, "Error saving settings", ex.Message, ExtendedMessageBoxImage.Error, 30);
                        notification.SetErrorColorStyle();
                        Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                    });
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
                    this.SaveSettingsCommand.ReportProgress(
                        () =>
                        {
                            var notification = new OpenSkyNotification("Saving settings", "Successfully saved settings.", MessageBoxButton.OK, ExtendedMessageBoxImage.Check, 10);
                            Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                        });
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

                            var notification = new OpenSkyNotification("Error saving settings", result.Message, MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                            notification.SetErrorColorStyle();
                            Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.ViewReference, this.SaveSettingsCommand, "Error saving settings.");
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

                            var notification = new OpenSkyNotification("Error updating profile image", result.Message, MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                            notification.SetErrorColorStyle();
                            Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                        });
                }
                else
                {
                    _ = UserSessionService.Instance.RefreshUserAccountOverview().Result;

                    // Load profile image
                    if (UserSessionService.Instance.AccountOverview?.ProfileImage?.Length > 0)
                    {
                        var image = new BitmapImage();
                        
                        // ReSharper disable once AssignNullToNotNullAttribute
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
                ex.HandleApiCallException(this.ViewReference, this.UpdateProfileImageCommand, "Error updating profile image.");
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
                                    
                                    // ReSharper disable once AssignNullToNotNullAttribute
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
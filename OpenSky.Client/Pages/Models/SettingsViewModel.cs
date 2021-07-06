// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SettingsViewModel.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Pages.Models
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Windows;

    using JetBrains.Annotations;

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
        /// Are there changes to the settings to be saved?
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private bool isDirty;

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
            // Create command first so that IsDirty can set the CanExecute property
            this.SaveSettingsCommand = new Command(this.SaveSettings, false);
            this.RestoreDefaultsCommand = new Command(this.RestoreDefaults);
            this.LoginOpenSkyUserCommand = new Command(this.LoginOpenSkyUser, !this.UserSession.IsUserLoggedIn);
            this.LogoutOpenSkyUserCommand = new AsynchronousCommand(this.LogoutOpenSkyUser, this.UserSession.IsUserLoggedIn);

            // Load settings (todo)
            Properties.Settings.Default.Reload();

            // Make sure we are notified if the UserSession service changes user logged in status
            this.UserSession.PropertyChanged += this.UserSessionPropertyChanged;

            // No changes, just us loading
            this.IsDirty = false;
        }

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

                // todo
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
                // todo

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
                UpdateGUIDelegate updateCommands = () =>
                {
                    this.LoginOpenSkyUserCommand.CanExecute = !this.UserSession.IsUserLoggedIn;
                    this.LogoutOpenSkyUserCommand.CanExecute = this.UserSession.IsUserLoggedIn;
                };
                Application.Current.Dispatcher.BeginInvoke(updateCommands);
            }
        }
    }
}
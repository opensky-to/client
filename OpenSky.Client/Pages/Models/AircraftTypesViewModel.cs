﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AircraftTypesViewModel.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Pages.Models
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Windows;

    using OpenSky.Client.Controls;
    using OpenSky.Client.Controls.Models;
    using OpenSky.Client.MVVM;
    using OpenSky.Client.Tools;
    using OpenSky.Client.Views;

    using OpenSkyApi;

    using TomsToolbox.Essentials;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Aircraft types view model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 29/11/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class AircraftTypesViewModel : ViewModel
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The aircraft type details visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Visibility aircraftTypeDetailsVisibility = Visibility.Collapsed;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The edit aircraft visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Visibility editAircraftVisibility = Visibility.Collapsed;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The aircraft type currently being edited.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private AircraftType editedAircraftType = new();

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The aircraft type the edited type is a variant of.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private AircraftType editedIsVariantOf;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The next version of the edited type.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private AircraftType editedNextVersion;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The loading text.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string loadingText;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The selected aircraft type.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private AircraftType selectedAircraftType;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The upgrade aircraft visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Visibility upgradeAircraftVisibility = Visibility.Collapsed;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="AircraftTypesViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 29/11/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public AircraftTypesViewModel()
        {
            this.ExistingAircraftTypes = new ObservableCollection<AircraftType>();
            this.SelectedAircraftTypes = new ObservableCollection<AircraftType>();
            this.AircraftUpgrades = new ObservableCollection<AircraftTypeUpgrade>();

            this.GetUserRolesCommand = new AsynchronousCommand(this.GetUserRoles);
            this.RefreshAircraftTypesCommand = new AsynchronousCommand(this.RefreshAircraftTypes);
            this.EnableTypeCommand = new AsynchronousCommand(this.EnableType, false);
            this.ClearTypeSelectionCommand = new Command(this.ClearTypeSelection);
            this.DisableTypeCommand = new AsynchronousCommand(this.DisableType, false);
            this.EnableDetailedChecksCommand = new AsynchronousCommand(this.EnableDetailedChecks, false);
            this.DisableDetailedChecksCommand = new AsynchronousCommand(this.DisableDetailedChecks, false);
            this.DeleteTypeCommand = new AsynchronousCommand(this.DeleteType, false);
            this.StartEditAircraftCommand = new Command(this.StartEditAircraft, false);
            this.CancelEditAircraftCommand = new Command(this.CancelEditAircraft, false);
            this.ClearVariantOfEditedCommand = new Command(this.ClearVariantOfEdited);
            this.ClearNextVersionOfEditedCommand = new Command(this.ClearNextVersionOfEdited);
            this.SaveEditedAircraftTypeCommand = new AsynchronousCommand(this.SaveEditedAircraftType, false);
            this.GetAircraftUpgradesCommand = new AsynchronousCommand(this.GetAircraftUpgrades, false);
            this.CloseUpgradeAircraftCommand = new Command(this.CloseUpgradeAircraft);
            this.UpdateAircraftTypeCommand = new AsynchronousCommand(this.UpdateAircraftType);

            this.GetUserRolesCommand.DoExecute(null);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the aircraft type details visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Visibility AircraftTypeDetailsVisibility
        {
            get => this.aircraftTypeDetailsVisibility;

            set
            {
                if (Equals(this.aircraftTypeDetailsVisibility, value))
                {
                    return;
                }

                this.aircraftTypeDetailsVisibility = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the aircraft upgrades.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<AircraftTypeUpgrade> AircraftUpgrades { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the cancel edit aircraft command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command CancelEditAircraftCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the clear edited next version command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command ClearNextVersionOfEditedCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the clear type selection command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command ClearTypeSelectionCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the clear VariantOf edited command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command ClearVariantOfEditedCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the close upgrade aircraft command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command CloseUpgradeAircraftCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the delete type command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand DeleteTypeCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the enable detailed checks command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand DisableDetailedChecksCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the disable type command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand DisableTypeCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the edit aircraft visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Visibility EditAircraftVisibility
        {
            get => this.editAircraftVisibility;

            set
            {
                if (Equals(this.editAircraftVisibility, value))
                {
                    return;
                }

                this.editAircraftVisibility = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the aircraft type currently being edited.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AircraftType EditedAircraftType
        {
            get => this.editedAircraftType;

            set
            {
                if (Equals(this.editedAircraftType, value))
                {
                    return;
                }

                this.editedAircraftType = value;
                this.NotifyPropertyChanged();

                this.CancelEditAircraftCommand.CanExecute = value != null;
                this.SaveEditedAircraftTypeCommand.CanExecute = value != null;
                this.EditAircraftVisibility = value != null ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the aircraft type the edited type is a variant of.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AircraftType EditedIsVariantOf
        {
            get => this.editedIsVariantOf;

            set
            {
                if (Equals(this.editedIsVariantOf, value))
                {
                    return;
                }

                this.editedIsVariantOf = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the next version of the edited type.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AircraftType EditedNextVersion
        {
            get => this.editedNextVersion;

            set
            {
                if (Equals(this.editedNextVersion, value))
                {
                    return;
                }

                this.editedNextVersion = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the enable detailed checks command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand EnableDetailedChecksCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the enable type command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand EnableTypeCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets a list of of the existing aircraft types.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<AircraftType> ExistingAircraftTypes { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the get aircraft upgrades command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand GetAircraftUpgradesCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the get user roles command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand GetUserRolesCommand { get; }

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
        /// Gets the refresh aircraft types command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand RefreshAircraftTypesCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the save edited aircraft type command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand SaveEditedAircraftTypeCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the the selected aircraft type.
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
                this.AircraftTypeDetailsVisibility = value != null ? Visibility.Visible : Visibility.Collapsed;

                if (UserSessionService.Instance.IsModerator)
                {
                    this.EnableTypeCommand.CanExecute = value != null;
                    this.DisableTypeCommand.CanExecute = value != null;
                    this.EnableDetailedChecksCommand.CanExecute = value != null;
                    this.DisableDetailedChecksCommand.CanExecute = value != null;
                    this.StartEditAircraftCommand.CanExecute = value != null;
                }

                if (UserSessionService.Instance.IsAdmin)
                {
                    this.DeleteTypeCommand.CanExecute = value != null;
                }
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The selected aircraft types.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<AircraftType> SelectedAircraftTypes { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the start edit aircraft command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command StartEditAircraftCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the update aircraft type command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand UpdateAircraftTypeCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the upgrade aircraft visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Visibility UpgradeAircraftVisibility
        {
            get => this.upgradeAircraftVisibility;

            set
            {
                if (Equals(this.upgradeAircraftVisibility, value))
                {
                    return;
                }

                this.upgradeAircraftVisibility = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Closes the upgrade aircraft view and clears the upgrades collection.
        /// </summary>
        /// <remarks>
        /// sushi.at, 29/11/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public void CloseUpgradeAircraft()
        {
            this.UpgradeAircraftVisibility = Visibility.Collapsed;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Cancel edit aircraft type.
        /// </summary>
        /// <remarks>
        /// sushi.at, 10/06/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void CancelEditAircraft()
        {
            this.EditedAircraftType = null;
            this.EditedIsVariantOf = null;
            this.EditedNextVersion = null;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Clears the next version of the edited type.
        /// </summary>
        /// <remarks>
        /// sushi.at, 11/06/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void ClearNextVersionOfEdited()
        {
            this.EditedNextVersion = null;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Clears the type selection.
        /// </summary>
        /// <remarks>
        /// sushi.at, 09/06/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void ClearTypeSelection()
        {
            this.SelectedAircraftType = null;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Clears the VariantOf property of the edited aircraft type.
        /// </summary>
        /// <remarks>
        /// sushi.at, 10/06/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void ClearVariantOfEdited()
        {
            this.EditedIsVariantOf = null;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Deletes the selected aircraft type.
        /// </summary>
        /// <remarks>
        /// sushi.at, 10/06/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void DeleteType()
        {
            if (this.SelectedAircraftTypes.Count != 1)
            {
                this.DeleteTypeCommand.ReportProgress(() =>
                {
                    var notification = new OpenSkyNotification("Error", "Please select exactly one aircraft type!", MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 10);
                    notification.SetErrorColorStyle();
                    Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                });
                return;
            }

            ExtendedMessageBoxResult? answer = null;
            this.DeleteTypeCommand.ReportProgress(
                () =>
                {
                    var messageBox = new OpenSkyMessageBox(
                        "Delete type?",
                        $"Are you sure you want to delete the aircraft type: {this.SelectedAircraftType}",
                        MessageBoxButton.YesNo,
                        ExtendedMessageBoxImage.Hand);
                    messageBox.SetWarningColorStyle();
                    messageBox.Closed += (_, _) =>
                    {
                        answer = messageBox.Result;
                    };
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

            this.LoadingText = "Deleting aircraft type";
            try
            {
                var result = OpenSkyService.Instance.DeleteAircraftTypeAsync(this.SelectedAircraftType.Id).Result;
                if (!result.IsError)
                {
                    this.DeleteTypeCommand.ReportProgress(
                        () => { this.RefreshAircraftTypesCommand.DoExecute(null); });
                }
                else
                {
                    this.DeleteTypeCommand.ReportProgress(
                        () =>
                        {
                            Debug.WriteLine("Error deleting aircraft type: " + result.Message);
                            if (!string.IsNullOrEmpty(result.ErrorDetails))
                            {
                                Debug.WriteLine(result.ErrorDetails);
                            }

                            var notification = new OpenSkyNotification("Error deleting aircraft type", result.Message, MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                            notification.SetErrorColorStyle();
                            Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.ViewReference, this.DeleteTypeCommand, "Error deleting aircraft type");
            }
            finally
            {
                this.LoadingText = null;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Disables the select aircraft type's detailed checks.
        /// </summary>
        /// <remarks>
        /// sushi.at, 10/06/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void DisableDetailedChecks()
        {
            if (this.SelectedAircraftTypes.Count != 1)
            {
                this.DisableDetailedChecksCommand.ReportProgress(() =>
                {
                    var notification = new OpenSkyNotification("Error", "Please select exactly one aircraft type!", MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 10);
                    notification.SetErrorColorStyle();
                    Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                });
                return;
            }

            this.LoadingText = "Enabling aircraft type detailed checks";
            try
            {
                var result = OpenSkyService.Instance.DisableAircraftTypeDetailedChecksAsync(this.SelectedAircraftType.Id).Result;
                if (!result.IsError)
                {
                    this.DisableDetailedChecksCommand.ReportProgress(
                        () => { this.RefreshAircraftTypesCommand.DoExecute(null); });
                }
                else
                {
                    this.DisableDetailedChecksCommand.ReportProgress(
                        () =>
                        {
                            Debug.WriteLine("Error disabling aircraft type detailed checks: " + result.Message);
                            if (!string.IsNullOrEmpty(result.ErrorDetails))
                            {
                                Debug.WriteLine(result.ErrorDetails);
                            }

                            var notification = new OpenSkyNotification("Error disabling aircraft type detailed checks", result.Message, MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                            notification.SetErrorColorStyle();
                            Main.ShowNotificationInSameViewAs(this.ViewReference, notification);

                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.ViewReference, this.DisableDetailedChecksCommand, "Error disabling aircraft type detailed checks");
            }
            finally
            {
                this.LoadingText = null;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Disables the select aircraft type.
        /// </summary>
        /// <remarks>
        /// sushi.at, 09/06/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void DisableType()
        {
            if (this.SelectedAircraftTypes.Count != 1)
            {
                this.DisableTypeCommand.ReportProgress(() =>
                {
                    var notification = new OpenSkyNotification("Error", "Please select exactly one aircraft type!", MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 10);
                    notification.SetErrorColorStyle();
                    Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                });
                return;
            }

            this.LoadingText = "Disabling aircraft type";
            try
            {
                var result = OpenSkyService.Instance.DisableAircraftTypeAsync(this.SelectedAircraftType.Id).Result;
                if (!result.IsError)
                {
                    this.DisableTypeCommand.ReportProgress(
                        () => { this.RefreshAircraftTypesCommand.DoExecute(null); });
                }
                else
                {
                    this.DisableTypeCommand.ReportProgress(
                        () =>
                        {
                            Debug.WriteLine("Error disabling aircraft type: " + result.Message);
                            if (!string.IsNullOrEmpty(result.ErrorDetails))
                            {
                                Debug.WriteLine(result.ErrorDetails);
                            }

                            var notification = new OpenSkyNotification("Error disabling aircraft type", result.Message, MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                            notification.SetErrorColorStyle();
                            Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.ViewReference, this.DisableTypeCommand, "Error disabling aircraft type");
            }
            finally
            {
                this.LoadingText = null;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Enables the select aircraft type's detailed checks.
        /// </summary>
        /// <remarks>
        /// sushi.at, 10/06/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void EnableDetailedChecks()
        {
            if (this.SelectedAircraftTypes.Count != 1)
            {
                this.EnableDetailedChecksCommand.ReportProgress(() =>
                {
                    var notification = new OpenSkyNotification("Error", "Please select exactly one aircraft type!", MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 10);
                    notification.SetErrorColorStyle();
                    Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                });
                return;
            }

            this.LoadingText = "Enabling aircraft type detailed checks";
            try
            {
                var result = OpenSkyService.Instance.EnableAircraftTypeDetailedChecksAsync(this.SelectedAircraftType.Id).Result;
                if (!result.IsError)
                {
                    this.EnableDetailedChecksCommand.ReportProgress(
                        () => { this.RefreshAircraftTypesCommand.DoExecute(null); });
                }
                else
                {
                    this.EnableDetailedChecksCommand.ReportProgress(
                        () =>
                        {
                            Debug.WriteLine("Error enabling aircraft type detailed checks: " + result.Message);
                            if (!string.IsNullOrEmpty(result.ErrorDetails))
                            {
                                Debug.WriteLine(result.ErrorDetails);
                            }

                            var notification = new OpenSkyNotification("Error enabling aircraft type detailed checks", result.Message, MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                            notification.SetErrorColorStyle();
                            Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.ViewReference, this.EnableDetailedChecksCommand, "Error enabling aircraft type detailed checks");
            }
            finally
            {
                this.LoadingText = null;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Enables the select aircraft type.
        /// </summary>
        /// <remarks>
        /// sushi.at, 09/06/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void EnableType()
        {
            if (this.SelectedAircraftTypes.Count != 1)
            {
                this.EnableTypeCommand.ReportProgress(() =>
                {
                    var notification = new OpenSkyNotification("Error", "Please select exactly one aircraft type!", MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 10);
                    notification.SetErrorColorStyle();
                    Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                });
                return;
            }

            this.LoadingText = "Enabling aircraft type";
            try
            {
                var result = OpenSkyService.Instance.EnableAircraftTypeAsync(this.SelectedAircraftType.Id).Result;
                if (!result.IsError)
                {
                    this.EnableTypeCommand.ReportProgress(
                        () => { this.RefreshAircraftTypesCommand.DoExecute(null); });
                }
                else
                {
                    this.EnableTypeCommand.ReportProgress(
                        () =>
                        {
                            Debug.WriteLine("Error enabling aircraft type: " + result.Message);
                            if (!string.IsNullOrEmpty(result.ErrorDetails))
                            {
                                Debug.WriteLine(result.ErrorDetails);
                            }

                            var notification = new OpenSkyNotification("Error enabling aircraft type", result.Message, MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                            notification.SetErrorColorStyle();
                            Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.ViewReference, this.EnableTypeCommand, "Error enabling aircraft type");
            }
            finally
            {
                this.LoadingText = null;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the available aircraft upgrades.
        /// </summary>
        /// <remarks>
        /// sushi.at, 29/11/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void GetAircraftUpgrades()
        {
            if (!UserSessionService.Instance.IsModerator)
            {
                return;
            }

            this.LoadingText = "Checking for available aircraft type upgrades...";
            try
            {
                var result = OpenSkyService.Instance.GetAircraftTypeUpgradesAsync().Result;
                if (!result.IsError)
                {
                    this.GetAircraftUpgradesCommand.ReportProgress(
                        () =>
                        {
                            this.AircraftUpgrades.Clear();
                            this.AircraftUpgrades.AddRange(result.Data);
                            this.UpgradeAircraftVisibility = Visibility.Visible;
                        });
                }
                else
                {
                    this.GetAircraftUpgradesCommand.ReportProgress(
                        () =>
                        {
                            Debug.WriteLine("Error checking for aircraft type upgrades: " + result.Message);
                            if (!string.IsNullOrEmpty(result.ErrorDetails))
                            {
                                Debug.WriteLine(result.ErrorDetails);
                            }

                            var notification = new OpenSkyNotification("Error checking for aircraft type upgrades", result.Message, MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                            notification.SetErrorColorStyle();
                            Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.ViewReference, this.GetAircraftUpgradesCommand, "Error checking for aircraft type upgrades");
            }
            finally
            {
                this.LoadingText = null;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the user's OpenSky roles.
        /// </summary>
        /// <remarks>
        /// sushi.at, 08/06/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void GetUserRoles()
        {
            this.LoadingText = "Fetching your roles";
            var result = UserSessionService.Instance.UpdateUserRoles().Result;
            if (result)
            {
                this.GetUserRolesCommand.ReportProgress(
                    () =>
                    {
                        this.RefreshAircraftTypesCommand.DoExecute(null);
                        this.GetAircraftUpgradesCommand.CanExecute = UserSessionService.Instance.IsModerator;
                    });
            }
            else
            {
                this.GetUserRolesCommand.ReportProgress(
                    () =>
                    {
                        var notification = new OpenSkyNotification("Error", "Error fetching your user roles.", MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                        notification.SetErrorColorStyle();
                        Main.ShowNotificationInSameViewAs(this.ViewReference, notification);

                        this.GetAircraftUpgradesCommand.CanExecute = false;
                    });
            }

            this.LoadingText = null;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Refreshes the list of existing aircraft types.
        /// </summary>
        /// <remarks>
        /// sushi.at, 02/06/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void RefreshAircraftTypes()
        {
            this.LoadingText = "Refreshing aircraft types";
            try
            {
                var result = OpenSkyService.Instance.GetAllAircraftTypesAsync().Result;
                if (!result.IsError)
                {
                    this.RefreshAircraftTypesCommand.ReportProgress(
                        () =>
                        {
                            this.ExistingAircraftTypes.Clear();
                            foreach (var type in result.Data)
                            {
                                this.ExistingAircraftTypes.Add(type);
                            }
                        });
                }
                else
                {
                    this.RefreshAircraftTypesCommand.ReportProgress(
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
                ex.HandleApiCallException(this.ViewReference, this.RefreshAircraftTypesCommand, "Error refreshing aircraft types");
            }
            finally
            {
                this.LoadingText = null;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Saves the edited aircraft type.
        /// </summary>
        /// <remarks>
        /// sushi.at, 10/06/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void SaveEditedAircraftType()
        {
            if (this.EditedAircraftType == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(this.EditedAircraftType.Name) || this.EditedAircraftType.Name.Length < 5)
            {
                var notification = new OpenSkyNotification("Error", "Name not specified or less than 5 characters!", MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 10);
                notification.SetErrorColorStyle();
                Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                return;
            }

            this.EditedAircraftType.IsVariantOf = this.EditedIsVariantOf?.Id;
            this.EditedAircraftType.NextVersion = this.EditedNextVersion?.Id;

            this.LoadingText = "Saving changed aircraft type";
            try
            {
                var result = OpenSkyService.Instance.UpdateAircraftTypeAsync(this.EditedAircraftType).Result;
                if (!result.IsError)
                {
                    this.SaveEditedAircraftTypeCommand.ReportProgress(
                        () =>
                        {
                            var notification = new OpenSkyNotification("Update aircraft type", result.Message, MessageBoxButton.OK, ExtendedMessageBoxImage.Check, 5);
                            Main.ShowNotificationInSameViewAs(this.ViewReference, notification);

                            this.CancelEditAircraft(); // This resets the input form and hides the groupbox
                            this.RefreshAircraftTypesCommand.DoExecute(null);
                        });
                }
                else
                {
                    this.SaveEditedAircraftTypeCommand.ReportProgress(
                        () =>
                        {
                            Debug.WriteLine("Error saving changed aircraft type: " + result.Message);
                            if (!string.IsNullOrEmpty(result.ErrorDetails))
                            {
                                Debug.WriteLine(result.ErrorDetails);
                            }

                            var notification = new OpenSkyNotification("Error saving changed aircraft type", result.Message, MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                            notification.SetErrorColorStyle();
                            Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.ViewReference, this.SaveEditedAircraftTypeCommand, "Error saving changed aircraft type");
            }
            finally
            {
                this.LoadingText = null;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Starts editing the selected aircraft type.
        /// </summary>
        /// <remarks>
        /// sushi.at, 10/06/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void StartEditAircraft()
        {
            if (this.SelectedAircraftTypes.Count != 1)
            {
                var notification = new OpenSkyNotification("Error", "Please select exactly one aircraft type!", MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 10);
                notification.SetErrorColorStyle();
                Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                return;
            }

            this.EditedAircraftType = new AircraftType(this.SelectedAircraftType);
            if (this.SelectedAircraftType.IsVariantOf.HasValue)
            {
                this.EditedIsVariantOf = this.ExistingAircraftTypes.SingleOrDefault(t => t.Id == this.SelectedAircraftType.IsVariantOf);
            }

            if (this.SelectedAircraftType.NextVersion.HasValue)
            {
                this.EditedNextVersion = this.ExistingAircraftTypes.SingleOrDefault(t => t.Id == this.SelectedAircraftType.NextVersion);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Perform the specified aircraft type upgrade.
        /// </summary>
        /// <remarks>
        /// sushi.at, 29/11/2021.
        /// </remarks>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void UpdateAircraftType(object parameter)
        {
            if (parameter is AircraftTypeUpgrade upgrade)
            {
                this.LoadingText = "Upgrading aircraft...";
                try
                {
                    var result = OpenSkyService.Instance.UpgradeAircraftTypeAsync(upgrade).Result;
                    if (!result.IsError)
                    {
                        this.UpdateAircraftTypeCommand.ReportProgress(() => this.GetAircraftUpgradesCommand.DoExecute(null));
                    }
                    else
                    {
                        this.UpdateAircraftTypeCommand.ReportProgress(
                            () =>
                            {
                                Debug.WriteLine("Error performing aircraft type upgrade: " + result.Message);
                                if (!string.IsNullOrEmpty(result.ErrorDetails))
                                {
                                    Debug.WriteLine(result.ErrorDetails);
                                }

                                var notification = new OpenSkyNotification("Error performing aircraft type upgrade", result.Message, MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                                notification.SetErrorColorStyle();
                                Main.ShowNotificationInSameViewAs(this.ViewReference, notification);

                            });
                    }
                }
                catch (Exception ex)
                {
                    ex.HandleApiCallException(this.ViewReference, this.UpdateAircraftTypeCommand, "Error performing aircraft type upgrade");
                }
                finally
                {
                    this.LoadingText = null;
                }
            }
        }
    }
}
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NotificationsViewModel.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Pages.Models
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows;

    using JetBrains.Annotations;

    using OpenSky.Client.Controls;
    using OpenSky.Client.Controls.Models;
    using OpenSky.Client.MVVM;
    using OpenSky.Client.Tools;
    using OpenSky.Client.Views;

    using OpenSkyApi;

    using TomsToolbox.Essentials;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// The notifications view model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 18/12/2023.
    /// </remarks>
    /// <seealso cref="OpenSky.Client.MVVM.ViewModel"/>
    /// -------------------------------------------------------------------------------------------------
    public class NotificationsViewModel : ViewModel
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// All usernames.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private ICollection<string> allUsernames;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The loading text.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string loadingText;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Type of the recipient.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private NotificationRecipient recipientType;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The username.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string username;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The username visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Visibility usernameVisibility = Visibility.Visible;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationsViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 18/12/2023.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public NotificationsViewModel()
        {
            // Initialize data structures
            this.Usernames = new ObservableCollection<string>();
            this.NewNotification = new AddNotification();

            // Create commands
            this.GetUserRolesCommand = new AsynchronousCommand(this.GetUserRoles);
            this.GetUsernamesCommand = new AsynchronousCommand(this.GetUsernames);
            this.RefreshNotificationsCommand = new AsynchronousCommand(this.RefreshNotifications);
            this.AddNotificationCommand = new AsynchronousCommand(this.AddNotification);
            this.ResetFormCommand = new Command(this.ResetForm);

            // Run initial commands
            this.GetUserRolesCommand.DoExecute(null);
            this.GetUsernamesCommand.DoExecute(null);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the add notification command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand AddNotificationCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the get user roles command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand GetUserRolesCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets a value indicating whether this user is admin.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool IsAdmin => UserSessionService.Instance.IsAdmin;

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
        /// Gets the add notification model to edit.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [NotNull]
        public AddNotification NewNotification { get; private set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the type of the recipient.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public NotificationRecipient RecipientType
        {
            get => this.recipientType;

            set
            {
                if (Equals(this.recipientType, value))
                {
                    return;
                }

                this.recipientType = value;
                this.NotifyPropertyChanged();
                this.UsernameVisibility = value == NotificationRecipient.User ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the refresh notifications command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand RefreshNotificationsCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the reset form command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command ResetFormCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string Username
        {
            get => this.username;

            set
            {
                if (Equals(this.username, value))
                {
                    return;
                }

                this.username = value;
                this.NotifyPropertyChanged();

                this.Usernames.Clear();
                foreach (var user in this.allUsernames.OrderBy(u => u))
                {
                    if (string.IsNullOrEmpty(this.Username) || user.ToLowerInvariant().Contains(this.Username.ToLowerInvariant()))
                    {
                        this.Usernames.Add(user);
                    }
                }
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the usernames collection.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<string> Usernames { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the username visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Visibility UsernameVisibility
        {
            get => this.usernameVisibility;

            set
            {
                if (Equals(this.usernameVisibility, value))
                {
                    return;
                }

                this.usernameVisibility = value;
                this.NotifyPropertyChanged();
            }
        }

        private AsynchronousCommand GetUsernamesCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Add new notification.
        /// </summary>
        /// <remarks>
        /// sushi.at, 18/12/2023.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void AddNotification()
        {
            this.LoadingText = "Submitting new notification...";
            try
            {
                this.NewNotification.RecipientType = this.RecipientType;
                this.NewNotification.RecipientUserName = this.Username;

                var result = OpenSkyService.Instance.AddNotificationAsync(this.NewNotification).Result;
                if (!result.IsError)
                {
                    this.AddNotificationCommand.ReportProgress(
                        () =>
                        {
                            this.ResetFormCommand.DoExecute(null);
                            this.RefreshNotificationsCommand.DoExecute(null);
                        });
                }
                else
                {
                    this.AddNotificationCommand.ReportProgress(
                        () =>
                        {
                            var notification = new OpenSkyNotification("Error", "Error submitting new notification", MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                            notification.SetErrorColorStyle();
                            Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.ViewReference, this.AddNotificationCommand, "Error submitting new notification");
            }
            finally
            {
                this.LoadingText = null;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the usernames list.
        /// </summary>
        /// <remarks>
        /// sushi.at, 18/12/2023.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void GetUsernames()
        {
            this.LoadingText = "Fetching user names...";
            try
            {
                var result = OpenSkyService.Instance.GetUserNamesAsync().Result;
                if (!result.IsError)
                {
                    this.GetUsernamesCommand.ReportProgress(
                        () =>
                        {
                            this.allUsernames = result.Data;

                            this.Usernames.Clear();
                            this.Usernames.AddRange(this.allUsernames.OrderBy(u => u));
                        });
                }
                else
                {
                    this.GetUsernamesCommand.ReportProgress(
                        () =>
                        {
                            var notification = new OpenSkyNotification("Error", "Error fetching usernames", MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                            notification.SetErrorColorStyle();
                            Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.ViewReference, this.GetUsernamesCommand, "Error fetching usernames");
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
        /// sushi.at, 18/12/2023.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void GetUserRoles()
        {
            this.LoadingText = "Fetching your roles...";
            var result = UserSessionService.Instance.UpdateUserRoles().Result;
            if (result)
            {
                this.GetUserRolesCommand.ReportProgress(
                    () => { this.NotifyPropertyChanged(nameof(this.IsAdmin)); });
            }
            else
            {
                this.GetUserRolesCommand.ReportProgress(
                    () =>
                    {
                        var notification = new OpenSkyNotification("Error", "Error fetching your user roles", MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                        notification.SetErrorColorStyle();
                        Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                    });
            }

            this.LoadingText = null;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Refresh the notifications.
        /// </summary>
        /// <remarks>
        /// sushi.at, 18/12/2023.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void RefreshNotifications()
        {
            // todo
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Resets the new notification form.
        /// </summary>
        /// <remarks>
        /// sushi.at, 18/12/2023.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void ResetForm()
        {
            this.NewNotification = new AddNotification
            {
                DisplayTimeout = 30
            };
            this.RecipientType = NotificationRecipient.User;
            this.Username = string.Empty;
            this.NotifyPropertyChanged(nameof(this.NewNotification));
        }
    }
}
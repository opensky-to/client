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
    using System.Threading;
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
        /// The selected notification.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private GroupedNotification selectedNotification;

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
            this.ActiveNotifications = new ObservableCollection<GroupedNotification>();
            this.CompletedNotifications = new ObservableCollection<GroupedNotification>();

            // Create commands
            this.GetUserRolesCommand = new AsynchronousCommand(this.GetUserRoles);
            this.GetUsernamesCommand = new AsynchronousCommand(this.GetUsernames);
            this.RefreshNotificationsCommand = new AsynchronousCommand(this.RefreshNotifications);
            this.AddNotificationCommand = new AsynchronousCommand(this.AddNotification);
            this.ResetFormCommand = new Command(this.ResetForm);
            this.DeleteNotificationCommand = new AsynchronousCommand(this.DeleteNotification, false);
            this.FallbackEmailNowCommand = new AsynchronousCommand(this.FallbackEmailNow, false);

            // Run initial commands
            this.GetUserRolesCommand.DoExecute(null);
            this.GetUsernamesCommand.DoExecute(null);
        }


        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the fallback email now command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand FallbackEmailNowCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the active notifications.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<GroupedNotification> ActiveNotifications { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the add notification command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand AddNotificationCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the completed notifications.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<GroupedNotification> CompletedNotifications { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the delete notification command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand DeleteNotificationCommand { get; }

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
        /// Gets or sets the selected notification.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public GroupedNotification SelectedNotification
        {
            get => this.selectedNotification;

            set
            {
                if (Equals(this.selectedNotification, value))
                {
                    return;
                }

                // Deselect to NULL first or other data grid won't deselect
                this.selectedNotification = null;
                this.NotifyPropertyChanged();
                this.selectedNotification = value;
                this.NotifyPropertyChanged();

                this.DeleteNotificationCommand.CanExecute = UserSessionService.Instance.IsAdmin && value != null;
                this.FallbackEmailNowCommand.CanExecute = UserSessionService.Instance.IsAdmin && value != null;
            }
        }

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
                            var notification = new OpenSkyNotification("New notification", result.Message, MessageBoxButton.OK, ExtendedMessageBoxImage.Information, 10);
                            Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
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
        /// Fallback to email for the selected notification.
        /// </summary>
        /// <remarks>
        /// sushi.at, 19/12/2023.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void FallbackEmailNow()
        {
            if (this.SelectedNotification == null)
            {
                return;
            }

            this.LoadingText = "Updating notification...";
            try
            {
                var result = OpenSkyService.Instance.FallbackNotificationToEmailNowAsync(this.SelectedNotification.GroupingID).Result;
                if (!result.IsError)
                {
                    this.FallbackEmailNowCommand.ReportProgress(
                        () => { this.RefreshNotificationsCommand.DoExecute(null); });
                }
                else
                {
                    this.FallbackEmailNowCommand.ReportProgress(
                        () =>
                        {
                            var notification = new OpenSkyNotification("Error", "Error updating notification", MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                            notification.SetErrorColorStyle();
                            Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.ViewReference, this.FallbackEmailNowCommand, "Error updating notification");
            }
            finally
            {
                this.LoadingText = null;
            }
        }


        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Delete the selected notification.
        /// </summary>
        /// <remarks>
        /// sushi.at, 19/12/2023.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void DeleteNotification()
        {
            if (this.SelectedNotification == null)
            {
                return;
            }

            if (this.SelectedNotification.MarkedForDeletion.HasValue)
            {
                ExtendedMessageBoxResult? answer = null;
                this.DeleteNotificationCommand.ReportProgress(
                    () =>
                    {
                        var messageBox = new OpenSkyMessageBox(
                            "Delete notification?",
                            $"The selected notification is already marked for deletion after {this.SelectedNotification.MarkedForDeletion:dd/MM/yyyy HH:mmZ}.\r\n\r\nAre you sure you want to delete it now?",
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
            else
            {
                var delivered = this.SelectedNotification.Recipients.Count(r => r.ClientPickup || r.AgentPickup || r.EmailSent);
                if (delivered < this.SelectedNotification.Recipients.Count)
                {
                    ExtendedMessageBoxResult? answer = null;
                    this.DeleteNotificationCommand.ReportProgress(
                        () =>
                        {
                            var messageBox = new OpenSkyMessageBox(
                                "Delete notification?",
                                $"The selected notification has only been delivered to {delivered} out of {this.SelectedNotification.Recipients.Count} recipients.\r\n\r\nAre you sure you want to delete it now?",
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
            }

            this.LoadingText = "Deleting selected notification...";
            try
            {
                var result = OpenSkyService.Instance.DeleteNotificationAsync(this.SelectedNotification.GroupingID).Result;
                if (!result.IsError)
                {
                    this.DeleteNotificationCommand.ReportProgress(
                        () => { this.RefreshNotificationsCommand.DoExecute(null); });
                }
                else
                {
                    this.DeleteNotificationCommand.ReportProgress(
                        () =>
                        {
                            var notification = new OpenSkyNotification("Error", "Error deleting notification", MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                            notification.SetErrorColorStyle();
                            Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.ViewReference, this.DeleteNotificationCommand, "Error deleting notification");
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
            this.LoadingText = "Fetching notifications...";
            try
            {
                var result = OpenSkyService.Instance.GetAllNotificationsAsync().Result;
                if (!result.IsError)
                {
                    this.RefreshNotificationsCommand.ReportProgress(
                        () =>
                        {
                            this.ActiveNotifications.Clear();
                            this.CompletedNotifications.Clear();
                            this.ActiveNotifications.AddRange(result.Data.Where(n => !n.MarkedForDeletion.HasValue).OrderBy(n => n.Sender));
                            this.CompletedNotifications.AddRange(result.Data.Where(n => n.MarkedForDeletion.HasValue).OrderBy(n => n.Sender));
                        });
                }
                else
                {
                    this.RefreshNotificationsCommand.ReportProgress(
                        () =>
                        {
                            var notification = new OpenSkyNotification("Error", "Error fetching notifications", MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                            notification.SetErrorColorStyle();
                            Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.ViewReference, this.RefreshNotificationsCommand, "Error fetching notifications");
            }
            finally
            {
                this.LoadingText = null;
            }
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
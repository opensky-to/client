// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UserManagerViewModel.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Pages.Models
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.Windows;
    using System.Windows.Media.Imaging;

    using OpenSky.Client.Controls;
    using OpenSky.Client.Controls.Models;
    using OpenSky.Client.MVVM;
    using OpenSky.Client.Tools;
    using OpenSky.Client.Views;

    using OpenSkyApi;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// User manager view model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 21/11/2023.
    /// </remarks>
    /// <seealso cref="OpenSky.Client.MVVM.ViewModel"/>
    /// -------------------------------------------------------------------------------------------------
    public class UserManagerViewModel : ViewModel
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The loading text.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string loadingText;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The selected user.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private User selectedUser;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The selected user profile image.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private BitmapImage selectedUserProfileImage;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="UserManagerViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 21/11/2023.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public UserManagerViewModel()
        {
            // Initialize data structures
            this.Users = new ObservableCollection<User>();

            // Create commands
            this.RefreshUsersCommand = new AsynchronousCommand(this.RefreshUsers);
            this.ToggleModeratorRoleCommand = new AsynchronousCommand(this.ToggleModeratorRole, false);
            this.GetProfileImageForSelectedUserCommand = new AsynchronousCommand(this.GetProfileImageForSelectedUser, false);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the get profile image for selected user command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand GetProfileImageForSelectedUserCommand { get; }

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
        /// Gets the refresh users command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand RefreshUsersCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the selected user.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public User SelectedUser
        {
            get => this.selectedUser;

            set
            {
                if (Equals(this.selectedUser, value))
                {
                    return;
                }

                this.selectedUser = value;
                this.NotifyPropertyChanged();
                this.ToggleModeratorRoleCommand.CanExecute = value != null;
                this.GetProfileImageForSelectedUserCommand.CanExecute = value != null;

                if (value != null)
                {
                    this.GetProfileImageForSelectedUserCommand.DoExecute(null);
                }
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the selected user profile image.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public BitmapImage SelectedUserProfileImage
        {
            get => this.selectedUserProfileImage;

            set
            {
                if (Equals(this.selectedUserProfileImage, value))
                {
                    return;
                }

                this.selectedUserProfileImage = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the toggle moderator role command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand ToggleModeratorRoleCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the users.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<User> Users { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets profile image for selected user.
        /// </summary>
        /// <remarks>
        /// sushi.at, 23/11/2023.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void GetProfileImageForSelectedUser()
        {
            if (this.SelectedUser == null)
            {
                this.GetProfileImageForSelectedUserCommand.ReportProgress(
                    () => { this.SelectedUserProfileImage = new BitmapImage(new Uri("pack://application:,,,/OpenSky.Client;component/Resources/profile200.png")); });
                return;
            }

            this.LoadingText = "Downloading profile image...";
            try
            {
                var result = OpenSkyService.Instance.GetProfileImageAsync(this.SelectedUser.Id).Result;
                if (!result.IsError)
                {
                    var image = new BitmapImage();

                    // ReSharper disable once AssignNullToNotNullAttribute
                    using (var mem = new MemoryStream(result.Data))
                    {
                        image.BeginInit();
                        image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                        image.CacheOption = BitmapCacheOption.OnLoad;
                        image.UriSource = null;
                        image.StreamSource = mem;
                        image.EndInit();
                    }

                    image.Freeze();
                    this.SelectedUserProfileImage = image;
                }
                else
                {
                    this.GetProfileImageForSelectedUserCommand.ReportProgress(
                        () =>
                        {
                            Debug.WriteLine("Error downloading profile image: " + result.Message);
                            if (!string.IsNullOrEmpty(result.ErrorDetails))
                            {
                                Debug.WriteLine(result.ErrorDetails);
                            }

                            var notification = new OpenSkyNotification("Error downloading profile image", result.Message, MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                            notification.SetErrorColorStyle();
                            Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.ViewReference, this.RefreshUsersCommand, "Error downloading profile image");
                this.GetProfileImageForSelectedUserCommand.ReportProgress(
                    () => { this.SelectedUserProfileImage = new BitmapImage(new Uri("pack://application:,,,/OpenSky.Client;component/Resources/profile200.png")); });
            }
            finally
            {
                this.LoadingText = null;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Refreshes the users.
        /// </summary>
        /// <remarks>
        /// sushi.at, 21/11/2023.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void RefreshUsers()
        {
            this.LoadingText = "Refreshing OpenSky users...";
            try
            {
                var result = OpenSkyService.Instance.GetUsersAsync().Result;
                if (!result.IsError)
                {
                    this.RefreshUsersCommand.ReportProgress(
                        () =>
                        {
                            this.Users.Clear();
                            foreach (var user in result.Data)
                            {
                                this.Users.Add(user);
                            }
                        });
                }
                else
                {
                    this.RefreshUsersCommand.ReportProgress(
                        () =>
                        {
                            Debug.WriteLine("Error refreshing OpenSky users: " + result.Message);
                            if (!string.IsNullOrEmpty(result.ErrorDetails))
                            {
                                Debug.WriteLine(result.ErrorDetails);
                            }

                            var notification = new OpenSkyNotification("Error refreshing OpenSky users", result.Message, MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                            notification.SetErrorColorStyle();
                            Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.ViewReference, this.RefreshUsersCommand, "Error refreshing OpenSky users");
            }
            finally
            {
                this.LoadingText = null;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Toggle moderator role for selected user.
        /// </summary>
        /// <remarks>
        /// sushi.at, 23/11/2023.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void ToggleModeratorRole()
        {
            if (this.SelectedUser == null)
            {
                return;
            }

            this.LoadingText = $"Toggling moderator role for user {this.SelectedUser.Username}...";
            try
            {
                var result = OpenSkyService.Instance.SetModeratorRoleAsync(
                    new ModeratorRole
                    {
                        Username = this.SelectedUser.Username,
                        IsModerator = !this.SelectedUser.Roles.Contains("Moderator")
                    }).Result;

                if (!result.IsError)
                {
                    this.ToggleModeratorRoleCommand.ReportProgress(
                        () =>
                        {
                            var messageBox = new OpenSkyNotification("Toggle moderator", result.Message, MessageBoxButton.OK, ExtendedMessageBoxImage.Check, 10);
                            Main.ShowNotificationInSameViewAs(this.ViewReference, messageBox);
                            this.RefreshUsersCommand.DoExecute(null);
                        });
                }
                else
                {
                    this.ToggleModeratorRoleCommand.ReportProgress(
                        () =>
                        {
                            Debug.WriteLine("Error toggling moderator role: " + result.Message);
                            if (!string.IsNullOrEmpty(result.ErrorDetails))
                            {
                                Debug.WriteLine(result.ErrorDetails);
                            }

                            var notification = new OpenSkyNotification("Error toggling moderator role", result.Message, MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                            notification.SetErrorColorStyle();
                            Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.ViewReference, this.ToggleModeratorRoleCommand, "Error toggling moderator role");
            }
            finally
            {
                this.LoadingText = null;
            }
        }
    }
}
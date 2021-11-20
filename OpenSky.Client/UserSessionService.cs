// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UserSessionService.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    using JetBrains.Annotations;

    using OpenSky.Client.Properties;

    using OpenSkyApi;

    using XDMessaging;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// User session service for OpenSky API.
    /// </summary>
    /// <remarks>
    /// sushi.at, 01/06/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class UserSessionService : INotifyPropertyChanged
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The user roles.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly List<string> userRoles = new();

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Is a user currently logged in?
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private bool isUserLoggedIn;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The username (for display purposes only)
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string username;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes static members of the <see cref="UserSessionService"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 01/06/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        static UserSessionService()
        {
            Instance = new UserSessionService();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="UserSessionService"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 01/06/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private UserSessionService()
        {
            this.LoadOpenSkyTokens();

            var client = new XDMessagingClient();
            var listener = client.Listeners.GetListenerForMode(XDTransportMode.Compatibility);
            listener.RegisterChannel("OPENSKY-CLIENT");
            listener.MessageReceived += (_, e) =>
            {
                if (e.DataGram.Channel == "OPENSKY-CLIENT")
                {
                    switch (e.DataGram.Message)
                    {
                        case "TokensUpdated":
                            this.LoadOpenSkyTokens();
                            break;
                    }
                }
            };
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public event PropertyChangedEventHandler PropertyChanged;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the single static instance.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static UserSessionService Instance { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the OpenSky user account overview.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AccountOverview AccountOverview { get; private set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the Date/Time of the expiration of the main JWT OpenSky API token.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public DateTime? Expiration { get; private set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets a value indicating whether the current user is an admin.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool IsAdmin => this.userRoles.Contains("Admin");

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets a value indicating whether the current user is a moderator.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool IsModerator => this.userRoles.Contains("Moderator") || this.userRoles.Contains("Admin");

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets a value indicating whether there is a user currently logged in.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool IsUserLoggedIn
        {
            get => this.isUserLoggedIn;

            set
            {
                if (Equals(this.isUserLoggedIn, value))
                {
                    return;
                }

                this.isUserLoggedIn = value;
                this.OnPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the linked accounts.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public LinkedAccounts LinkedAccounts { get; private set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the current OpenSky API token, null if no token is available.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string OpenSkyApiToken { get; private set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the Date/Time of the refresh token expiration.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public DateTime? RefreshExpiration { get; private set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the refresh token.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string RefreshToken { get; private set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the username (for display purposes only).
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
                this.OnPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the user roles.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public IEnumerable<string> UserRoles => this.userRoles;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Check token expiration.
        /// </summary>
        /// <remarks>
        /// sushi.at, 01/06/2021.
        /// </remarks>
        /// <returns>
        /// True if the main JWT token needs to be refreshed, false if it is current or the refresh token
        /// is also expired (will trigger logout!).
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public bool CheckTokenNeedsRefresh()
        {
            if (!string.IsNullOrEmpty(this.OpenSkyApiToken) && this.Expiration.HasValue && this.Expiration.Value > DateTime.UtcNow)
            {
                this.IsUserLoggedIn = true;
                return false;
            }

            if (!string.IsNullOrEmpty(this.RefreshToken) && this.RefreshExpiration.HasValue && this.RefreshExpiration.Value > DateTime.UtcNow)
            {
                this.IsUserLoggedIn = true;
                return true;
            }

            this.Logout();
            return false;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Force token refresh.
        /// </summary>
        /// <remarks>
        /// sushi.at, 04/06/2021.
        /// </remarks>
        /// <returns>
        /// An asynchronous result that yields true if the token refresh worked, false if the token is
        /// now invalid and the user needs to login again.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public async Task<bool> ForceTokenRefresh()
        {
            this.Expiration = DateTime.MinValue;

            try
            {
                var result = await OpenSkyService.Instance.GetUserRolesAsync();
                return !result.IsError;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during force-token refresh, logging user out! {ex}");
                this.Logout();
                return false;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// User wants to log out.
        /// </summary>
        /// <remarks>
        /// sushi.at, 07/05/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public void Logout()
        {
            this.IsUserLoggedIn = false;
            this.OpenSkyApiToken = null;
            this.Expiration = null;
            this.Username = null;
            this.RefreshToken = null;
            this.RefreshExpiration = null;
            this.AccountOverview = null;
            this.LinkedAccounts = null;

            Settings.Default.OpenSkyApiToken = null;
            Settings.Default.OpenSkyTokenExpiration = DateTime.MinValue;
            Settings.Default.OpenSkyRefreshToken = null;
            Settings.Default.OpenSkyRefreshTokenExpiration = DateTime.MinValue;
            Settings.Default.Save();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Refresh linked accounts.
        /// </summary>
        /// <remarks>
        /// sushi.at, 03/11/2021.
        /// </remarks>
        /// <returns>
        /// An asynchronous result that yields true if it succeeds, false if it fails.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public async Task<bool> RefreshLinkedAccounts()
        {
            try
            {
                var result = await OpenSkyService.Instance.GetLinkedAccountsAsync();
                if (!result.IsError)
                {
                    this.LinkedAccounts = result.Data;
                }

                return !result.IsError;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error refreshing linked accounts! {ex}");
                this.LinkedAccounts = null;
                return false;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Refresh user account overview.
        /// </summary>
        /// <remarks>
        /// sushi.at, 03/11/2021.
        /// </remarks>
        /// <returns>
        /// An asynchronous result that yields true if it succeeds, false if it fails.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public async Task<bool> RefreshUserAccountOverview()
        {
            try
            {
                var result = await OpenSkyService.Instance.GetAccountOverviewAsync();
                if (!result.IsError)
                {
                    this.AccountOverview = result.Data;
                }

                return !result.IsError;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during refresh user account overview! {ex}");
                this.AccountOverview = null;
                return false;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The tokens were refreshed.
        /// </summary>
        /// <remarks>
        /// sushi.at, 01/06/2021.
        /// </remarks>
        /// <param name="refreshToken">
        /// The refresh token response model (contains new tokens).
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public void TokensWereRefreshed(RefreshTokenResponse refreshToken)
        {
            this.OpenSkyApiToken = refreshToken.Token;
            this.Expiration = refreshToken.Expiration.UtcDateTime;
            this.RefreshToken = refreshToken.RefreshToken;
            this.RefreshExpiration = refreshToken.RefreshTokenExpiration.UtcDateTime;

            Settings.Default.OpenSkyApiToken = refreshToken.Token;
            Settings.Default.OpenSkyTokenExpiration = refreshToken.Expiration.UtcDateTime;
            Settings.Default.OpenSkyRefreshToken = refreshToken.RefreshToken;
            Settings.Default.OpenSkyRefreshTokenExpiration = refreshToken.RefreshTokenExpiration.UtcDateTime;
            Settings.Default.Save();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Updates the user roles.
        /// </summary>
        /// <remarks>
        /// sushi.at, 09/06/2021.
        /// </remarks>
        /// <returns>
        /// An asynchronous result that yields true if it succeeds, false if it fails.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public async Task<bool> UpdateUserRoles()
        {
            try
            {
                var result = await OpenSkyService.Instance.GetUserRolesAsync();
                if (!result.IsError)
                {
                    this.userRoles.Clear();
                    this.userRoles.AddRange(result.Data);
                }

                return !result.IsError;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during update user roles! {ex}");
                this.userRoles.Clear();
                return false;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Executes the property changed action.
        /// </summary>
        /// <remarks>
        /// sushi.at, 01/06/2021.
        /// </remarks>
        /// <param name="propertyName">
        /// (Optional) Name of the property.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Load OpenSky tokens from settings.
        /// </summary>
        /// <remarks>
        /// sushi.at, 01/06/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void LoadOpenSkyTokens()
        {
            Settings.Default.Reload();
            this.OpenSkyApiToken = Settings.Default.OpenSkyApiToken;
            try
            {
                this.Expiration = Settings.Default.OpenSkyTokenExpiration;
            }
            catch (NullReferenceException)
            {
            }

            this.RefreshToken = Settings.Default.OpenSkyRefreshToken;
            try
            {
                this.RefreshExpiration = Settings.Default.OpenSkyRefreshTokenExpiration;
            }
            catch (NullReferenceException)
            {
            }

            this.Username = Settings.Default.OpenSkyUsername;

            this.CheckTokenNeedsRefresh();
        }
    }
}
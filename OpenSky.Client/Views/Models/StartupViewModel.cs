// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StartupViewModel.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Views.Models
{
    using System;
    using System.Reflection;

    using JetBrains.Annotations;

    using OpenSky.Client.MVVM;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     The startup view model.
    /// </summary>
    /// <remarks>
    ///     sushi.at, 11/03/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class StartupViewModel : ViewModel
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Initializes a new instance of the <see cref="StartupViewModel" /> class.
        /// </summary>
        /// <remarks>
        ///     sushi.at, 12/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------

        // ReSharper disable NotNullMemberIsNotInitialized
        public StartupViewModel()
        {
            if (!Startup.StartupFailed)
            {
                if (Instance != null)
                    throw new Exception("Only one instance of the startup view model may be created!");

                Instance = this;
                if (!UserSessionService.Instance.IsUserLoggedIn)
                    LoginNotification.Open();
            }

            // Initialize commands
            // todo
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets the single instance of the startup view model.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [CanBeNull]
        public static StartupViewModel Instance { get; private set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets the version string of the application assembly.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string VersionString => $"v{Assembly.GetExecutingAssembly().GetName().Version}";
    }
}
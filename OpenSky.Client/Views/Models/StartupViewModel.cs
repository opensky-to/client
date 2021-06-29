// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StartupViewModel.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Views.Models
{
    using System;
    using System.Reflection;
    using System.Threading;

    using JetBrains.Annotations;

    using OpenSky.Client.MVVM;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// The startup view model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 11/03/2021.
    /// </remarks>
    /// <seealso cref="T:OpenSky.Client.MVVM.ViewModel"/>
    /// -------------------------------------------------------------------------------------------------
    public class StartupViewModel : ViewModel
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="StartupViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 15/06/2021.
        /// </remarks>
        /// <exception cref="Exception">
        /// Thrown when an exception error condition occurs.
        /// </exception>
        /// -------------------------------------------------------------------------------------------------

        // ReSharper disable NotNullMemberIsNotInitialized
        public StartupViewModel()
        {
            if (!Startup.StartupFailed)
            {
                if (Instance != null)
                {
                    throw new Exception("Only one instance of the startup view model may be created!");
                }

                Instance = this;
            }

            // Initialize commands
            this.StartupChecksCommand = new AsynchronousCommand(this.StartupChecks);
            this.StartupChecksCommand.DoExecute(null);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Startup checks.
        /// </summary>
        /// <remarks>
        /// sushi.at, 15/06/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void StartupChecks()
        {
            var checksStarted = DateTime.Now;

            // Is no user logged in?
            if (!UserSessionService.Instance.IsUserLoggedIn)
            {
                this.StartupChecksCommand.ReportProgress(() => LoginNotification.Open());
            }

            // todo other checks

            // Show the splash screen for at least for 2 seconds, then open the main window and trigger the close window event
            Thread.Sleep(2000 - (int)(DateTime.Now - checksStarted).TotalMilliseconds);
            this.StartupChecksCommand.ReportProgress(() =>
            {
                new Main().Show();
                this.CloseWindow?.Invoke(this, EventArgs.Empty);
            });
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the startup checks command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand StartupChecksCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the single instance of the startup view model.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [CanBeNull]
        public static StartupViewModel Instance { get; private set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the version string of the application assembly.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string VersionString => $"v{Assembly.GetExecutingAssembly().GetName().Version}";

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Occurs when the view model wants to close the window.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public event EventHandler CloseWindow;
    }
}
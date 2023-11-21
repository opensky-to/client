// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LoginNotificationViewModel.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Views.Models
{
    using System;
    using System.Diagnostics;
    using System.Media;
    using System.Reflection;
    using System.Threading;

    using OpenSky.Client.MVVM;
    using OpenSky.Client.Tools;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Login notification view model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 01/06/2021.
    /// </remarks>
    /// <seealso cref="T:OpenSky.Client.MVVM.ViewModel"/>
    /// -------------------------------------------------------------------------------------------------
    public class LoginNotificationViewModel : ViewModel
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The sound last played Date/Time.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private DateTime soundLastPlayed = DateTime.Now;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="LoginNotificationViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 01/06/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public LoginNotificationViewModel()
        {
            this.Timeout = DateTime.Now.AddMilliseconds(60 * 1000);

            var assembly = Assembly.GetExecutingAssembly();
            var player = new SoundPlayer(assembly.GetManifestResourceStream("OpenSky.Client.Resources.OSdingdong.wav"));
            player.Play();

            this.LoginCommand = new Command(this.Login);
            new Thread(this.NotificationTimeout) { Name = "OpenSky.LoginNotificationTimeout" }.Start();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Occurs when the view model wants to close the window.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public event EventHandler CloseWindow;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the login command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command LoginCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the timeout for the notification.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public DateTime Timeout { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Play sound again.
        /// </summary>
        /// <remarks>
        /// sushi.at, 04/06/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public void PlaySoundAgain()
        {
            if ((DateTime.Now - this.soundLastPlayed).TotalSeconds > 10)
            {
                this.soundLastPlayed = DateTime.Now;

                var assembly = Assembly.GetExecutingAssembly();
                var player = new SoundPlayer(assembly.GetManifestResourceStream("OpenSky.Client.Resources.OSdingdong.wav"));
                player.Play();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Start OpenSky login in default browser.
        /// </summary>
        /// <remarks>
        /// sushi.at, 01/06/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void Login()
        {
            Process.Start(Properties.Settings.Default.OpenSkyTokenUrl);
            this.CloseWindow?.Invoke(this, EventArgs.Empty);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Notification timeout.
        /// </summary>
        /// <remarks>
        /// sushi.at, 27/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void NotificationTimeout()
        {
            while (DateTime.Now < this.Timeout && !SleepScheduler.IsShutdownInProgress && !UserSessionService.Instance.IsUserLoggedIn)
            {
                Thread.Sleep(2000);
            }

            this.CloseWindow?.Invoke(this, EventArgs.Empty);
        }
    }
}
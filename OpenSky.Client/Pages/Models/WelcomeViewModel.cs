// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WelcomeViewModel.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Pages.Models
{
    using System;
    using System.IO;

    using OpenSky.Client.MVVM;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Welcome page view model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 08/12/2021.
    /// </remarks>
    /// <seealso cref="T:OpenSky.Client.MVVM.ViewModel"/>
    /// -------------------------------------------------------------------------------------------------
    public class WelcomeViewModel : ViewModel
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The change log.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string changeLog;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="WelcomeViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 08/12/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public WelcomeViewModel()
        {
            this.LoadChangeLogCommand = new AsynchronousCommand(this.LoadChangeLog);

            this.LoadChangeLogCommand.DoExecute(null);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the change log.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string ChangeLog
        {
            get => this.changeLog;

            set
            {
                if (Equals(this.changeLog, value))
                {
                    return;
                }

                this.changeLog = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the load change log command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand LoadChangeLogCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Loads the change log from changelog.txt.
        /// </summary>
        /// <remarks>
        /// sushi.at, 08/12/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void LoadChangeLog()
        {
            try
            {
                var changeLogText = File.ReadAllText("changelog.txt");
                this.LoadChangeLogCommand.ReportProgress(() => this.ChangeLog = changeLogText);
            }
            catch (Exception ex)
            {
                this.changeLog = $"Error: {ex}";
            }
        }
    }
}
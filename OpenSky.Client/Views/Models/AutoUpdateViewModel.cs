// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AutoUpdateViewModel.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Views.Models
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Reflection;
    using System.Windows;

    using Newtonsoft.Json;

    using OpenSky.Client.MVVM;
    using OpenSky.Client.Tools;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Auto update view model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 24/11/2021.
    /// </remarks>
    /// <seealso cref="T:OpenSky.AgentMSFS.MVVM.ViewModel"/>
    /// -------------------------------------------------------------------------------------------------
    public class AutoUpdateViewModel : ViewModel
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The change log markup.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string changeLogMarkup;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The current version.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Version currentVersion;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The download progress.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private int downloadProgress;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// URL of the download.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string downloadURL;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The installed version.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Version installedVersion;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// True to show, false to hide the in taskbar.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private bool showInTaskbar;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Name of the update.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string updateName;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The window opacity.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private int windowOpacity;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="AutoUpdateViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 24/11/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public AutoUpdateViewModel()
        {
            this.InstalledVersion = Assembly.GetEntryAssembly().GetVersionWithoutRevision();

            // Create commands
            this.CheckForUpdatesCommand = new AsynchronousCommand(this.CheckForUpdates);
            this.DownloadUpdateCommand = new AsynchronousCommand(this.DownloadUpdate);

            // Fire off initial commands
            this.CheckForUpdatesCommand.DoExecute(null);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Occurs when the viewmodel wants to close the window.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public event EventHandler Close;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the change log markup.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string ChangeLogMarkup
        {
            get => this.changeLogMarkup;

            set
            {
                if (Equals(this.changeLogMarkup, value))
                {
                    return;
                }

                this.changeLogMarkup = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the check for updates command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand CheckForUpdatesCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the current version.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Version CurrentVersion
        {
            get => this.currentVersion;

            set
            {
                if (Equals(this.currentVersion, value))
                {
                    return;
                }

                this.currentVersion = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged(nameof(this.VersionInfoText));
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the download progress.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int DownloadProgress
        {
            get => this.downloadProgress;

            set
            {
                if (Equals(this.downloadProgress, value))
                {
                    return;
                }

                this.downloadProgress = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the download update command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand DownloadUpdateCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets URL of the download.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string DownloadURL
        {
            get => this.downloadURL;

            set
            {
                if (Equals(this.downloadURL, value))
                {
                    return;
                }

                this.downloadURL = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the installed version.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Version InstalledVersion
        {
            get => this.installedVersion;

            set
            {
                if (Equals(this.installedVersion, value))
                {
                    return;
                }

                this.installedVersion = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged(nameof(this.VersionInfoText));
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether the in taskbar is shown.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool ShowInTaskbar
        {
            get => this.showInTaskbar;

            set
            {
                if (Equals(this.showInTaskbar, value))
                {
                    return;
                }

                this.showInTaskbar = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the name of the update.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string UpdateName
        {
            get => this.updateName;

            set
            {
                if (Equals(this.updateName, value))
                {
                    return;
                }

                this.updateName = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the version information text.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string VersionInfoText => $"OpenSky client {this.CurrentVersion} is now available. You have version {this.InstalledVersion} installed.\r\nWould you like to download it now?";

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the window opacity.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int WindowOpacity
        {
            get => this.windowOpacity;

            set
            {
                if (Equals(this.windowOpacity, value))
                {
                    return;
                }

                this.windowOpacity = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Check for updates.
        /// </summary>
        /// <remarks>
        /// sushi.at, 24/11/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void CheckForUpdates()
        {
            var updateAvailable = false;
            try
            {
                using var client = new WebClient();
                client.Headers[HttpRequestHeader.UserAgent] = "OpenSky.AgentMSFS";

                var jsonString = client.DownloadString(Properties.Settings.Default.AutoUpdateURL);
                dynamic json = JsonConvert.DeserializeObject(jsonString);

                if ("Not Found".Equals(json?.message))
                {
                    Debug.WriteLine("Github release information not found.");
                }
                else
                {
                    if (!string.IsNullOrEmpty((string)json?.tag_name))
                    {
                        var version = (string)json.tag_name;
                        Debug.WriteLine($"Auto update remote version: {version}");
                        this.CurrentVersion = new Version(version.Replace("v", string.Empty));
                    }

                    if (json?.assets.Count > 0)
                    {
                        for (var i = 0; i < json?.assets.Count; i++)
                        {
                            if (!string.IsNullOrEmpty((string)json?.assets[i].browser_download_url))
                            {
                                var url = (string)json.assets[0].browser_download_url;
                                Debug.WriteLine($"Auto update asset URL: {url}");
                                if (url.ToLowerInvariant().EndsWith(".msi"))
                                {
                                    this.DownloadURL = url;
                                }
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty((string)json?.body))
                    {
                        var mdBody = (string)json.body;
                        Debug.WriteLine($"Auto update changelog MD: {mdBody}");
                        this.ChangeLogMarkup = mdBody;
                    }

                    if (!string.IsNullOrEmpty((string)json?.name))
                    {
                        var name = (string)json.name;
                        Debug.WriteLine($"Auto update name: {name}");
                        this.UpdateName = $" for {name}";
                    }

                    if (this.CurrentVersion > this.InstalledVersion && !string.IsNullOrEmpty(this.DownloadURL))
                    {
                        updateAvailable = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error checking for update: {ex}");
            }

            if (updateAvailable)
            {
                this.CheckForUpdatesCommand.ReportProgress(
                    () =>
                    {
                        this.ShowInTaskbar = true;
                        this.WindowOpacity = 100;
                    });
            }
            else
            {
                this.CheckForUpdatesCommand.ReportProgress(() => this.Close?.Invoke(this, EventArgs.Empty));
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Download of file completed.
        /// </summary>
        /// <remarks>
        /// sushi.at, 24/11/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Asynchronous completed event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Debug.WriteLine(e.Error);
                UpdateGUIDelegate showError = () =>
                {
                    ModernWpf.MessageBox.Show("Error downloading update: " + e.Error.Message);
                    this.DownloadUpdateCommand.CanExecute = true;
                };
                Application.Current.Dispatcher.BeginInvoke(showError);
                return;
            }

            if (e.Cancelled)
            {
                UpdateGUIDelegate close = () => { this.Close?.Invoke(this, EventArgs.Empty); };
                Application.Current.Dispatcher.BeginInvoke(close);
                return;
            }

            if (e.UserState is string path)
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = "msiexec",
                    Arguments = $"/i \"{path}\"",
                    Verb = "runas"
                };

                Process.Start(processInfo);

                // Make sure we shut down, and quick :)
                Environment.Exit(0);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Download progress changed.
        /// </summary>
        /// <remarks>
        /// sushi.at, 24/11/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Download progress changed event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            this.DownloadProgress = e.ProgressPercentage;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Downloads and installs the update.
        /// </summary>
        /// <remarks>
        /// sushi.at, 24/11/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void DownloadUpdate()
        {
            UpdateGUIDelegate disableCommand = () => this.DownloadUpdateCommand.CanExecute = false;
            Application.Current.Dispatcher.BeginInvoke(disableCommand);
            if (string.IsNullOrEmpty(this.DownloadURL))
            {
                this.DownloadUpdateCommand.ReportProgress(() => this.Close?.Invoke(this, EventArgs.Empty));
                return;
            }

            var localInstallerFile = Path.Combine(Path.GetTempPath(), this.DownloadURL.Substring(this.DownloadURL.LastIndexOf('/') + 1));

            using var client = new WebClient();
            client.Headers[HttpRequestHeader.UserAgent] = "OpenSky.AgentMSFS";
            client.DownloadProgressChanged += this.DownloadProgressChanged;
            client.DownloadFileCompleted += this.DownloadFileCompleted;
            client.DownloadFileAsync(new Uri(this.DownloadURL), localInstallerFile, localInstallerFile);
        }
    }
}
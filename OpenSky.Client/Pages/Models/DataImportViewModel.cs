// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataImportViewModel.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Pages.Models
{
    using System;
    using System.Collections.ObjectModel;
    using System.Data.SQLite;
    using System.Diagnostics;
    using System.IO;
    using System.Windows;

    using JetBrains.Annotations;

    using Microsoft.Win32;

    using OpenSky.Client.Controls;
    using OpenSky.Client.Controls.Models;
    using OpenSky.Client.MVVM;
    using OpenSky.Client.Tools;
    using OpenSky.Client.Views;

    using OpenSkyApi;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Data import view model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 01/07/2021.
    /// </remarks>
    /// <seealso cref="T:OpenSky.Client.MVVM.ViewModel"/>
    /// -------------------------------------------------------------------------------------------------
    public class DataImportViewModel : ViewModel
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The Little Navmap MSFS database tables to delete in order to reduce the file size (700mb-
        /// >50mb)
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly string[] littleNavmapMSFSTablesToDelete =
        {
            "taxi_path",
            "nav_search",
            "vor",
            "ndb",
            "parking",
            "airway",
            "waypoint",
            "route_edge_airway",
            "route_edge_radio",
            "route_node_airway",
            "route_node_radio",
            "translation",
            "apron",
            "marker",
            "boundary",
            "airport_file",
            "helipad",
            "start",
            "approach_leg",
            "airport_large",
            "airport_medium",
            "ils",
            "transition_leg",
            "magdecl",
            "metadata",
            "mora_grid",
            "script",
            "sqlite_stat1"
        };

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The import status details.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string importStatusDetails;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The log text for the Little Navmap MSFS import.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string littleNavmapLogText;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The loading text.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string loadingText;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The progress maximum.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private int progressMax = 100;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The progress value.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private int progressValue;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The selected import.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private DataImport selectedImport;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="DataImportViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 01/07/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public DataImportViewModel()
        {
            // Initialize data structures
            this.DataImports = new ObservableCollection<DataImport>();

            // Create commands
            this.RefreshDataImportsCommand = new AsynchronousCommand(this.RefreshDataImports);
            this.ClearDataImportSelectionCommand = new Command(this.ClearDataImportSelection);
            this.BrowseLittleNavmapMSFSCommand = new AsynchronousCommand(this.BrowseLittleNavmapMSFS);
            this.GenerateClientAirportPackageCommand = new AsynchronousCommand(this.GenerateClientAirportPackage);
            this.RefreshDataImportStatusCommand = new AsynchronousCommand(this.RefreshDataImportStatus);

            this.RefreshDataImportsCommand.DoExecute(null);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the browse little navmap msfs command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand BrowseLittleNavmapMSFSCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the clear data import selection command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command ClearDataImportSelectionCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the data imports.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<DataImport> DataImports { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the generate client airport package command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand GenerateClientAirportPackageCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the import status details.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string ImportStatusDetails
        {
            get => this.importStatusDetails;

            set
            {
                if (Equals(this.importStatusDetails, value))
                {
                    return;
                }

                this.importStatusDetails = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged(nameof(this.ImportStatusDetailsVisibility));
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the import status details visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Visibility ImportStatusDetailsVisibility => string.IsNullOrEmpty(this.ImportStatusDetails) ? Visibility.Collapsed : Visibility.Visible;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the log text for the Little Navmap import.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string LittleNavmapLogText
        {
            get => this.littleNavmapLogText;

            set
            {
                if (Equals(this.littleNavmapLogText, value))
                {
                    return;
                }

                this.littleNavmapLogText = value;
                this.NotifyPropertyChanged();
            }
        }

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
        /// Gets or sets the progress maximum.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int ProgressMax
        {
            get => this.progressMax;

            set
            {
                if (Equals(this.progressMax, value))
                {
                    return;
                }

                this.progressMax = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the progress value.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int ProgressValue
        {
            get => this.progressValue;

            set
            {
                if (Equals(this.progressValue, value))
                {
                    return;
                }

                this.progressValue = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the refresh data imports command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand RefreshDataImportsCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the refresh data import status command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand RefreshDataImportStatusCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the selected import.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public DataImport SelectedImport
        {
            get => this.selectedImport;

            set
            {
                if (Equals(this.selectedImport, value))
                {
                    return;
                }

                this.selectedImport = value;
                this.NotifyPropertyChanged();

                if (value?.ImportStatus != null)
                {
                    this.FormatImportStatus(value.ImportStatus);
                }
                else
                {
                    this.ImportStatusDetails = string.Empty;
                }
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Analyze Little Navmap MSFS SQLite database.
        /// </summary>
        /// <remarks>
        /// sushi.at, 02/07/2021.
        /// </remarks>
        /// <exception cref="Exception">
        /// Thrown when an exception error condition occurs.
        /// </exception>
        /// <param name="fileName">
        /// Filename of the file.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void AnalyzeLittleNavmapMSFS(string fileName)
        {
            Debug.WriteLine($"Analyzing SQLite database {fileName}...");
            this.LittleNavmapLogText += $"Analyzing SQLite database {fileName}...\r\n";
            var connection = new SQLiteConnection($"URI=file:{fileName};Read Only=true");
            connection.Open();

            // Airports
            var airportCountCommand = new SQLiteCommand("SELECT COUNT(ident) FROM airport", connection);
            if (airportCountCommand.ExecuteScalar() is not long airportCount)
            {
                throw new Exception("Selected sqlite database contains 0 airports or count failed");
            }

            this.LittleNavmapLogText += $"Selected sqlite database contains {airportCount} airports\r\n";

            // Runways
            var runwayCountCommand = new SQLiteCommand("SELECT COUNT(runway_id) FROM runway", connection);
            if (runwayCountCommand.ExecuteScalar() is not long runwayCount)
            {
                throw new Exception("Selected sqlite database contains 0 runways or count failed");
            }

            this.LittleNavmapLogText += $"Selected sqlite database contains {runwayCount} runways\r\n";

            // Runway ends
            var runwayEndCountCommand = new SQLiteCommand("SELECT COUNT(runway_end_id) FROM runway_end", connection);
            if (runwayEndCountCommand.ExecuteScalar() is not long runwayEndCount)
            {
                throw new Exception("Selected sqlite database contains 0 runway ends or count failed");
            }

            this.LittleNavmapLogText += $"Selected sqlite database contains {runwayEndCount} runway ends\r\n";

            // Approaches
            var approachCountCommand = new SQLiteCommand("SELECT COUNT(approach_id) FROM approach", connection);
            if (approachCountCommand.ExecuteScalar() is not long approachCount)
            {
                throw new Exception("Selected sqlite database contains 0 approaches or count failed");
            }

            this.LittleNavmapLogText += $"Selected sqlite database contains {approachCount} approaches\r\n";

            airportCountCommand.Dispose();
            runwayCountCommand.Dispose();
            runwayEndCountCommand.Dispose();
            approachCountCommand.Dispose();
            connection.Close();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Browse for Little Navmap MSFS SQLite database file.
        /// </summary>
        /// <remarks>
        /// sushi.at, 01/07/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void BrowseLittleNavmapMSFS()
        {
            bool? openFileResult = null;
            string fileName = null;
            this.BrowseLittleNavmapMSFSCommand.ReportProgress(
                () =>
                {
                    var littleNavmapDbDirectory = "%appdata%\\ABarthel\\little_navmap_db\\";
                    littleNavmapDbDirectory = Environment.ExpandEnvironmentVariables(littleNavmapDbDirectory);

                    var openFileDialog = new OpenFileDialog
                    {
                        DefaultExt = ".sqlite",
                        Filter = "SQLite databases (.sqlite)|*.sqlite",
                        FileName = "little_navmap_msfs.sqlite",
                        Multiselect = false
                    };

                    if (Directory.Exists(littleNavmapDbDirectory))
                    {
                        openFileDialog.InitialDirectory = littleNavmapDbDirectory;
                    }

                    openFileResult = openFileDialog.ShowDialog();
                    fileName = openFileDialog.FileName;
                },
                true);

            if (openFileResult == true)
            {
                try
                {
                    this.LittleNavmapLogText = string.Empty;
                    this.AnalyzeLittleNavmapMSFS(fileName);
                    var openSkyFileName = fileName.Replace(".sqlite", "_opensky.sqlite");
                    this.LittleNavmapLogText += $"Copying database to {openSkyFileName}...\r\n";
                    File.Copy(fileName, openSkyFileName, true);
                    this.ShrinkLittleNavmapMSFS(openSkyFileName);
                    this.UploadLittleNavmapMSFS(openSkyFileName);

                    this.LittleNavmapLogText += "\r\n\r\n";
                }
                catch (Exception ex)
                {
                    this.LittleNavmapLogText += $"Error processing LittleNavmap SQLite database.\r\n{ex}\r\n";
                }
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Clears the data import selection.
        /// </summary>
        /// <remarks>
        /// sushi.at, 02/07/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void ClearDataImportSelection()
        {
            this.SelectedImport = null;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Format import status.
        /// </summary>
        /// <remarks>
        /// sushi.at, 02/07/2021.
        /// </remarks>
        /// <param name="status">
        /// The status. This cannot be null.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void FormatImportStatus([NotNull] DataImportStatus status)
        {
            var updateText = "---------------------------------------------------------\r\n";
            updateText += $"{"ELEMENT",-15} | PROCESSED /  TOTAL | NEW    | SKIPPED |\r\n";
            updateText += "---------------------------------------------------------\r\n";
            foreach (var element in status.Elements)
            {
                updateText += $"{element.Key,-15} | {element.Value.Processed,9:D1} / {element.Value.Total,6:D1} | {element.Value.New,6:D1} | {element.Value.Skipped,7:D1} | \r\n";
            }

            updateText += "---------------------------------------------------------\r\n";
            updateText += $"Processed {status.Processed} / {status.Total} [{status.PercentDone} %]";

            this.ImportStatusDetails = updateText;
            this.ProgressMax = status.Total;
            this.ProgressValue = status.Processed;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Send a request to the OpenSky server to generate a new client airport package.
        /// </summary>
        /// <remarks>
        /// sushi.at, 21/09/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void GenerateClientAirportPackage()
        {
            this.LoadingText = "Generating client airport package";
            try
            {
                var result = OpenSkyService.Instance.CreateAirportClientPackageAsync().Result;
                if (!result.IsError)
                {
                    this.GenerateClientAirportPackageCommand.ReportProgress(
                        () =>
                        {
                            var notification = new OpenSkyNotification("Generate airport package", result.Message, MessageBoxButton.OK, ExtendedMessageBoxImage.Check, 10);
                            Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                        });
                }
                else
                {
                    this.GenerateClientAirportPackageCommand.ReportProgress(
                        () =>
                        {
                            Debug.WriteLine("Error generating client airport package: " + result.Message);
                            if (!string.IsNullOrEmpty(result.ErrorDetails))
                            {
                                Debug.WriteLine(result.ErrorDetails);
                            }

                            var notification = new OpenSkyNotification("Error generating client airport package", result.Message, MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                            notification.SetErrorColorStyle();
                            Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.ViewReference, this.RefreshDataImportsCommand, "Error generating client airport package");
            }
            finally
            {
                this.LoadingText = null;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Refresh data imports.
        /// </summary>
        /// <remarks>
        /// sushi.at, 02/07/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void RefreshDataImports()
        {
            this.LoadingText = "Refreshing data imports";
            try
            {
                var result = OpenSkyService.Instance.GetDataImportsAsync().Result;
                if (!result.IsError)
                {
                    this.RefreshDataImportsCommand.ReportProgress(
                        () =>
                        {
                            this.DataImports.Clear();
                            foreach (var import in result.Data)
                            {
                                this.DataImports.Add(import);
                            }
                        });
                }
                else
                {
                    this.RefreshDataImportsCommand.ReportProgress(
                        () =>
                        {
                            Debug.WriteLine("Error refreshing data imports: " + result.Message);
                            if (!string.IsNullOrEmpty(result.ErrorDetails))
                            {
                                Debug.WriteLine(result.ErrorDetails);
                            }

                            var notification = new OpenSkyNotification("Error refreshing data imports", result.Message, MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                            notification.SetErrorColorStyle();
                            Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.ViewReference, this.RefreshDataImportsCommand, "Error refreshing data imports");
            }
            finally
            {
                this.LoadingText = null;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Refresh data import status of currently selected import.
        /// </summary>
        /// <remarks>
        /// sushi.at, 27/01/2022.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void RefreshDataImportStatus()
        {
            if (this.SelectedImport is { Finished: null })
            {
                var updateResult = OpenSkyService.Instance.GetImportStatusAsync(this.SelectedImport.Id).Result;
                if (updateResult.IsError)
                {
                    Debug.WriteLine($"Error monitoring data import process: {updateResult.Message}");
                    if (!string.IsNullOrEmpty(updateResult.ErrorDetails))
                    {
                        Debug.WriteLine(updateResult.ErrorDetails);
                    }
                }
                else
                {
                    this.FormatImportStatus(updateResult.Data);
                    if (updateResult.Status?.Equals("COMPLETE") == true || (updateResult.Data != null && updateResult.Data.Processed == updateResult.Data.Total))
                    {
                        this.RefreshDataImportStatusCommand.ReportProgress(() => this.RefreshDataImportsCommand.DoExecute(null));
                    }
                }
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Shrink Little Navmap MSFS SQLite database.
        /// </summary>
        /// <remarks>
        /// sushi.at, 02/07/2021.
        /// </remarks>
        /// <param name="fileName">
        /// Filename of the file.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void ShrinkLittleNavmapMSFS(string fileName)
        {
            Debug.WriteLine($"Shrinking SQLite database {fileName}...");
            this.LittleNavmapLogText += $"Shrinking SQLite database {fileName}...\r\n";
            var connection = new SQLiteConnection($"URI=file:{fileName};Read Only=false");
            connection.Open();

            foreach (var table in this.littleNavmapMSFSTablesToDelete)
            {
                this.LittleNavmapLogText += $"Deleting table {table}...\r\n";
                var command = new SQLiteCommand($"DROP TABLE IF EXISTS {table}", connection);
                command.ExecuteNonQuery();
                command.Dispose();
            }

            this.LittleNavmapLogText += "Cleaning up database to reduce file size...\r\n";
            var vacuumCommand = new SQLiteCommand("vacuum", connection);
            vacuumCommand.ExecuteNonQuery();
            vacuumCommand.Dispose();

            connection.Close();
            connection.Dispose();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Upload Little Navmap MSFS SQLite database (shrunk version) to OpenSky API server for
        /// processing.
        /// </summary>
        /// <remarks>
        /// sushi.at, 02/07/2021.
        /// </remarks>
        /// <param name="fileName">
        /// Filename of the file.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void UploadLittleNavmapMSFS(string fileName)
        {
            try
            {
                Debug.WriteLine("Uploading Little Navmap MSFS sqlite database...");
                this.LittleNavmapLogText += "Uploading Little Navmap MSFS sqlite database...\r\n";
                var fileParamter = new FileParameter(File.OpenRead(fileName), fileName, "application/x-sqlite3");
                var result = OpenSkyService.Instance.LittleNavmapMSFSAsync(fileParamter).Result;
                if (!result.IsError)
                {
                    var importID = result.Data;
                    if (importID.HasValue)
                    {
                        this.BrowseLittleNavmapMSFSCommand.ReportProgress(
                            () => { this.RefreshDataImportsCommand.DoExecute(null); },
                            true);

                        foreach (var dataImport in this.DataImports)
                        {
                            if (dataImport.Id == importID.Value)
                            {
                                this.SelectedImport = dataImport;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    Debug.WriteLine($"Error uploading Little Navmap MSFS sqlite database: {result.Message}");
                    this.LittleNavmapLogText += $"Error uploading Little Navmap MSFS sqlite database:\r\n{result.Message}\r\n";
                    if (!string.IsNullOrEmpty(result.ErrorDetails))
                    {
                        Debug.WriteLine(result.ErrorDetails);
                        this.LittleNavmapLogText += $"{result.ErrorDetails}\r\n";
                    }
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.ViewReference, this.BrowseLittleNavmapMSFSCommand, "Error uploading or monitoring database import.");
                throw;
            }
        }
    }
}
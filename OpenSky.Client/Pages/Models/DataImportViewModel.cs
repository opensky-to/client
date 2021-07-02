// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataImportViewModel.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Pages.Models
{
    using System;
    using System.Collections.Generic;
    using System.Data.SQLite;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Win32;

    using OpenSky.Client.MVVM;
    using OpenSky.Client.Tools;

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
        /// The log text.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string logText;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the log text.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string LogText
        {
            get => this.logText;

            set
            {
                if (Equals(this.logText, value))
                {
                    return;
                }

                this.logText = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The progress maximum.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private int progressMax;

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
        /// The progress value.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private int progressValue;

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
        /// Initializes a new instance of the <see cref="DataImportViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 01/07/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public DataImportViewModel()
        {
            // Create commands
            this.BrowseLittleNavmapMSFSCommand = new AsynchronousCommand(this.BrowseLittleNavmapMSFS);
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
                }, true);

            if (openFileResult == true)
            {
                try
                {
                    this.AnalyzeLittleNavmapMSFS(fileName);
                    var openSkyFileName = fileName.Replace(".sqlite", "_opensky.sqlite");
                    this.LogText += $"Copying database to {openSkyFileName}...";
                    File.Copy(fileName, openSkyFileName, true);
                    this.ShrinkLittleNavmapMSFS(openSkyFileName);
                    this.UploadLittleNavmapMSFS(openSkyFileName);
                }
                catch (Exception ex)
                {
                    this.LogText += $"Error processing LittleNavmap SQLite database.\r\n{ex}\r\n";
                }
            }
        }

        private void UploadLittleNavmapMSFS(string fileName)
        {
            try
            {
                Debug.WriteLine("Uploading Little Navmap MSFS sqlite database...");
                var fileParamter = new FileParameter(File.OpenRead(fileName), fileName, "application/x-sqlite3");
                var result = OpenSkyService.Instance.LittleNavmapMSFSAsync(fileParamter).Result;
                if (!result.IsError)
                {
                    var importID = result.Data;
                    if (importID.HasValue)
                    {
                        Thread.Sleep(5000);
                        var updateResult = OpenSkyService.Instance.GetImportStatusAsync(importID.Value).Result;
                        while (!updateResult.IsError && updateResult.Status != "COMPLETE")
                        {
                            this.ProgressMax = updateResult.Data.Total;
                            this.ProgressValue = updateResult.Data.Processed;

                            var updateText = $"Status of data import {importID.Value}\r\n";
                            updateText += "----------------------------------------------------------\r\n";
                            updateText += $"{"ELEMENT",-15} | PROCESSED | SKIPPED | {"TOTAL",9}\r\n";
                            foreach (var element in updateResult.Data.Elements)
                            {
                                updateText += $"{element.Key,-15} | {element.Value.Processed,9:D1} | {element.Value.Skipped,7:D1} | {element.Value.Total,9:D1}\r\n";

                                //updateText += string.Format("{0-20} | \r\n", element.Key);
                            }

                            this.LogText = updateText;

                            Thread.Sleep(5000);
                            updateResult = OpenSkyService.Instance.GetImportStatusAsync(importID.Value).Result;
                        }

                        if (updateResult.IsError)
                        {
                            Debug.WriteLine($"Error monitoring data import process: {updateResult.Message}");
                            this.LogText += $"Error monitoring data import process:\r\n{updateResult.Message}\r\n";
                            if (!string.IsNullOrEmpty(updateResult.ErrorDetails))
                            {
                                Debug.WriteLine(updateResult.ErrorDetails);
                                this.LogText += $"{updateResult.ErrorDetails}\r\n";
                            }
                        }
                        else
                        {
                            this.LogText += "----------------------------------------------------------\r\n";
                            this.LogText += updateResult.Message;
                        }
                    }
                }
                else
                {
                    Debug.WriteLine($"Error uploading Little Navmap MSFS sqlite database: {result.Message}");
                    this.LogText += $"Error uploading Little Navmap MSFS sqlite database:\r\n{result.Message}\r\n";
                    if (!string.IsNullOrEmpty(result.ErrorDetails))
                    {
                        Debug.WriteLine(result.ErrorDetails);
                        this.LogText += $"{result.ErrorDetails}\r\n";
                    }
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.BrowseLittleNavmapMSFSCommand, "Error uploading or monitoring database import.");
                throw;
            }

        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The Little Navmap MSFS database tables to delete in order to reduce the file size (700mb-
        /// >50mb)
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly string[] littleNavmapMSFSTablesToDelete = {
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

        private void ShrinkLittleNavmapMSFS(string fileName)
        {
            Debug.WriteLine($"Shrinking SQLite database {fileName}...");
            this.LogText += $"Shrinking SQLite database {fileName}...\r\n";
            var connection = new SQLiteConnection($"URI=file:{fileName};Read Only=false");
            connection.Open();

            this.ProgressMax = this.littleNavmapMSFSTablesToDelete.Length;
            foreach (var table in this.littleNavmapMSFSTablesToDelete)
            {
                this.LogText += $"Deleting table {table}...\r\n";
                var command = new SQLiteCommand($"DROP TABLE IF EXISTS {table}", connection);
                command.ExecuteNonQuery();
                command.Dispose();
                this.ProgressValue++;
            }

            this.LogText += "Cleaning up database to reduce file size...";
            var vacuumCommand = new SQLiteCommand("vacuum", connection);
            vacuumCommand.ExecuteNonQuery();
            vacuumCommand.Dispose();

            connection.Close();
            connection.Dispose();
            GC.Collect();
            GC.WaitForPendingFinalizers();

            this.ProgressValue = 0;
        }

        private void AnalyzeLittleNavmapMSFS(string fileName)
        {
            Debug.WriteLine($"Analyzing SQLite database {fileName}...");
            this.LogText += $"Analyzing SQLite database {fileName}...\r\n";
            var connection = new SQLiteConnection($"URI=file:{fileName};Read Only=true");
            connection.Open();

            // Airports
            var airportCountCommand = new SQLiteCommand("SELECT COUNT(ident) FROM airport", connection);
            if (airportCountCommand.ExecuteScalar() is not long airportCount)
            {
                throw new Exception("Selected sqlite database contains 0 airports or count failed");
            }

            this.LogText += $"Selected sqlite database contains {airportCount} airports\r\n";

            // Runways
            var runwayCountCommand = new SQLiteCommand("SELECT COUNT(runway_id) FROM runway", connection);
            if (runwayCountCommand.ExecuteScalar() is not long runwayCount)
            {
                throw new Exception("Selected sqlite database contains 0 runways or count failed");
            }

            this.LogText += $"Selected sqlite database contains {runwayCount} runways\r\n";

            // Runway ends
            var runwayEndCountCommand = new SQLiteCommand("SELECT COUNT(runway_end_id) FROM runway_end", connection);
            if (runwayEndCountCommand.ExecuteScalar() is not long runwayEndCount)
            {
                throw new Exception("Selected sqlite database contains 0 runway ends or count failed");
            }

            this.LogText += $"Selected sqlite database contains {runwayEndCount} runway ends\r\n";

            // Approaches
            var approachCountCommand = new SQLiteCommand("SELECT COUNT(approach_id) FROM approach", connection);
            if (approachCountCommand.ExecuteScalar() is not long approachCount)
            {
                throw new Exception("Selected sqlite database contains 0 approaches or count failed");
            }

            this.LogText += $"Selected sqlite database contains {approachCount} approaches\r\n";

            airportCountCommand.Dispose();
            runwayCountCommand.Dispose();
            runwayEndCountCommand.Dispose();
            approachCountCommand.Dispose();
            connection.Close();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the browse little navmap msfs command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand BrowseLittleNavmapMSFSCommand { get; }
    }
}

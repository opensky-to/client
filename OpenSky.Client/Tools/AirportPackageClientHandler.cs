// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AirportPackageClientHandler.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Tools
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;

    using JetBrains.Annotations;

    using OpenSky.AirportsJSON;

    using OpenSkyApi;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Airport package client handler.
    /// </summary>
    /// <remarks>
    /// sushi.at, 21/09/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public static class AirportPackageClientHandler
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The cached package.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private static AirportPackage cachedPackage;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Downloads the updated client airport package if we don't have one or the server has a newer
        /// one.
        /// </summary>
        /// <remarks>
        /// sushi.at, 21/09/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public static void DownloadPackage()
        {
            if (!IsPackageUpToDate())
            {
                var result = OpenSkyService.Instance.GetAirportClientPackageAsync().Result;
                if (result.IsError)
                {
                    Debug.WriteLine("Error retrieving airport client package: " + result.Message);
                    if (!string.IsNullOrEmpty(result.ErrorDetails))
                    {
                        Debug.WriteLine(result.ErrorDetails);
                    }

                    throw new Exception("Error retrieving airport client package: " + result.Message);
                }

                // Check if we can deserialize it
                var package = AirportPackage.FromPackageJson(result.Data.Package);

                var openSkyFolder = Environment.ExpandEnvironmentVariables("%localappdata%\\OpenSky");
                if (!Directory.Exists(openSkyFolder))
                {
                    Directory.CreateDirectory(openSkyFolder);
                }

                var packagePath = Environment.ExpandEnvironmentVariables("%localappdata%\\OpenSky\\airports.json");
                File.WriteAllText(packagePath, result.Data.Package);
                cachedPackage = package;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Loads and returns the airport client package data from the local disk.
        /// </summary>
        /// <remarks>
        /// sushi.at, 21/09/2021.
        /// </remarks>
        /// <returns>
        /// The package loaded from disk.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [CanBeNull]
        public static AirportPackage GetPackage()
        {
            if (cachedPackage != null)
            {
                return cachedPackage;
            }

            if (!IsPackageAvailable())
            {
                throw new Exception("No local client airport package file available.");
            }

            try
            {
                var packagePath = Environment.ExpandEnvironmentVariables("%localappdata%\\OpenSky\\airports.json");
                var json = File.ReadAllText(packagePath);

                var package = AirportPackage.FromPackageJson(json);
                cachedPackage = package ?? throw new Exception("Error deserializing airport client package JSON.");
                return package;
            }
            catch (FormatException ex)
            {
                if (ex.Message?.ToLowerInvariant().Contains("non-base 64") == true)
                {
                    return null;
                }

                throw;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading local airport package file: {ex}");
                throw;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Is the local package available?
        /// </summary>
        /// <remarks>
        /// sushi.at, 21/09/2021.
        /// </remarks>
        /// <returns>
        /// True if the package is available, false if not.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public static bool IsPackageAvailable()
        {
            var packagePath = Environment.ExpandEnvironmentVariables("%localappdata%\\OpenSky\\airports.json");

            return File.Exists(packagePath) && new FileInfo(packagePath).Length > 0;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Is the current airport client package on the disk up-to-date?
        /// </summary>
        /// <remarks>
        /// sushi.at, 21/09/2021.
        /// </remarks>
        /// <returns>
        /// True if package is up-to-date, false if not.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public static bool IsPackageUpToDate()
        {
            if (!IsPackageAvailable())
            {
                return false;
            }

            try
            {
                var currentPackage = GetPackage();
                var result = OpenSkyService.Instance.GetAirportClientPackageHashAsync().Result;
                if (result.IsError)
                {
                    Debug.WriteLine("Error retrieving airport client package hash: " + result.Message);
                    if (!string.IsNullOrEmpty(result.ErrorDetails))
                    {
                        Debug.WriteLine(result.ErrorDetails);
                    }

                    throw new Exception("Error retrieving airport client package hash: " + result.Message);
                }

                var serverHash = result.Data;
                return string.Equals(currentPackage?.Hash, serverHash, StringComparison.InvariantCultureIgnoreCase);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error checking for updated airport client package hash: {ex}");
                throw;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Validates the specified airport icao.
        /// </summary>
        /// <remarks>
        /// sushi.at, 10/11/2021.
        /// </remarks>
        /// <param name="icao">
        /// The icao.
        /// </param>
        /// <returns>
        /// A ValidationResult.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public static ValidationResult ValidateAirportICAO(string icao)
        {
            try
            {
                if (!string.IsNullOrEmpty(icao))
                {
                    var localAirportPackage = GetPackage();
                    var airport = localAirportPackage?.Airports.SingleOrDefault(a => a.ICAO == icao.ToUpper(CultureInfo.InvariantCulture));
                    if (airport == null)
                    {
                        return new ValidationResult($"No airport with ICAO code {icao}");
                    }
                }
            }
            catch (Exception ex)
            {
                return new ValidationResult(ex.Message);
            }

            return ValidationResult.Success;
        }
    }
}
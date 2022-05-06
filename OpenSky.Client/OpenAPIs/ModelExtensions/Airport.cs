// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Airport.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace

namespace OpenSkyApi
{
    using System.Globalization;
    using System.Linq;

    using OpenSky.Client.Converters;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Airport extensions.
    /// </summary>
    /// <remarks>
    /// sushi.at, 17/01/2022.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public partial class Airport
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// (Immutable) the runway surface converter.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private static readonly RunwaySurfaceConverter RunwaySurfaceConverter = new();

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// (Immutable) the settings unit converter.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private static readonly SettingsUnitConverter SettingsUnitConverter = new();

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets a value indicating whether this airport has an IFR approach.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool HasIFRApproach => this.Approaches?.Count > 0;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets a value indicating whether this airport has an ILS approach.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool HasILSApproach => this.Approaches?.Any(a => a.Type.Contains("ILS")) ?? false;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets a value indicating whether this airport has runway lights.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool HasRunwayLights => this.Runways?.Any(r => !string.IsNullOrEmpty(r.CenterLight) || !string.IsNullOrEmpty(r.EdgeLight)) ?? false;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets information describing the runways.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string RunwayInfo
        {
            get
            {
                var info = string.Empty;
                if (this.Runways == null)
                {
                    return string.Empty;
                }

                foreach (var runway in this.Runways.Where(r => r.RunwayEnds.Count == 2))
                {
                    var end = runway.RunwayEnds.OrderBy(e => e.Name).ToArray();
                    var name = $"{end[0].Name}/{end[1].Name}";
                    var length = SettingsUnitConverter.Convert(runway.Length, typeof(string), "shortdistance|F0|true", CultureInfo.InvariantCulture)?.ToString() ?? "???";
                    var surface = RunwaySurfaceConverter.Convert(runway.Surface, typeof(string), null, CultureInfo.InvariantCulture)?.ToString() ?? "Unknown";
                    var lights = string.Empty;
                    if (!string.IsNullOrEmpty(runway.EdgeLight))
                    {
                        lights += $"Edge:{runway.EdgeLight} ";
                    }

                    if (!string.IsNullOrEmpty(runway.CenterLight))
                    {
                        lights += $"Ctr:{runway.CenterLight}";
                    }

                    info += $"Rwy {name,-7} | {length,-8} {surface,-10} | {lights}\r\n";

                    foreach (var runwayEnd in end)
                    {
                        var approachLights = !string.IsNullOrEmpty(runwayEnd.ApproachLightSystem) ? $"{runwayEnd.ApproachLightSystem,-7} " : string.Empty;
                        var vasi = string.Empty;
                        if (!string.IsNullOrEmpty(runwayEnd.LeftVasiType))
                        {
                            vasi = $"{runwayEnd.LeftVasiType}";
                            if (runwayEnd.LeftVasiPitch.HasValue)
                            {
                                vasi += $"_{runwayEnd.LeftVasiPitch.Value:F1}°";
                            }
                        }

                        if (string.IsNullOrEmpty(vasi) && !string.IsNullOrEmpty(runwayEnd.RightVasiType))
                        {
                            vasi = $"{runwayEnd.RightVasiType}";
                            if (runwayEnd.RightVasiPitch.HasValue)
                            {
                                vasi += $"_{runwayEnd.RightVasiPitch.Value:F1}°";
                            }
                        }

                        var closed = runwayEnd.HasClosedMarkings ? "CLOSED" : string.Empty;

                        info += $" -- {runwayEnd.Name,-3}: {approachLights}{vasi}{closed}\r\n";
                    }
                }

                return info;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// S2 geometry cell ID for level 3.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ulong S2Cell3Id => ulong.Parse(this.S2Cell3);

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// S2 geometry cell ID for level 4.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ulong S2Cell4Id => ulong.Parse(this.S2Cell4);

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// S2 geometry cell ID for level 5.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ulong S2Cell5Id => ulong.Parse(this.S2Cell5);

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// S2 geometry cell ID for level 6.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ulong S2Cell6Id => ulong.Parse(this.S2Cell6);

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// S2 geometry cell ID for level 7.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ulong S2Cell7Id => ulong.Parse(this.S2Cell7);

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// S2 geometry cell ID for level 8.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ulong S2Cell8Id => ulong.Parse(this.S2Cell8);

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// S2 geometry cell ID for level 9.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ulong S2Cell9Id => ulong.Parse(this.S2Cell9);

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the simulators info string.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string Simulators
        {
            get
            {
                var sims = string.Empty;
                if (this.Msfs)
                {
                    sims += "MSFS, ";
                }

                if (this.XP11)
                {
                    sims += "XP11, ";
                }

                return sims.TrimEnd(' ', ',');
            }
        }
    }
}
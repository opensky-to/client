// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RunwaySurface.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.OpenAPIs.ModelExtensions
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Runway surface enum.
    /// </summary>
    /// <remarks>
    /// sushi.at, 11/06/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public enum RunwaySurface
    {
        /// <summary>
        /// Asphalt "A" (Hard)
        /// </summary>
        Asphalt,

        /// <summary>
        /// Bituminous "B" (Hard)
        /// </summary>
        Bituminous,

        /// <summary>
        /// Brick "BR" (Soft)
        /// </summary>
        Brick,

        /// <summary>
        /// Cement "CE" (Hard)
        /// </summary>
        Cement,

        /// <summary>
        /// Clay "CL" (Soft)
        /// </summary>
        Clay,

        /// <summary>
        /// Concrete "C" (Hard)
        /// </summary>
        Concrete,

        /// <summary>
        /// Coral "CR" (Soft)
        /// </summary>
        Coral,

        /// <summary>
        /// Dirt "D" (Soft)
        /// </summary>
        Dirt,

        /// <summary>
        /// Macadam "M" (Soft)
        /// </summary>
        Macadam,

        /// <summary>
        /// Oil treated "OT" (Soft)
        /// </summary>
        OilTreated,

        /// <summary>
        /// Grass "G" (Soft)
        /// </summary>
        Grass,

        /// <summary>
        /// Gravel "GR" (Soft)
        /// </summary>
        Gravel,

        /// <summary>
        /// Ice "I" (Soft)
        /// </summary>
        Ice,

        /// <summary>
        /// Planks "PL" (Soft)
        /// </summary>
        Planks,

        /// <summary>
        /// Sand "S" (Soft)
        /// </summary>
        Sand,

        /// <summary>
        /// Shale "SH" (Soft)
        /// </summary>
        Shale,

        /// <summary>
        /// Snow "SN" (Soft)
        /// </summary>
        Snow,

        /// <summary>
        /// Steel mats "SM" (Soft)
        /// </summary>
        SteelMats,

        /// <summary>
        /// Tarmac "T" (Hard)
        /// </summary>
        Tarmac,

        /// <summary>
        /// Unknown "UNKNOWN" (Soft)
        /// </summary>
        Unknown,

        /// <summary>
        /// Water "W" (Soft)
        /// </summary>
        Water
    }

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Runway surface extension methods.
    /// </summary>
    /// <remarks>
    /// sushi.at, 11/06/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public static class RunwaySurfaceExtensions
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// A RunwaySurface extension method that checks if the surface is a hard surface (suited for Airliners).
        /// </summary>
        /// <remarks>
        /// sushi.at, 11/06/2021.
        /// </remarks>
        /// <param name="surface">
        /// The surface enum value.
        /// </param>
        /// <returns>
        /// True if hard surface, false if soft.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public static bool IsHardSurface(this RunwaySurface surface)
        {
            return surface is RunwaySurface.Asphalt or RunwaySurface.Bituminous or RunwaySurface.Cement or RunwaySurface.Concrete or RunwaySurface.Tarmac;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// A string extension method that parses the runway surface from the string in the database to the enumeration.
        /// </summary>
        /// <remarks>
        /// sushi.at, 11/06/2021.
        /// </remarks>
        /// <param name="surface">
        /// The surface string to parse.
        /// </param>
        /// <returns>
        /// The matching RunwaySurface enum value or Unknown.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public static RunwaySurface ParseRunwaySurface(this string surface)
        {
            return surface switch
            {
                "A" => RunwaySurface.Asphalt,
                "B" => RunwaySurface.Bituminous,
                "BR" => RunwaySurface.Brick,
                "CE" => RunwaySurface.Cement,
                "CL" => RunwaySurface.Clay,
                "C" => RunwaySurface.Concrete,
                "CR" => RunwaySurface.Coral,
                "D" => RunwaySurface.Dirt,
                "M" => RunwaySurface.Macadam,
                "OT" => RunwaySurface.OilTreated,
                "G" => RunwaySurface.Grass,
                "GR" => RunwaySurface.Gravel,
                "I" => RunwaySurface.Ice,
                "PL" => RunwaySurface.Planks,
                "S" => RunwaySurface.Sand,
                "SH" => RunwaySurface.Shale,
                "SN" => RunwaySurface.Snow,
                "SM" => RunwaySurface.SteelMats,
                "T" => RunwaySurface.Tarmac,
                "W" => RunwaySurface.Water,
                _ => RunwaySurface.Unknown
            };
        }
    }
}
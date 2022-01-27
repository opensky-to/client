// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OrdinalsExtension.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Tools
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// The ordinals extension extension method for integers.
    /// </summary>
    /// <remarks>
    /// sushi.at, 23/11/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public static class OrdinalsExtension
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// An int extension method that returns the ordinal for the given number.
        /// </summary>
        /// <remarks>
        /// sushi.at, 23/11/2021.
        /// </remarks>
        /// <param name="number">
        /// The number to act on.
        /// </param>
        /// <returns>
        /// A string containing the ordinal.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public static string Ordinal(this int number)
        {
            var work = number.ToString();
            if (number % 100 is 11 or 12 or 13)
            {
                return work + "th";
            }

            work += (number % 10) switch
            {
                1 => "st",
                2 => "nd",
                3 => "rd",
                _ => "th"
            };

            return work;
        }
    }
}
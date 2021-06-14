// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OpenSkyService.cs" company="OpenSky">
// sushi.at for OpenSky 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace

namespace OpenSkyApi
{
    using System.Net.Http;

    using OpenSky.Client.Properties;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// OpenSky API service client.
    /// </summary>
    /// <remarks>
    /// sushi.at, 01/06/2021.
    /// </remarks>
    /// <seealso cref="T:OpenSkyApi.OpenSkyServiceBase"/>
    /// -------------------------------------------------------------------------------------------------
    public partial class OpenSkyService
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes static members of the <see cref="OpenSkyService"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 01/06/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        static OpenSkyService()
        {
            Instance = new OpenSkyService(new HttpClient());
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="OpenSkyService"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 01/06/2021.
        /// </remarks>
        /// <param name="httpClient">
        /// The HTTP client.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private OpenSkyService(HttpClient httpClient) : base(httpClient)
        {
            this.BaseUrl = Settings.Default.OpenSkyAPIUrl;
            this._httpClient = httpClient;
            this._settings = new System.Lazy<Newtonsoft.Json.JsonSerializerSettings>(this.CreateSerializerSettings);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the single static instance.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static OpenSkyService Instance { get; }
    }
}
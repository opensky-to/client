// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompressionExtensions.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Tools
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Text;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Compression extension methods.
    /// </summary>
    /// <remarks>
    /// sushi.at, 21/09/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public static class CompressionExtensions
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// A byte[] extension method that compresses the given data.
        /// </summary>
        /// <remarks>
        /// sushi.at, 21/09/2021.
        /// </remarks>
        /// <param name="data">
        /// The data to compress.
        /// </param>
        /// <returns>
        /// A byte[] containing the compressed data.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public static byte[] Compress(this byte[] data)
        {
            using var sourceStream = new MemoryStream(data);
            using var destinationStream = new MemoryStream();
            sourceStream.CompressTo(destinationStream);
            return destinationStream.ToArray();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// A Stream extension method that compresses from source to destination stream.
        /// </summary>
        /// <remarks>
        /// sushi.at, 21/09/2021.
        /// </remarks>
        /// <param name="stream">
        /// The stream to act on.
        /// </param>
        /// <param name="outputStream">
        /// Stream to write compressed data to.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public static void CompressTo(this Stream stream, Stream outputStream)
        {
            using var gZipStream = new GZipStream(outputStream, CompressionMode.Compress);
            stream.CopyTo(gZipStream);
            gZipStream.Flush();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Compress string to base64 encoded string using g-zip.
        /// </summary>
        /// <remarks>
        /// sushi.at, 21/09/2021.
        /// </remarks>
        /// <param name="data">
        /// The string to compress.
        /// </param>
        /// <returns>
        /// The compressed string.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public static string CompressToBase64(this string data)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(data).Compress());
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// A byte[] extension method that decompresses the given data.
        /// </summary>
        /// <remarks>
        /// sushi.at, 21/09/2021.
        /// </remarks>
        /// <param name="data">
        /// The data to decompress.
        /// </param>
        /// <returns>
        /// A byte[] containing the decompressed data.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public static byte[] Decompress(this byte[] data)
        {
            using var sourceStream = new MemoryStream(data);
            using var destinationStream = new MemoryStream();
            sourceStream.DecompressTo(destinationStream);
            return destinationStream.ToArray();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Decompress base64 string using g-zip.
        /// </summary>
        /// <remarks>
        /// sushi.at, 21/09/2021.
        /// </remarks>
        /// <param name="data">
        /// The string to decompress.
        /// </param>
        /// <returns>
        /// The decompressed source string.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public static string DecompressFromBase64(this string data)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(data).Decompress());
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// A Stream extension method that decompresses from source to destination stream.
        /// </summary>
        /// <remarks>
        /// sushi.at, 21/09/2021.
        /// </remarks>
        /// <param name="stream">
        /// The stream to act on.
        /// </param>
        /// <param name="outputStream">
        /// Stream to write decompressed data to.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public static void DecompressTo(this Stream stream, Stream outputStream)
        {
            using var gZipStream = new GZipStream(stream, CompressionMode.Decompress);
            gZipStream.CopyTo(outputStream);
        }
    }
}
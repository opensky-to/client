// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DateTimeTextWriterTraceListener.cs" company="OpenSky">
// sushi.at for OpenSky 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Tools
{
    using System;
    using System.Diagnostics;
    using System.IO;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Text writer trace listener that adds UTC date/time to each logged line.
    /// </summary>
    /// <remarks>
    /// sushi.at, 31/03/2021.
    /// </remarks>
    /// <seealso cref="T:System.Diagnostics.TextWriterTraceListener"/>
    /// -------------------------------------------------------------------------------------------------
    public class DateTimeTextWriterTraceListener : TextWriterTraceListener
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimeTextWriterTraceListener"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 31/03/2021.
        /// </remarks>
        /// <param name="stream">
        /// The stream.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public DateTimeTextWriterTraceListener(Stream stream) : base(stream)
        {
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Writes a message to this instance's
        /// <see cref="P:System.Diagnostics.TextWriterTraceListener.Writer" /> followed by a line
        /// terminator. The default line terminator is a carriage return followed by a line feed (\r\n).
        /// </summary>
        /// <remarks>
        /// sushi.at, 31/03/2021.
        /// </remarks>
        /// <param name="message">
        /// A message to write.
        /// </param>
        /// <seealso cref="M:System.Diagnostics.TextWriterTraceListener.WriteLine(string)"/>
        /// -------------------------------------------------------------------------------------------------
        public override void WriteLine(string message)
        {
            base.WriteLine($"{DateTime.UtcNow} | {message}");
        }
    }
}
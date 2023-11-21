// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AgentAutoLauncher.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Tools
{
    using System.Diagnostics;
    using System.IO;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Agent auto launcher.
    /// </summary>
    /// <remarks>
    /// sushi.at, 21/11/2023.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public static class AgentAutoLauncher
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Automatic launch the OpenSky flight tracking agent if the setting is enabled and it isn't already running.
        /// </summary>
        /// <remarks>
        /// sushi.at, 21/11/2023.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public static void AutoLaunchAgent()
        {
            if (Properties.Settings.Default.AutoLaunchAgent)
            {
                var agentExeLocation = Properties.Settings.Default.FlightTrackingAgentLocation;
                if (!string.IsNullOrEmpty(agentExeLocation) && File.Exists(agentExeLocation))
                {
                    var agentExeFile = Path.GetFileName(agentExeLocation);

                    var agentProcesses = Process.GetProcessesByName(agentExeFile.Replace(".exe", string.Empty));
                    if (agentProcesses.Length == 0)
                    {
                        Process.Start(agentExeLocation);
                    }
                }
            }
        }
    }
}
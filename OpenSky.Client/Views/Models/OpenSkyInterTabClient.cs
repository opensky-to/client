// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OpenSkyInterTabClient.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Views.Models
{
    using System.Windows;

    using Dragablz;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// The inter tab client.
    /// </summary>
    /// <remarks>
    /// sushi.at, 17/06/2021.
    /// </remarks>
    /// <seealso cref="T:Dragablz.IInterTabClient"/>
    /// -------------------------------------------------------------------------------------------------
    public class OpenSkyInterTabClient : IInterTabClient
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Provide a new host window so a tab can be teared from an existing window into a new window.
        /// </summary>
        /// <remarks>
        /// sushi.at, 17/06/2021.
        /// </remarks>
        /// <param name="interTabClient">
        /// The inter tab client.
        /// </param>
        /// <param name="partition">
        /// Provides the partition where the drag operation was initiated.
        /// </param>
        /// <param name="source">
        /// The source control where a dragging operation was initiated.
        /// </param>
        /// <returns>
        /// The new host.
        /// </returns>
        /// <seealso cref="M:Dragablz.IInterTabClient.GetNewHost(IInterTabClient,object,TabablzControl)"/>
        /// -------------------------------------------------------------------------------------------------
        public INewTabHost<Window> GetNewHost(IInterTabClient interTabClient, object partition, TabablzControl source)
        {
            var view = new Main();
            // todo set hamburger menu to collapsed on default for new windows
            return new NewTabHost<Window>(view, view.InitialTabablzControl);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Called when a tab has been emptied, and thus typically a window needs closing.
        /// </summary>
        /// <remarks>
        /// sushi.at, 17/06/2021.
        /// </remarks>
        /// <param name="tabControl">
        /// The tab control.
        /// </param>
        /// <param name="window">
        /// The window.
        /// </param>
        /// <returns>
        /// A TabEmptiedResponse.
        /// </returns>
        /// <seealso cref="M:Dragablz.IInterTabClient.TabEmptiedHandler(TabablzControl,Window)"/>
        /// -------------------------------------------------------------------------------------------------
        public TabEmptiedResponse TabEmptiedHandler(TabablzControl tabControl, Window window)
        {
            return TabEmptiedResponse.CloseWindowOrLayoutBranch;
        }
    }
}
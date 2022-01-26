// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FlightPlanViewModel.Payload.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Pages.Models
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Windows;

    using OpenSky.Client.Controls;
    using OpenSky.Client.Controls.Models;
    using OpenSky.Client.MVVM;
    using OpenSky.Client.Tools;
    using OpenSky.Client.Views;

    using OpenSkyApi;

    using TomsToolbox.Essentials;

    /// -------------------------------------------------------------------------------------------------
    /// <content>
    /// Flight plan view model - Payload.
    /// </content>
    /// -------------------------------------------------------------------------------------------------
    public partial class FlightPlanViewModel
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the other payloads.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<PlannablePayload> OtherPayloads { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the payloads planned for the flight.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<Guid> Payloads { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the plannable payloads.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<PlannablePayload> PayloadsAtOrigin { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the payloads on board.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<PlannablePayload> PayloadsOnBoard { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the payloads towards origin.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<PlannablePayload> PayloadsTowardsOrigin { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the payload weight in lbs.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double PayloadWeight
        {
            get
            {
                var totalPayload = 0.0;
                foreach (var payloadID in this.Payloads)
                {
                    var atOriginPayload = this.PayloadsAtOrigin.SingleOrDefault(p => p.Id == payloadID);
                    if (atOriginPayload != null)
                    {
                        totalPayload += atOriginPayload.Weight;
                    }

                    var towardsOriginPayload = this.PayloadsTowardsOrigin.SingleOrDefault(p => p.Id == payloadID);
                    if (towardsOriginPayload != null)
                    {
                        totalPayload += towardsOriginPayload.Weight;
                    }

                    var otherPayload = this.OtherPayloads.SingleOrDefault(p => p.Id == payloadID);
                    if (otherPayload != null)
                    {
                        totalPayload += otherPayload.Weight;
                    }
                }

                return totalPayload;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the refresh payloads command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand RefreshPayloadsCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Refreshes the list of plannable payloads.
        /// </summary>
        /// <remarks>
        /// sushi.at, 19/12/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void RefreshPayloads()
        {
            this.LoadingText = "Refreshing payloads...";
            try
            {
                var result = OpenSkyService.Instance.GetPlannablePayloadsAsync().Result;
                if (!result.IsError)
                {
                    this.RefreshPayloadsCommand.ReportProgress(
                        () =>
                        {
                            this.PayloadsOnBoard.Clear();
                            this.PayloadsAtOrigin.Clear();
                            this.PayloadsTowardsOrigin.Clear();
                            this.OtherPayloads.Clear();
                            this.OtherPayloads.AddRange(result.Data);
                            this.UpdatePlannablePayloads();
                        });
                }
                else
                {
                    this.RefreshPayloadsCommand.ReportProgress(
                        () =>
                        {
                            Debug.WriteLine("Error refreshing payloads: " + result.Message);
                            if (!string.IsNullOrEmpty(result.ErrorDetails))
                            {
                                Debug.WriteLine(result.ErrorDetails);
                            }

                            var notification = new OpenSkyNotification("Error refreshing payloads", result.Message, MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                            notification.SetErrorColorStyle();
                            Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.ViewReference, this.RefreshPayloadsCommand, "Error refreshing payloads");
            }
            finally
            {
                this.LoadingText = null;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Update plannable payloads distribution depending on selected Origin.
        /// </summary>
        /// <remarks>
        /// sushi.at, 19/12/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void UpdatePlannablePayloads()
        {
            var payloads = new List<PlannablePayload>(this.PayloadsOnBoard);
            payloads.AddRange(this.PayloadsAtOrigin);
            payloads.AddRange(this.PayloadsTowardsOrigin);
            payloads.AddRange(this.OtherPayloads);
            this.PayloadsOnBoard.Clear();
            this.PayloadsAtOrigin.Clear();
            this.PayloadsTowardsOrigin.Clear();
            this.OtherPayloads.Clear();

            if (string.IsNullOrEmpty(this.OriginICAO) && this.SelectedAircraft == null)
            {
                // No origin, no aircraft
                this.OtherPayloads.AddRange(payloads);
            }
            else
            {
                if (this.SelectedAircraft == null)
                {
                    // Origin selected, but no aircraft
                    this.PayloadsAtOrigin.AddRange(payloads.Where(p => p.CurrentLocation == this.OriginICAO));
                    this.PayloadsTowardsOrigin.AddRange(payloads.Where(p => p.CurrentLocation != this.OriginICAO && p.Destinations.Contains(this.OriginICAO)));
                    this.OtherPayloads.AddRange(payloads.Where(p => p.CurrentLocation != this.OriginICAO && !p.Destinations.Contains(this.OriginICAO)));
                }
                else if (string.IsNullOrEmpty(this.OriginICAO))
                {
                    // Aircraft selected, but no origin
                    this.PayloadsOnBoard.AddRange(payloads.Where(p => p.CurrentLocation == this.SelectedAircraft.Registry));
                    this.OtherPayloads.AddRange(payloads.Where(p => p.CurrentLocation != this.SelectedAircraft.Registry));
                }
                else
                {
                    // Both origin and aircraft selected
                    this.PayloadsOnBoard.AddRange(payloads.Where(p => p.CurrentLocation == this.SelectedAircraft.Registry));
                    this.PayloadsAtOrigin.AddRange(payloads.Where(p => p.CurrentLocation == this.OriginICAO));
                    this.PayloadsTowardsOrigin.AddRange(payloads.Where(p => p.CurrentLocation != this.OriginICAO && p.Destinations.Contains(this.OriginICAO)));
                    this.OtherPayloads.AddRange(payloads.Where(p => p.CurrentLocation != this.OriginICAO && p.CurrentLocation != this.SelectedAircraft.Registry && !p.Destinations.Contains(this.OriginICAO)));
                }
            }
        }
    }
}
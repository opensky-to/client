// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FinancialOverviewViewModel.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Pages.Models
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Windows;

    using OpenSky.Client.Controls;
    using OpenSky.Client.Controls.Models;
    using OpenSky.Client.MVVM;
    using OpenSky.Client.Tools;
    using OpenSky.Client.Views;

    using OpenSkyApi;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Financial overview view model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 26/01/2022.
    /// </remarks>
    /// <seealso cref="T:OpenSky.Client.MVVM.ViewModel"/>
    /// -------------------------------------------------------------------------------------------------
    public class FinancialOverviewViewModel : ViewModel
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The loading text.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string loadingText;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The overview.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private FinancialOverview overview;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="FinancialOverviewViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 26/01/2022.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public FinancialOverviewViewModel()
        {
            // Create commands
            this.RefreshCommand = new AsynchronousCommand(this.Refresh);
            this.BobsYourUncleCommand = new AsynchronousCommand(this.BobsYourUncle);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the bob's your uncle command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand BobsYourUncleCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the cash flow.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Dictionary<DateTime, long> CashFlow
        {
            get
            {
                var cashFlow = new Dictionary<DateTime, long>();
                var today = DateTime.UtcNow.Date;
                for (var i = 0; i < 30; i++)
                {
                    cashFlow.Add(today.AddDays(-1 * i), 0);
                }

                if (this.Overview?.RecentFinancialRecords != null)
                {
                    foreach (var financialRecord in this.Overview.RecentFinancialRecords)
                    {
                        if (financialRecord.ChildRecords.Count == 0)
                        {
                            if (!cashFlow.ContainsKey(financialRecord.Timestamp.UtcDateTime.Date))
                            {
                                cashFlow.Add(financialRecord.Timestamp.UtcDateTime.Date, 0);
                            }

                            cashFlow[financialRecord.Timestamp.UtcDateTime.Date] += financialRecord.Income;
                            cashFlow[financialRecord.Timestamp.UtcDateTime.Date] -= financialRecord.Expense;
                        }
                        else
                        {
                            foreach (var childRecord in financialRecord.ChildRecords)
                            {
                                if (!cashFlow.ContainsKey(financialRecord.Timestamp.UtcDateTime.Date))
                                {
                                    cashFlow.Add(financialRecord.Timestamp.UtcDateTime.Date, 0);
                                }

                                cashFlow[financialRecord.Timestamp.UtcDateTime.Date] += childRecord.Income;
                                cashFlow[financialRecord.Timestamp.UtcDateTime.Date] -= childRecord.Expense;
                            }
                        }
                    }
                }

                return cashFlow;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the cash flow expense.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Dictionary<DateTime, long> CashFlowExpense
        {
            get
            {
                var cashFlow = new Dictionary<DateTime, long>();
                var today = DateTime.UtcNow.Date;
                for (var i = 0; i < 30; i++)
                {
                    cashFlow.Add(today.AddDays(-1 * i), 0);
                }

                if (this.Overview?.RecentFinancialRecords != null)
                {
                    foreach (var financialRecord in this.Overview.RecentFinancialRecords)
                    {
                        if (financialRecord.ChildRecords.Count == 0)
                        {
                            if (financialRecord.Expense != 0)
                            {
                                if (!cashFlow.ContainsKey(financialRecord.Timestamp.UtcDateTime.Date))
                                {
                                    cashFlow.Add(financialRecord.Timestamp.UtcDateTime.Date, 0);
                                }

                                cashFlow[financialRecord.Timestamp.UtcDateTime.Date] -= financialRecord.Expense;
                            }
                        }
                        else
                        {
                            foreach (var childRecord in financialRecord.ChildRecords)
                            {
                                if (childRecord.Expense != 0)
                                {
                                    if (!cashFlow.ContainsKey(financialRecord.Timestamp.UtcDateTime.Date))
                                    {
                                        cashFlow.Add(financialRecord.Timestamp.UtcDateTime.Date, 0);
                                    }

                                    cashFlow[financialRecord.Timestamp.UtcDateTime.Date] -= childRecord.Expense;
                                }
                            }
                        }
                    }
                }

                return cashFlow;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the cash flow income.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Dictionary<DateTime, long> CashFlowIncome
        {
            get
            {
                var cashFlow = new Dictionary<DateTime, long>();
                var today = DateTime.UtcNow.Date;
                for (var i = 0; i < 30; i++)
                {
                    cashFlow.Add(today.AddDays(-1 * i), 0);
                }

                if (this.Overview?.RecentFinancialRecords != null)
                {
                    foreach (var financialRecord in this.Overview.RecentFinancialRecords)
                    {
                        if (financialRecord.ChildRecords.Count == 0)
                        {
                            if (financialRecord.Income != 0)
                            {
                                if (!cashFlow.ContainsKey(financialRecord.Timestamp.UtcDateTime.Date))
                                {
                                    cashFlow.Add(financialRecord.Timestamp.UtcDateTime.Date, 0);
                                }

                                cashFlow[financialRecord.Timestamp.UtcDateTime.Date] += financialRecord.Income;
                            }
                        }
                        else
                        {
                            foreach (var childRecord in financialRecord.ChildRecords)
                            {
                                if (childRecord.Income != 0)
                                {
                                    if (!cashFlow.ContainsKey(financialRecord.Timestamp.UtcDateTime.Date))
                                    {
                                        cashFlow.Add(financialRecord.Timestamp.UtcDateTime.Date, 0);
                                    }

                                    cashFlow[financialRecord.Timestamp.UtcDateTime.Date] += childRecord.Income;
                                }
                            }
                        }
                    }
                }

                return cashFlow;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the expense distribution.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public List<PieChartValue> ExpenseDistribution
        {
            get
            {
                var distribution = new Dictionary<string, long>();
                if (this.Overview?.RecentFinancialRecords != null)
                {
                    foreach (var financialRecord in this.Overview.RecentFinancialRecords)
                    {
                        if (financialRecord.ChildRecords.Count == 0)
                        {
                            if (financialRecord.Expense != 0)
                            {
                                if (!distribution.ContainsKey(financialRecord.Category.ToString()))
                                {
                                    distribution.Add(financialRecord.Category.ToString(), 0);
                                }

                                distribution[financialRecord.Category.ToString()] += financialRecord.Expense;
                            }
                        }
                        else
                        {
                            foreach (var childRecord in financialRecord.ChildRecords)
                            {
                                if (childRecord.Expense != 0)
                                {
                                    if (!distribution.ContainsKey(childRecord.Category.ToString()))
                                    {
                                        distribution.Add(childRecord.Category.ToString(), 0);
                                    }

                                    distribution[childRecord.Category.ToString()] += childRecord.Expense;
                                }
                            }
                        }
                    }
                }

                return distribution.Select(value => new PieChartValue { Key = value.Key, Value = value.Value }).ToList();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the income distribution.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public List<PieChartValue> IncomeDistribution
        {
            get
            {
                var distribution = new Dictionary<string, long>();
                if (this.Overview?.RecentFinancialRecords != null)
                {
                    foreach (var financialRecord in this.Overview.RecentFinancialRecords)
                    {
                        if (financialRecord.ChildRecords.Count == 0)
                        {
                            if (financialRecord.Income != 0)
                            {
                                if (!distribution.ContainsKey(financialRecord.Category.ToString()))
                                {
                                    distribution.Add(financialRecord.Category.ToString(), 0);
                                }

                                distribution[financialRecord.Category.ToString()] += financialRecord.Income;
                            }
                        }
                        else
                        {
                            foreach (var childRecord in financialRecord.ChildRecords)
                            {
                                if (childRecord.Income != 0)
                                {
                                    if (!distribution.ContainsKey(childRecord.Category.ToString()))
                                    {
                                        distribution.Add(childRecord.Category.ToString(), 0);
                                    }

                                    distribution[childRecord.Category.ToString()] += childRecord.Income;
                                }
                            }
                        }
                    }
                }

                return distribution.Select(value => new PieChartValue { Key = value.Key, Value = value.Value }).ToList();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the loading text.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string LoadingText
        {
            get => this.loadingText;

            set
            {
                if (Equals(this.loadingText, value))
                {
                    return;
                }

                this.loadingText = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the overview.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public FinancialOverview Overview
        {
            get => this.overview;

            set
            {
                if (Equals(this.overview, value))
                {
                    return;
                }

                this.overview = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged(nameof(this.IncomeDistribution));
                this.NotifyPropertyChanged(nameof(this.ExpenseDistribution));
                this.NotifyPropertyChanged(nameof(this.CashFlow));
                this.NotifyPropertyChanged(nameof(this.CashFlowIncome));
                this.NotifyPropertyChanged(nameof(this.CashFlowExpense));
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the refresh command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand RefreshCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Free cash!
        /// </summary>
        /// <remarks>
        /// sushi.at, 27/01/2022.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void BobsYourUncle()
        {
            this.LoadingText = "Printing money...";
            try
            {
                var result = OpenSkyService.Instance.BobsYourUncleAsync().Result;
                if (!result.IsError)
                {
                    this.BobsYourUncleCommand.ReportProgress(() => this.RefreshCommand.DoExecute(null));
                }
                else
                {
                    this.BobsYourUncleCommand.ReportProgress(
                        () =>
                        {
                            Debug.WriteLine("Error printing money: " + result.Message);
                            if (!string.IsNullOrEmpty(result.ErrorDetails))
                            {
                                Debug.WriteLine(result.ErrorDetails);
                            }

                            var notification = new OpenSkyNotification("Error printing money", result.Message, MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                            notification.SetErrorColorStyle();
                            Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.ViewReference, this.BobsYourUncleCommand, "Error printing money");
            }
            finally
            {
                this.LoadingText = null;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Refresh the financial overview.
        /// </summary>
        /// <remarks>
        /// sushi.at, 26/01/2022.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void Refresh()
        {
            this.LoadingText = "Refreshing financial overview...";
            try
            {
                var result = OpenSkyService.Instance.GetFinancialOverviewAsync().Result;
                if (!result.IsError)
                {
                    // Sort the sub-records as the grid refuses to do it
                    foreach (var financialRecord in result.Data.RecentFinancialRecords)
                    {
                        if (financialRecord.ChildRecords?.Count > 0)
                        {
                            financialRecord.ChildRecords = financialRecord.ChildRecords.OrderByDescending(r => r.Timestamp).ToList();
                        }
                    }

                    this.Overview = result.Data;
                }
                else
                {
                    this.RefreshCommand.ReportProgress(
                        () =>
                        {
                            Debug.WriteLine("Error refreshing financial overview: " + result.Message);
                            if (!string.IsNullOrEmpty(result.ErrorDetails))
                            {
                                Debug.WriteLine(result.ErrorDetails);
                            }

                            var notification = new OpenSkyNotification("Error refreshing financial overview", result.Message, MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                            notification.SetErrorColorStyle();
                            Main.ShowNotificationInSameViewAs(this.ViewReference, notification);
                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.ViewReference, this.RefreshCommand, "Error refreshing financial overview");
            }
            finally
            {
                this.LoadingText = null;
            }
        }
    }
}
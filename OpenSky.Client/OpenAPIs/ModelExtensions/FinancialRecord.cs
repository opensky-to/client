// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FinancialRecord.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace

namespace OpenSkyApi;

using System.Linq;

/// -------------------------------------------------------------------------------------------------
/// <summary>
/// Financial record extensions.
/// </summary>
/// <remarks>
/// sushi.at, 01/02/2022.
/// </remarks>
/// -------------------------------------------------------------------------------------------------
public partial class FinancialRecord
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Gets the expense combined.
    /// </summary>
    /// -------------------------------------------------------------------------------------------------
    public long ExpenseCombined => this.ChildRecords.Count == 0 ? this.Expense : this.ChildRecords.Sum(cr => cr.Expense);

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Gets the income combined.
    /// </summary>
    /// -------------------------------------------------------------------------------------------------
    public long IncomeCombined => this.ChildRecords.Count == 0 ? this.Income : this.ChildRecords.Sum(cr => cr.Income);
}
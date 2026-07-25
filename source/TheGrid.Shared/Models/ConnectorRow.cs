// <copyright file="ConnectorRow.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

namespace TheGrid.Shared.Models
{
    /// <summary>
    /// A single row streamed from a connector while a query is executing, along with the column metadata for the result set.
    /// </summary>
    /// <param name="Columns">Column metadata for the result set. The same reference is attached to every row in a given stream.</param>
    /// <param name="Data">Data for this row, keyed by column name.</param>
    public sealed record ConnectorRow(IReadOnlyDictionary<string, QueryResultColumn> Columns, IReadOnlyDictionary<string, object?> Data);
}

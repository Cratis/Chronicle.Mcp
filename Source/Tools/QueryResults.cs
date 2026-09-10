// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Contracts.Queries;

namespace Cratis.Chronicle.Mcp.Tools;

/// <summary>
/// Helpers for unwrapping query results from the Chronicle contracts.
/// </summary>
public static class QueryResults
{
    /// <summary>
    /// Ensures a query result succeeded and returns its data.
    /// </summary>
    /// <param name="result">The result to unwrap.</param>
    /// <typeparam name="TData">The type of data in the result.</typeparam>
    /// <returns>The data.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the query did not succeed.</exception>
    public static TData Unwrap<TData>(QueryResult<TData> result)
    {
        if (!result.IsSuccess)
        {
            throw new InvalidOperationException($"Query failed: {string.Join(", ", result.ExceptionMessages)}");
        }

        return result.Data;
    }
}

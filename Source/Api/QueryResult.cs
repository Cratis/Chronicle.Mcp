// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Api;

/// <summary>
/// Represents the envelope returned by Chronicle query endpoints.
/// </summary>
/// <typeparam name="T">The type of data items in the result.</typeparam>
/// <param name="Data">The collection of result items.</param>
/// <param name="IsSuccess">Whether the query was successful.</param>
public record QueryResult<T>(IEnumerable<T> Data, bool IsSuccess);

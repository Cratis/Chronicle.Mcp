// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Mcp.Authorization;

/// <summary>
/// Identifies the argument scoped by a mutation grant.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public sealed class AuthorizationTargetAttribute : Attribute;

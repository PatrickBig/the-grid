// <copyright file="ConnectionTestResult.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

namespace TheGrid.Connectors
{
    /// <summary>
    /// Describes the outcome of a connection test.
    /// </summary>
    /// <param name="Success">Indicates whether the connection test succeeded.</param>
    /// <param name="Message">A message describing the outcome, typically populated with failure details when <paramref name="Success"/> is <see langword="false"/>.</param>
    /// <param name="Elapsed">The amount of time the connection test took to complete.</param>
    public sealed record ConnectionTestResult(bool Success, string? Message, TimeSpan Elapsed);
}

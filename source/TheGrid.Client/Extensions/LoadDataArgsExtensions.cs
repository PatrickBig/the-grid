﻿// <copyright file="LoadDataArgsExtensions.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Radzen;
using System.Net;

namespace TheGrid.Client.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="LoadDataArgs"/>.
    /// </summary>
    public static class LoadDataArgsExtensions
    {
        /// <summary>
        /// Builds a query to append to the path in a URL. Values will be URL encoded for saftey.
        /// This will automatically include the skip and take values from the request.
        /// </summary>
        /// <param name="e">Argument to create the query from.</param>
        /// <param name="path">Path to the resource.</param>
        /// <param name="extraParameters">Extra parameters to add to the URL.</param>
        /// <returns>A full path with query to be used in an HTTP request.</returns>
        public static string GetQueryUrl(this LoadDataArgs e, string path, Dictionary<string, string>? extraParameters = null)
        {
            var parameters = extraParameters ?? new Dictionary<string, string>();

            if (e.Skip != null)
            {
                parameters.Add("skip", e.Skip.ToString() ?? "0");
            }

            if (e.Top != null)
            {
                parameters.Add("take", e.Top!.ToString() ?? "25");
            }

            return path.TrimEnd('/') + "?" + GetQueryString(parameters);
        }

        private static string GetQueryString(Dictionary<string, string> parameters)
        {
            return string.Join("&", parameters.Select(p => WebUtility.UrlEncode(p.Key) + "=" + WebUtility.UrlEncode(p.Value)));
        }
    }
}

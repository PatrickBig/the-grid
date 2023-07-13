// <copyright file="LoadDataArgsExtensions.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Radzen;
using System.Net;

namespace TheGrid.Client.Extensions
{
    public static class LoadDataArgsExtensions
    {
        public static string GetQueryUrl(this LoadDataArgs e, string path, Dictionary<string, string>? extraParameters = null)
        {
            var parameters = extraParameters ?? new Dictionary<string, string>();

            if (e.Skip != null)
            {
                parameters.Add("skip", e.Skip.ToString());
            }

            if (e.Top != null)
            {
                parameters.Add("take", e.Top.ToString());
            }

            return path.TrimEnd('/') + "?" + GetQueryString(parameters);
        }

        private static string GetQueryString(Dictionary<string, string> parameters)
        {
            return string.Join("&", parameters.Select(p => WebUtility.UrlEncode(p.Key) + "=" + WebUtility.UrlEncode(p.Value)));
        }
    }
}

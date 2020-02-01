// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Specialized;
using System.Net.Http;
using System.Threading;
using System.Web;

namespace Microsoft.DotNet.NugetSearch
{
    internal class NugetSearchApiRequest : INugetSearchApiRequest
    {
        public string GetResult(NugetSearchApiParameter nugetSearchApiParameter)
        {
            var queryUrl = ConstructUrl(
                nugetSearchApiParameter.SearchTerm,
                nugetSearchApiParameter.Skip,
                nugetSearchApiParameter.Take,
                nugetSearchApiParameter.Prerelease,
                nugetSearchApiParameter.SemverLevel);

            var httpClient = new HttpClient();
            var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            HttpResponseMessage response = httpClient.GetAsync(queryUrl, cancellation.Token).Result;
            if (!response.IsSuccessStatusCode)
            {
                if ((int)response.StatusCode >= 500 && (int)response.StatusCode < 600)
                {
                    throw new NugetSearchApiRequestException(
                        string.Format(
                            LocalizableStrings.RetriableNugetSearchFailure,
                            queryUrl.AbsoluteUri, response.ReasonPhrase, response.StatusCode));
                }

                throw new NugetSearchApiRequestException(
                    string.Format(
                        LocalizableStrings.NonRetriableNugetSearchFailure,
                        queryUrl.AbsoluteUri, response.ReasonPhrase, response.StatusCode));
            }

            return response.Content.ReadAsStringAsync().Result;
        }

        internal static Uri ConstructUrl(string searchTerm = null, int? skip = null, int? take = null,
            bool prerelease = false, string semverLevel = null)
        {
            var uriBuilder = new UriBuilder("https://azuresearch-usnc.nuget.org/query");
            NameValueCollection query = HttpUtility.ParseQueryString(uriBuilder.Query);
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query["q"] = searchTerm;
            }

            query["packageType"] = "dotnettool";

            if (skip.HasValue)
            {
                query["skip"] = skip.Value.ToString();
            }

            if (take.HasValue)
            {
                query["take"] = take.Value.ToString();
            }

            if (prerelease)
            {
                query["prerelease"] = "true";
            }

            if (!string.IsNullOrWhiteSpace(semverLevel))
            {
                query["semVerLevel"] = semverLevel;
            }

            uriBuilder.Query = query.ToString();

            return uriBuilder.Uri;
        }
    }
}

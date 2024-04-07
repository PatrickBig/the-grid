// <copyright file="MockHttpClientBunitHelpers.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Bunit;
using Microsoft.Extensions.DependencyInjection;
using RichardSzalay.MockHttp;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace TheGrid.Tests.Client
{
    /// <summary>
    /// Service helpers to add a mocked HttpClient for component tests. This is provided straight from <a href="https://bunit.dev/docs/test-doubles/mocking-httpclient.html">bUnit's documentation</a>.
    /// </summary>
    public static class MockHttpClientBunitHelpers
    {
        /// <summary>
        /// Adds a mocked <see cref="HttpClient"/> to a test service provider for bUnit.
        /// </summary>
        /// <param name="services">Service provider.</param>
        /// <returns>A mocked HttpMessageHandler.</returns>
        public static MockHttpMessageHandler AddMockHttpClient(this TestServiceProvider services)
        {
            var mockHttpHandler = new MockHttpMessageHandler();
            var httpClient = mockHttpHandler.ToHttpClient();
            httpClient.BaseAddress = new Uri("http://localhost");
            services.AddSingleton<HttpClient>(httpClient);
            return mockHttpHandler;
        }

        /// <summary>
        /// Sets up a mocked response.
        /// </summary>
        /// <typeparam name="T">Type to return.</typeparam>
        /// <param name="request">Request.</param>
        /// <param name="content">Content.</param>
        /// <returns>A mocked request.</returns>
        public static MockedRequest RespondJson<T>(this MockedRequest request, T content)
        {
            request.Respond(req =>
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(JsonSerializer.Serialize(content));
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return response;
            });
            return request;
        }

        /// <summary>
        /// Sets up a mocked response.
        /// </summary>
        /// <typeparam name="T">Type to return.</typeparam>
        /// <param name="request">Request.</param>
        /// <param name="contentProvider">Content provider.</param>
        /// <returns>A mocked request.</returns>
        public static MockedRequest RespondJson<T>(this MockedRequest request, Func<T> contentProvider)
        {
            request.Respond(req =>
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(JsonSerializer.Serialize(contentProvider()));
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return response;
            });
            return request;
        }
    }
}

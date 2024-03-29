﻿using System.Diagnostics.CodeAnalysis;
using TotovBuilder.AzureFunctions.Abstractions.Net;

namespace TotovBuilder.AzureFunctions.Net
{
    /// <summary>
    /// Represents an HTTP client wrapper factory.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class HttpClientWrapperFactory : IHttpClientWrapperFactory
    {
        /// <summary>
        /// Http client factory.
        /// </summary>
        private readonly IHttpClientFactory HttpClientFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpClientWrapperFactory"/> class.
        /// </summary>
        /// <param name="httpClientFactory"></param>
        public HttpClientWrapperFactory(IHttpClientFactory httpClientFactory)
        {
            HttpClientFactory = httpClientFactory;
        }

        /// <inheritdoc/>
        public IHttpClientWrapper Create()
        {
            return new HttpClientWrapper(HttpClientFactory);
        }
    }
}

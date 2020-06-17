using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Codenizer.HttpClient.Testable
{
    /// <summary>
    /// Implements a message handler that allows to configure predefined responses to HTTP calls.
    /// </summary>
    public class TestableMessageHandler : HttpMessageHandler
    {
        private readonly List<RequestBuilder> _configuredRequests;
        private Exception _exceptionToThrow;

        /// <summary>
        /// Returns the list of requests that were captured by this message handler
        /// </summary>
        public List<HttpRequestMessage> Requests { get; } = new List<HttpRequestMessage>();

        /// <summary>
        /// Returns the list of responses that are configured for this message handler
        /// </summary>
        public ReadOnlyCollection<RequestBuilder> ConfiguredResponses => _configuredRequests.AsReadOnly();

        /// <summary>
        /// Creates a new instance without any predefined responses
        /// </summary>
        public TestableMessageHandler()
        {
            _configuredRequests = new List<RequestBuilder>();
        }

        /// <inheritdoc />
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(request);

            if (_exceptionToThrow != null)
            {
                throw _exceptionToThrow;
            }

            var match = RouteDictionary
                .From(_configuredRequests)
                .Match(
                    request.Method,
                    request.RequestUri.PathAndQuery);

            if(match == null)
            {
                return new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent($"No response configured for {request.RequestUri.PathAndQuery}")
                };
            }

            var responseBuilder = match;

            if (!string.IsNullOrWhiteSpace(responseBuilder.ContentType))
            {
                var requestContentType = request.Content.Headers.ContentType.MediaType;

                if (requestContentType != responseBuilder.ContentType)
                {
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.UnsupportedMediaType
                    };
                }
            }

            responseBuilder.ActionWhenCalled?.Invoke(request);

            var response = new HttpResponseMessage
            {
                StatusCode = responseBuilder.StatusCode
            };

            if (responseBuilder.Data != null)
            {
                response.Content = new StringContent(responseBuilder.Data, Encoding.UTF8, responseBuilder.MediaType);
            }

            foreach (var header in responseBuilder.Headers)
            {
                response.Headers.Add(header.Key, header.Value);
            }

            foreach (var cookie in responseBuilder.Cookies)
            {
                response.Headers.Add("Set-Cookie", cookie);
            }

            if (responseBuilder.Duration > TimeSpan.Zero)
            {
                Thread.Sleep((int)responseBuilder.Duration.TotalMilliseconds);
            }

            return response;
        }

        /// <summary>
        /// Respond to a GET request for the given relative path and query string
        /// </summary>
        /// <param name="pathAndQuery">The path and query string to match</param>
        /// <returns>A <see cref="IRequestBuilder"/> instance that can be used to further configure the response</returns>
        public IRequestBuilder RespondTo(string pathAndQuery)
        {
            return RespondTo(HttpMethod.Get, pathAndQuery);
        }

        /// <summary>
        /// Respond to a request for the given HTTP method, relative path and query string
        /// </summary>
        /// <param name="method">The HTTP method to match</param>
        /// <param name="pathAndQuery">The path and query string to match</param>
        /// <returns>A <see cref="IRequestBuilder"/> instance that can be used to further configure the response</returns>
        public IRequestBuilder RespondTo(HttpMethod method, string pathAndQuery)
        {
            return RespondTo(method, pathAndQuery, null);
        }

        /// <summary>
        /// Respond to a request for the given HTTP method, relative path and query string and Content-Type header
        /// </summary>
        /// <param name="method">The HTTP method to match</param>
        /// <param name="pathAndQuery">The path and query string to match</param>
        /// <param name="contentType">The MIME type to match</param>
        /// <returns>A <see cref="IRequestBuilder"/> instance that can be used to further configure the response</returns>
        public IRequestBuilder RespondTo(HttpMethod method, string pathAndQuery, string contentType)
        {
            var requestBuilder = new RequestBuilder(method, pathAndQuery, contentType);

            _configuredRequests.Add(requestBuilder);

            return requestBuilder;
        }

        /// <summary>
        /// Configures the handler to throw the given exception on any request that is made
        /// </summary>
        /// <param name="exception">The <see cref="Exception"/> instance to throw</param>
        public void ShouldThrow(Exception exception)
        {
            _exceptionToThrow = exception;
        }
        
        /// <summary>
        /// Clears the list of configured responses
        /// </summary>
        public void ClearConfiguredResponses()
        {
            _configuredRequests.Clear();
        }
    }
}

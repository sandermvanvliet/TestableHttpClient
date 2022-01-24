using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Codenizer.HttpClient.Testable
{
    /// <summary>
    /// Implements a message handler that allows to configure predefined responses to HTTP calls.
    /// </summary>
    public class TestableMessageHandler : HttpMessageHandler
    {
        private readonly JsonSerializerSettings _serializerSettings;
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
        public TestableMessageHandler() : this(null)
        {
        }

        /// <summary>
        /// Creates a new instance without any predefined responses
        /// </summary>
        public TestableMessageHandler(JsonSerializerSettings serializerSettings)
        {
            _serializerSettings = serializerSettings ?? new JsonSerializerSettings();
            _configuredRequests = new List<RequestBuilder>();
        }

        /// <inheritdoc />
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
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

            if (responseBuilder.ResponseSequence.Any())
            {
                if (responseBuilder.ResponseSequenceCounter >= responseBuilder.ResponseSequence.Count)
                {
                    return new HttpResponseMessage(HttpStatusCode.InternalServerError)
                    {
                        Content = new StringContent($"Received request number {responseBuilder.ResponseSequenceCounter+1} for {request.RequestUri.PathAndQuery} but only {responseBuilder.ResponseSequence.Count} responses were configured")
                    };
                }

                responseBuilder = responseBuilder.ResponseSequence[responseBuilder.ResponseSequenceCounter++];
            }

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
                if (responseBuilder.Data is byte[] buffer)
                {
                    response.Content = new ByteArrayContent(buffer);
                }
                else if (responseBuilder.Data is string content)
                {
                    response.Content = new StringContent(content, Encoding.UTF8, responseBuilder.MediaType);
                }
                else if (responseBuilder.MediaType == "application/json")
                {
                    response.Content = new StringContent(JsonConvert.SerializeObject(responseBuilder.Data, responseBuilder.SerializerSettings ?? _serializerSettings), Encoding.UTF8, responseBuilder.MediaType);
                }

                else
                {
                    throw new InvalidOperationException(
                        "Unable to determine the response object to return as it's not a string, byte[] or object to return as application/json");
                }
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
                var stopwatch = Stopwatch.StartNew();

                while (stopwatch.ElapsedMilliseconds < responseBuilder.Duration.TotalMilliseconds)
                {
                    Thread.Sleep(5);
                    cancellationToken.ThrowIfCancellationRequested();
                }

            }

            return response;
        }

        /// <summary>
        /// Respond to a GET request for the given relative path and query string
        /// </summary>
        /// <returns>A <see cref="IRequestBuilder"/> instance that can be used to further configure the response</returns>
        public IRequestBuilder RespondTo()
        {
            var requestBuilder = new RequestBuilder();

            _configuredRequests.Add(requestBuilder);

            return requestBuilder;
        }

        /// <summary>
        /// Respond to a GET request for the given relative path and query string
        /// </summary>
        /// <param name="pathAndQuery">The path and query string to match</param>
        /// <returns>A <see cref="IRequestBuilder"/> instance that can be used to further configure the response</returns>
        /// <remarks>A more fluent approach is available through RespondTo().Get().Url()</remarks>
        [Obsolete("A more fluent approach is available through RespondTo().Get()", false)]
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
        /// <remarks>A more fluent approach is available through RespondTo().Get().Url()</remarks>
        [Obsolete("A more fluent approach is available through RespondTo().Get()", false)]
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
        /// <remarks>A more fluent approach is available through RespondTo().Get().Url()</remarks>
        [Obsolete("A more fluent approach is available through RespondTo().Get()", false)]
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

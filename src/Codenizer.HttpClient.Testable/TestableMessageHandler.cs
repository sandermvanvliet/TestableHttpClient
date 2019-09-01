using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Codenizer.HttpClient.Testable
{
    public class TestableMessageHandler : HttpMessageHandler
    {
        private readonly List<RequestBuilder> _configuredRequests;
        private Exception _exceptionToThrow;

        public List<HttpRequestMessage> Requests { get; } = new List<HttpRequestMessage>();

        public TestableMessageHandler()
        {
            _configuredRequests = new List<RequestBuilder>();
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if(_exceptionToThrow != null)
            {
                throw _exceptionToThrow;
            }

            Requests.Add(request);

            var matches = _configuredRequests
                .Where(r => r.PathAndQuery == request.RequestUri.PathAndQuery && 
                            r.Method == request.Method)
                .ToList();

            if(!matches.Any())
            {
                return new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent($"No response configured for {request.RequestUri.PathAndQuery}")
                };
            }
            
            if(matches.Count > 1)
            {
                throw new MultipleResponsesConfiguredException(matches.Count, request.RequestUri.PathAndQuery);
            }

            var responseBuilder = matches.Single();

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

            if (responseBuilder.Duration > TimeSpan.Zero)
            {
                await Task.Delay(responseBuilder.Duration, cancellationToken);
            }

            return response;
        }

        public IRequestBuilder RespondTo(string pathAndQuery)
        {
            return RespondTo(HttpMethod.Get, pathAndQuery);
        }

        public IRequestBuilder RespondTo(HttpMethod method, string pathAndQuery)
        {
            return RespondTo(method, pathAndQuery, null);
        }

        public IRequestBuilder RespondTo(HttpMethod method, string pathAndQuery, string contentType)
        {
            var requestBuilder = new RequestBuilder(method, pathAndQuery, contentType);

            _configuredRequests.Add(requestBuilder);

            return requestBuilder;
        }

        public void ShouldThrow(Exception exception)
        {
            _exceptionToThrow = exception;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace Codenizer.HttpClient.Testable
{
    internal class ConfiguredRequests
    {
        private readonly RequestsRootNode _root;

        public ConfiguredRequests(IEnumerable<RequestBuilder> requestBuilders)
        {
            _root = new RequestsRootNode();

            foreach (var requestBuilder in requestBuilders)
            {
                if(Uri.TryCreate(requestBuilder.PathAndQuery, UriKind.RelativeOrAbsolute, out var parsedUri))
                {
                    var methodNode = _root.Add(requestBuilder.Method);

                    var scheme = parsedUri.IsAbsoluteUri ? parsedUri.Scheme : "*";

                    var schemeNode = methodNode.Add(scheme);

                    var authority = parsedUri.IsAbsoluteUri ? parsedUri.Authority : "*";

                    var authorityNode = schemeNode.Add(authority);

                    var path = parsedUri.IsAbsoluteUri
                        ? parsedUri.PathAndQuery.Split('?').First()
                        : parsedUri.OriginalString.Split('?').First();

                    var pathNode = authorityNode.Add(path);

                    var queryNode = pathNode.Add(requestBuilder.QueryParameters);

                    queryNode.Add(requestBuilder.Headers);

                    Count++;
                }
                else
                {
                    throw new InvalidOperationException("Can't create URI from request builder");
                }
            }
        }

        public int Count { get; }

        public object? Match(HttpRequestMessage httpRequestMessage)
        {
            var scheme = httpRequestMessage.RequestUri.IsAbsoluteUri ? httpRequestMessage.RequestUri.Scheme : "*";
            
            var methodNode = _root.Match(httpRequestMessage.Method);

            if (methodNode == null)
            {
                return null;
            }

            var schemeNode = methodNode.Match(scheme);

            if (schemeNode == null)
            {
                return null;
            }

            var authority = httpRequestMessage.RequestUri.IsAbsoluteUri ? httpRequestMessage.RequestUri.Authority : "*";

            var authorityNode = schemeNode.Match(authority);

            if (authorityNode == null)
            {
                return null;
            }

            var path = httpRequestMessage.RequestUri.IsAbsoluteUri
                ? httpRequestMessage.RequestUri.PathAndQuery.Split('?').First()
                : httpRequestMessage.RequestUri.OriginalString.Split('?').First();

            var pathNode = authorityNode.Match(path);

            if (pathNode == null)
            {
                return null;
            }

            var query = httpRequestMessage.RequestUri.IsAbsoluteUri
                ? httpRequestMessage.RequestUri.Query
                // If the URI doesn't contain query parameters the last element after
                // a split is the entire string so we first need to check if there is
                // a ? in the URI in the first place.
                : httpRequestMessage.RequestUri.OriginalString.Contains('?')
                    ? httpRequestMessage.RequestUri.OriginalString.Split('?').Last()
                    : null;

            var queryNode = pathNode.Match(query);

            if (queryNode == null)
            {
                return null;
            }

            var headersNode = queryNode.Match(httpRequestMessage.Headers);

            return headersNode;
        }

        internal static ConfiguredRequests FromRequestBuilders(IEnumerable<RequestBuilder> requestBuilders)
        {
            return new ConfiguredRequests(requestBuilders);
        }
    }
}
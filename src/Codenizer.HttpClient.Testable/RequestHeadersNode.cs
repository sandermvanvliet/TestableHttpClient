using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;

namespace Codenizer.HttpClient.Testable
{
    internal class RequestHeadersNode
    {
        private readonly Dictionary<string, string> _headers;

        public RequestHeadersNode(Dictionary<string, string> headers)
        {
            _headers = headers;
        }

        public RequestBuilder RequestBuilder { get; private set; }

        public bool Matches(Dictionary<string, string> headers)
        {
            if (_headers.Count != headers.Count)
            {
                return false;
            }

            foreach (var kv in _headers)
            {
                if (!headers.Any(h => h.Key == kv.Key && h.Value == kv.Value))
                {
                    return false;
                }
            }

            return true;
        }

        public bool Match(HttpRequestHeaders headers)
        {
            foreach (var kv in _headers)
            {
                if (headers.TryGetValues(kv.Key, out var values))
                {
                    if (values.All(v => v != kv.Value))
                    {
                        return false;
                    }
                }
                else if (kv.Key == "Content-Type" && _headers.ContainsKey("Content-Type"))
                {
                    // TODO: Remove this in a future release (2.4.x)
                    // This check is only here for backwards compatibility
                    // for the "feature" to automatically return HTTP 415 Unsupported Media Type
                    // when the Content-Type header value doesn't match what has been configured
                    // on that particular URI.
                    // This should be configured by the user of the library when their software
                    // depends on that behaviour from a server.
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        public void SetRequestBuilder(RequestBuilder requestBuilder)
        {
            if(RequestBuilder != null)
            {
                throw new MultipleResponsesConfiguredException(2, requestBuilder.PathAndQuery);
            }

            RequestBuilder = requestBuilder;
        }
    }
}
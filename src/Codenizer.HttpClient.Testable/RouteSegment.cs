using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace Codenizer.HttpClient.Testable
{
    internal class RouteSegment
    {
        public string Part { get; }
        public Dictionary<string, RouteSegment> Segments { get; } = new Dictionary<string, RouteSegment>();
        
        private readonly Dictionary<HttpMethod, List<RequestBuilder>> _requestBuildersWithParameters = new Dictionary<HttpMethod, List<RequestBuilder>>();

        public RouteSegment(string part)
        {
            Part = part;
        }

        public bool HasForQueryParameters(HttpMethod method)
        {
            if (!_requestBuildersWithParameters.ContainsKey(method))
            {
                return false;
            }

            return _requestBuildersWithParameters.ContainsKey(method);
        }

        public RequestBuilder GetForQueryParameters(HttpMethod method, Dictionary<string, string> queryParameters)
        {
            var all = _requestBuildersWithParameters[method];

            foreach (var b in all)
            {
                if (b.QueryParameters.All(kv =>
                    queryParameters.ContainsKey(kv.Key)))
                {
                    var match = b;

                    foreach (var kv in b.QueryParameters)
                    {
                        var assertion = b.QueryStringAssertions.SingleOrDefault(a => a.Key == kv.Key);

                        if (assertion != null)
                        {
                            if (!assertion.AnyValue && assertion.Value != queryParameters[kv.Key])
                            {
                                match = null;
                                break;
                            }
                        }
                        else if (b.QueryParameters[kv.Key] != queryParameters[kv.Key])
                        {
                            match = null;
                            break;
                        }
                    }

                    if (match != null)
                    {
                        return match;
                    }
                }
            }

            return null;
        }

        public void Add(RequestBuilder route)
        {
            if (!_requestBuildersWithParameters.ContainsKey(route.Method))
            {
                _requestBuildersWithParameters.Add(route.Method, new List<RequestBuilder>());
            }

            // If this route has no query parameters and there is already a route without query parameters
            // then we can't add it.
            if (!route.QueryParameters.Any() &&
                _requestBuildersWithParameters[route.Method].Any() &&
                _requestBuildersWithParameters[route.Method].All(r => !r.QueryParameters.Any()))
            {
                throw new MultipleResponsesConfiguredException(2, route.PathAndQuery);
            }

            if(route.QueryParameters.Any())
            {
                var othersWithParameters = _requestBuildersWithParameters[route.Method]
                    .Where(r => r.QueryParameters.Any())
                    .ToList();

                foreach (var b in othersWithParameters)
                {
                    if (b.QueryParameters.All(kv =>
                        route.QueryParameters.ContainsKey(kv.Key) &&
                        route.QueryParameters[kv.Key] == kv.Value))
                    {
                        throw new MultipleResponsesConfiguredException(2, route.PathAndQuery);
                    }
                }
            }

            _requestBuildersWithParameters[route.Method].Add(route);
        }

        public RequestBuilder GetWithoutQueryParameters(HttpMethod method)
        {
            if (!_requestBuildersWithParameters.ContainsKey(method))
            {
                return null;
            }

            if (_requestBuildersWithParameters[method].Count == 0)
            {
                return null;
            }

            if (_requestBuildersWithParameters[method].Count > 1)
            {
                throw new MultipleResponsesConfiguredException(_requestBuildersWithParameters[method].Count, method + " has responses with query parameters, use GetForQueryParameters() instead");
            }

            return _requestBuildersWithParameters[method].Single();
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;

namespace Codenizer.HttpClient.Testable
{
    internal class RequestQueryNode
    {
        private readonly List<KeyValuePair<string, string?>> _queryParameters;
        private readonly List<RequestHeadersNode> _headersNodes = new List<RequestHeadersNode>();

        public RequestQueryNode(List<KeyValuePair<string, string>> queryParameters)
        {
            _queryParameters = queryParameters;
        }

        public bool Matches(string? queryString)
        {
            var inputQueryParameters = QueryParametersFrom(queryString);

            return Matches(inputQueryParameters!);
        }

        public bool Matches(List<KeyValuePair<string, string>> inputQueryParameters)
        {
            if (inputQueryParameters.Count != _queryParameters.Count)
            {
                // if the counts don't match then we're done
                return false;
            }

            foreach (var qp in inputQueryParameters)
            {
                // Check if the query parameter name exists at all
                if (_queryParameters.All(q => q.Key != qp.Key))
                {
                    return false;
                }

                // Check if the query parameter exists with the right name and value
                if (!_queryParameters.Any(q => q.Key == qp.Key && q.Value == qp.Value))
                {
                    return false;
                }
            }

            return true;
        }

        private static List<KeyValuePair<string, string?>> QueryParametersFrom(string? query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return new List<KeyValuePair<string, string?>>();
            }

            return query!
                .Replace("?", "") // When using the Query property from Uri you will get a leading ?
                .Split('&')
                .Select(p => p.Split('='))
                .Select(p => new KeyValuePair<string, string?>(p[0],  p.Length == 2 ? p[1] : null))
                .ToList();
        }

        public RequestHeadersNode Add(Dictionary<string, string> headers)
        {
            var existingHeaders = _headersNodes.SingleOrDefault(node => node.Matches(headers));

            if (existingHeaders == null)
            {
                existingHeaders = new RequestHeadersNode(headers);
                _headersNodes.Add(existingHeaders);
            }

            return existingHeaders;
        }

        public RequestHeadersNode? Match(HttpRequestHeaders headers)
        {
            return _headersNodes.SingleOrDefault(node => node.Match(headers));;
        }
    }
}
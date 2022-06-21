using System.Collections.Generic;
using System.Linq;

namespace Codenizer.HttpClient.Testable
{
    internal class RequestPathNode
    {
        public string Path { get; }
        private readonly List<RequestQueryNode> _queryNodes = new List<RequestQueryNode>();

        public RequestPathNode(string path)
        {
            Path = path;
        }

        public RequestQueryNode Add(List<KeyValuePair<string, string>> queryParameters)
        {
            var existingQuery = _queryNodes.SingleOrDefault(node => node.Matches(queryParameters));

            if (existingQuery == null)
            {
                existingQuery = new RequestQueryNode(queryParameters);
                _queryNodes.Add(existingQuery);
            }

            return existingQuery;
        }

        public RequestQueryNode? Match(string? queryString)
        {
            return _queryNodes.SingleOrDefault(node => node.Matches(queryString));
        }
    }
}
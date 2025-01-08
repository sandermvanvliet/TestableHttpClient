using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace Codenizer.HttpClient.Testable
{
    internal class RequestPathNode : RequestNode
    {
        public string Path { get; }
        private readonly List<RequestWhenNode> _whenNodes = new ();
        private readonly List<RequestQueryNode> _queryNodes = new ();

        public RequestPathNode(string path)
        {
            Path = path;
        }

        public RequestWhenNode Add(Func<HttpRequestMessage, object, bool>? predicate, object? userValue) {
            
            var existingWhen = _whenNodes.SingleOrDefault(node => node.Matches(userValue));

            if (existingWhen is null) {
                _whenNodes.Add(existingWhen = new RequestWhenNode(
                    predicate ?? ((_, _) => true),
                    userValue ?? new object()));
            }

            return existingWhen;
        }
        

        public RequestQueryNode Add(
            List<KeyValuePair<string, string?>> queryParameters,
            List<QueryStringAssertion> queryStringAssertions)
        {
            var existingQuery = _queryNodes.SingleOrDefault(node => node.Matches(queryParameters));

            if (existingQuery == null)
            {
                existingQuery = new RequestQueryNode(queryParameters, queryStringAssertions);
                _queryNodes.Add(existingQuery);
            }

            return existingQuery;
        }

        public RequestQueryNode? Match(string? queryString)
        {
            return _queryNodes.SingleOrDefault(node => node.Matches(queryString));
        }

        public RequestWhenNode? Match(HttpRequestMessage request)
        {
            return _whenNodes.SingleOrDefault(node => node.Matches(request));
        }
        
        public bool HasWhenNodes() => _whenNodes.Any();

        public bool MatchesPath(string path)
        {
            if (PathHasRouteParameters())
            {
                return MatchesPathWithRouteParameters(path);
            }

            return Path == path;
        }

        private bool PathHasRouteParameters()
        {
            return Path.Contains("{") && Path.Contains("}");
        }

        private bool MatchesPathWithRouteParameters(string path)
        {
            var pathSegments = Path.Split('/');
            var matchSegments = path.Split('/');

            if (pathSegments.Length != matchSegments.Length)
            {
                return false;
            }

            for (var index = 0; index < pathSegments.Length; index++)
            {
                // Check whether this segment is a route parameter (for example: '{id}')
                if (pathSegments[index].StartsWith("{") && pathSegments[index].EndsWith("}"))
                {
                    // It is, ignore this particular segment for matching
                    // because we treat this as a wildcard.
                    continue;
                }

                if (pathSegments[index] != matchSegments[index])
                {

                    return false;
                }
            }

            return true;
        }

        public override void Accept(RequestNodeVisitor visitor)
        {
            visitor.Path(Path);

            foreach (var node in _whenNodes)
            {
                node.Accept(visitor);
            }
        }
    }
}
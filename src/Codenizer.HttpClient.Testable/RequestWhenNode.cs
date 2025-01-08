using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Codenizer.HttpClient.Testable
{
    internal class RequestWhenNode : RequestNode {
        private readonly Func<HttpRequestMessage, object, bool> _predicate;
        private readonly object _userObject;        
        private readonly List<RequestQueryNode> _queryNodes = new List<RequestQueryNode>();

        
        public RequestWhenNode(Func<HttpRequestMessage, object, bool> predicate, object userObject) {
            _predicate = predicate;
            _userObject = userObject;
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

        public RequestQueryNode? Match(string queryString)
        {            
            return _queryNodes.SingleOrDefault(node => node.Matches(queryString));
        }
        
        public bool Matches(object? userObject) {
            if (userObject == null)
                return false;
            return _userObject == userObject;
        }
        
        public bool Matches(HttpRequestMessage message) {
            return _predicate(message, _userObject);
        }

        public override void Accept(RequestNodeVisitor visitor)
        {
            visitor.When(_predicate);
            
            foreach (var node in _queryNodes)
            {
                node.Accept(visitor);
            }
        }
        
        
    }
}
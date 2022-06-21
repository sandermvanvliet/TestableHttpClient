using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace Codenizer.HttpClient.Testable
{
    internal class RequestMethodNode
    {
        private readonly List<RequestSchemeNode> _schemeNodes = new List<RequestSchemeNode>();
        public HttpMethod Method { get; }

        public RequestMethodNode(HttpMethod method)
        {
            Method = method;
        }

        public RequestSchemeNode Add(string scheme)
        {
            var existingScheme = _schemeNodes.SingleOrDefault(s => s.Scheme == scheme);

            if (existingScheme == null)
            {
                existingScheme = new RequestSchemeNode(scheme);
                _schemeNodes.Add(existingScheme);
            }

            return existingScheme;
        }

        public RequestSchemeNode? Match(string scheme)
        {
            return _schemeNodes.SingleOrDefault(s => s.Scheme == scheme);
        }
    }
}
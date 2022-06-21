using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace Codenizer.HttpClient.Testable
{
    internal class RequestsRootNode
    {
        private readonly List<RequestMethodNode> _methodNodes = new List<RequestMethodNode>();

        public RequestMethodNode Add(HttpMethod method)
        {
            var existingMethod = _methodNodes.SingleOrDefault(node => node.Method == method);

            if (existingMethod == null)
            {
                existingMethod = new RequestMethodNode(method);
                _methodNodes.Add(existingMethod);
            }

            return existingMethod;
        }

        public RequestMethodNode? Match(HttpMethod method)
        {
            return _methodNodes.SingleOrDefault(node => node.Method == method);
        }
    }
}
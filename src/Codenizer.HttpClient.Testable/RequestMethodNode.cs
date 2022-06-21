using System.CodeDom.Compiler;
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
            var explicitSchemeMatch = _schemeNodes.SingleOrDefault(s => s.Scheme == scheme);

            if (explicitSchemeMatch == null)
            {
                // If no explicit match was found, try to use a wildcard match.
                // This is applicable for responses with a relative URI.
                return _schemeNodes.SingleOrDefault(s => s.Scheme == "*");
            }

            return explicitSchemeMatch;
        }

        public void Dump(IndentedTextWriter indentedWriter)
        {
            foreach (var node in _schemeNodes)
            {
                indentedWriter.WriteLine(node.Scheme + "://");
                indentedWriter.Indent++;
                node.Dump(indentedWriter);
                indentedWriter.Indent--;
            }
        }
    }
}
using System.Collections.Generic;
using System.Linq;

namespace Codenizer.HttpClient.Testable
{
    internal class RequestSchemeNode
    {
        private readonly List<RequestAuthorityNode> _authorityNodes = new List<RequestAuthorityNode>();

        public RequestSchemeNode(string scheme)
        {
            Scheme = scheme;
        }

        public string Scheme { get; }

        public RequestAuthorityNode Add(string authority)
        {
            var existingAuthority = _authorityNodes.SingleOrDefault(s => s.Authority == authority);

            if (existingAuthority == null)
            {
                existingAuthority = new RequestAuthorityNode(authority);
                _authorityNodes.Add(existingAuthority);
            }

            return existingAuthority;
        }

        public RequestAuthorityNode? Match(string authority)
        {
            return _authorityNodes.SingleOrDefault(node => node.Authority == authority);
        }
    }
}
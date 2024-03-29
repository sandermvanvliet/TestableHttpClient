﻿using System.Collections.Generic;
using System.Linq;

namespace Codenizer.HttpClient.Testable
{
    internal class RequestSchemeNode : RequestNode
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
            var explicitAuthorityMatch = _authorityNodes.SingleOrDefault(node => node.Authority == authority);

            if (explicitAuthorityMatch == null)
            {
                // If no explicit match was found, try to use a wildcard match.
                // This is applicable for responses with a relative URI.
                return _authorityNodes.SingleOrDefault(node => node.Authority == "*");
            }

            return explicitAuthorityMatch;
        }

        public override void Accept(RequestNodeVisitor visitor)
        {
            visitor.Scheme(Scheme);

            foreach (var node in _authorityNodes)
            {
                node.Accept(visitor);
            }
        }
    }
}
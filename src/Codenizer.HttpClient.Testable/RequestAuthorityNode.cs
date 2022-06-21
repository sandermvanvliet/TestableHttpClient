using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;

namespace Codenizer.HttpClient.Testable
{
    internal class RequestAuthorityNode
    {
        public string Authority { get; }
        private readonly List<RequestPathNode> _pathNodes = new List<RequestPathNode>();

        public RequestAuthorityNode(string authority)
        {
            Authority = authority;
        }

        public RequestPathNode Add(string path)
        {
            var existingPath = _pathNodes.SingleOrDefault(s => s.Path == path);

            if (existingPath == null)
            {
                existingPath = new RequestPathNode(path);
                _pathNodes.Add(existingPath);
            }

            return existingPath;
        }

        public RequestPathNode? Match(string path)
        {
            return _pathNodes.SingleOrDefault(node => node.MatchesPath(path));
        }

        public void Dump(IndentedTextWriter indentedWriter)
        {
            foreach (var node in _pathNodes)
            {
                indentedWriter.WriteLine(node.Path);
                indentedWriter.Indent++;
                node.Dump(indentedWriter);
                indentedWriter.Indent--;
            }
        }
    }
}
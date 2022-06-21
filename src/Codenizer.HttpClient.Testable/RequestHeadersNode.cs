using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;

namespace Codenizer.HttpClient.Testable
{
    internal class RequestHeadersNode
    {
        private readonly Dictionary<string, string> _headers;

        public RequestHeadersNode(Dictionary<string, string> headers)
        {
            _headers = headers;
        }

        public bool Matches(Dictionary<string, string> headers)
        {
            return false;
        }

        public bool Match(HttpRequestHeaders headers)
        {
            foreach (var kv in _headers)
            {
                if (headers.TryGetValues(kv.Key, out var values))
                {
                    if (values.All(v => v != kv.Value))
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            return true;
        }
    }
}
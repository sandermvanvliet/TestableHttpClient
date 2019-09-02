using System.Collections.Generic;
using System.Net.Http;

namespace Codenizer.HttpClient.Testable
{
    public class RouteSegment
    {
        public string Part { get; }
        public Dictionary<string, RouteSegment> Segments { get; } = new Dictionary<string, RouteSegment>();
        public Dictionary<HttpMethod, RequestBuilder> RequestBuilders { get; } = new Dictionary<HttpMethod, RequestBuilder>();

        public RouteSegment(string part)
        {
            Part = part;
        }
    }
}
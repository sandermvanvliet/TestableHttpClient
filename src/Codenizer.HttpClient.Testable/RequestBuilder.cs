using System.Collections.Generic;
using System.Net;
using System.Net.Http;

namespace Codenizer.HttpClient.Testable
{
    public class RequestBuilder
    {
        public RequestBuilder(HttpMethod method, string pathAndQuery)
        {
            Method = method;
            PathAndQuery = pathAndQuery;
        }

        public string PathAndQuery { get; }
        public HttpStatusCode StatusCode { get; private set; } = HttpStatusCode.InternalServerError;
        public HttpMethod Method { get; }
        public string Data { get; private set; }
        public string MediaType { get; private set; }
        public Dictionary<string, string> Headers { get; } = new Dictionary<string, string>();

        public RequestBuilder With(HttpStatusCode httpStatusCode)
        {
            StatusCode = httpStatusCode;
            return this;
        }

        public RequestBuilder AndContent(string mimeType, string data)
        {
            MediaType = mimeType;
            Data = data;

            return this;
        }

        public RequestBuilder AndHeaders(Dictionary<string, string> headers)
        {
            foreach (var header in headers)
            {
                if (Headers.ContainsKey(header.Key))
                {
                    Headers[header.Key] += "," + header.Value;
                }
                else
                {
                    Headers.Add(header.Key, header.Value);
                }
            }
            
            return this;
        }
    }
}
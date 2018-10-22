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

        public string PathAndQuery { get; private set; }
        public HttpStatusCode StatusCode { get; private set; } = HttpStatusCode.InternalServerError;
        public HttpMethod Method { get; private set; } = HttpMethod.Get;
        public string Data { get; private set; }
        public string MediaType { get; private set; }

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
    }
}
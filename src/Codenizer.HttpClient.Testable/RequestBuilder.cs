using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;

namespace Codenizer.HttpClient.Testable
{
    public class RequestBuilder : IRequestBuilder, IResponseBuilder
    {
        public RequestBuilder(HttpMethod method, string pathAndQuery, string contentType)
        {
            Method = method;
            PathAndQuery = pathAndQuery;
            ContentType = contentType;

            RouteParameters = ParseRouteParametersFromPathAndQuery(pathAndQuery);
        }

        private List<string> ParseRouteParametersFromPathAndQuery(string pathAndQuery)
        {
            
            return new List<string>();
        }

        public string PathAndQuery { get; }
        public string ContentType { get; }
        public HttpStatusCode StatusCode { get; private set; } = HttpStatusCode.InternalServerError;
        public HttpMethod Method { get; }
        public string Data { get; private set; }
        public string MediaType { get; private set; }
        public Dictionary<string, string> Headers { get; } = new Dictionary<string, string>();
        public TimeSpan Duration { get; private set; } = TimeSpan.Zero;
        public Action<HttpRequestMessage> ActionWhenCalled { get; private set; }
        public List<string> RouteParameters { get; private set; }

        public IResponseBuilder With(HttpStatusCode httpStatusCode)
        {
            StatusCode = httpStatusCode;
            return this;
        }

        public IResponseBuilder AndContent(string mimeType, string data)
        {
            MediaType = mimeType;
            Data = data;

            return this;
        }

        public IResponseBuilder AndHeaders(Dictionary<string, string> headers)
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

        public IResponseBuilder Taking(TimeSpan time)
        {
            Duration = time;

            return this;
        }

        public IResponseBuilder WhenCalled(Action<HttpRequestMessage> action)
        {
            ActionWhenCalled = action;

            return this;
        }
    }

    public interface IRequestBuilder
    {
        IResponseBuilder With(HttpStatusCode httpStatusCode);
    }

    public interface IResponseBuilder
    {
        IResponseBuilder AndContent(string mimeType, string data);
        IResponseBuilder AndHeaders(Dictionary<string, string> headers);
        IResponseBuilder Taking(TimeSpan time);
        IResponseBuilder WhenCalled(Action<HttpRequestMessage> action);
    }
}
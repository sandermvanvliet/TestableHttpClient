using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;

namespace Codenizer.HttpClient.Testable
{
    public class RequestBuilder : IRequestBuilder, IResponseBuilder
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
        public TimeSpan Duration { get; private set; } = TimeSpan.Zero;

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
    }
}
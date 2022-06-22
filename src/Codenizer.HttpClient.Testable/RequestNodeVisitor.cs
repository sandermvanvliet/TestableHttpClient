using System.Net.Http;

namespace Codenizer.HttpClient.Testable
{
    internal abstract class RequestNodeVisitor
    {
        public abstract void Header(string key, string value);
        public abstract void QueryParameter(string key, string? value);
        public abstract void Path(string path);
        public abstract void Authority(string authority);
        public abstract void Scheme(string scheme);
        public abstract void Method(HttpMethod method);
        public abstract void Response(RequestBuilder requestBuilder);
    }
}
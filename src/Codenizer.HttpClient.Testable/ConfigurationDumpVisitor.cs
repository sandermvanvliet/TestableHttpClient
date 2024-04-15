using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using System.Net.Http;

namespace Codenizer.HttpClient.Testable
{
    internal class ConfigurationDumpVisitor : RequestNodeVisitor
    {
        private readonly StringWriter _writer;
        private readonly IndentedTextWriter _indentedWriter;
        private bool _hasMultipleQueryParams;

        public ConfigurationDumpVisitor()
        {
            _writer = new StringWriter();

            _indentedWriter = new IndentedTextWriter(_writer, "    ");
        }

        public string Output => _writer.ToString();

        public override void Header(string key, string value)
        {
            _indentedWriter.Indent = 4;
            _indentedWriter.WriteLine();

            _indentedWriter.WriteLine($"{key}: {value}");
        }

        public override void QueryParameter(string key, string? value)
        {
            _indentedWriter.Indent = 4;

            if (_hasMultipleQueryParams)
            {
                _indentedWriter.Write("&");
            }
            else
            {
                _indentedWriter.Write("?");
            }

            _indentedWriter.Write($"{key}={value}");
            
            _hasMultipleQueryParams = true;
        }

        public override void Path(string path)
        {
            _indentedWriter.Indent = 3;
            _indentedWriter.Write(path);

            _hasMultipleQueryParams = false;
        }

        public override void Authority(string authority)
        {
            _indentedWriter.Indent = 2;
            _indentedWriter.WriteLine(authority);
        }

        public override void Scheme(string scheme)
        {
            _indentedWriter.Indent = 1;
            _indentedWriter.WriteLine($"{scheme}://");
        }

        public override void Method(HttpMethod method)
        {
            _indentedWriter.Indent = 0;
            _indentedWriter.WriteLine(method.Method);
        }

        public override void Response(RequestBuilder requestBuilder)
        {
            _indentedWriter.Indent = 4;
            _indentedWriter.WriteLine("Response:");
            _indentedWriter.Indent++;

            var payload = requestBuilder.Data != null
                ? $" with {requestBuilder.MediaType} payload"
                : "";

            _indentedWriter.WriteLine($"HTTP {(int)requestBuilder.StatusCode} {requestBuilder.StatusCode}{payload}");

            if (requestBuilder.Headers.Any())
            {
                foreach (var h in requestBuilder.Headers)
                {
                    _indentedWriter.WriteLine($"{h.Key}: {h.Value}");
                }
            }

            if (requestBuilder.Cookies.Any())
            {
                foreach (var cookie in requestBuilder.Cookies)
                {
                    _indentedWriter.WriteLine($"Set-Cookie: {cookie}");
                }
            }
        }

        public override void Content(string expectedContent)
        {
            _indentedWriter.WriteLine($"Content: {expectedContent}");
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Codenizer.HttpClient.Testable
{
    /// <summary>
    /// Implements a builder to configure a response
    /// </summary>
    public class RequestBuilder : IRequestBuilder, IResponseBuilder
    {
        private readonly RequestBuilder? _root;

        internal RequestBuilder()
        {
        }

        /// <summary>
        /// Creates a new instance that matches the HTTP method, path and query string and content types
        /// </summary>
        /// <param name="method">The HTTP method to match</param>
        /// <param name="pathAndQuery">The path and query string to match</param>
        /// <param name="contentType">The MIME type to match, <c>null</c> matches any MIME type</param>
        /// <param name="root">Optional. The root <see cref="RequestBuilder"/> that this instance belongs to, this is used for requests that have a sequence of responses.</param>
        internal RequestBuilder(HttpMethod method, string pathAndQuery, string? contentType, RequestBuilder? root = null)
        {
            _root = root;
            Method = method;
            PathAndQuery = pathAndQuery;

            if (pathAndQuery.Contains("?"))
            {
                var parts = pathAndQuery.Split('?');

                PathAndQuery = parts[0];

                QueryParameters = parts[1]
                    .Split('&')
                    .Select(p => p.Split('='))
                    .Select(p => new KeyValuePair<string, string?>(p[0], p.Length == 2 ? p[1] : null))
                    .ToList();
            }

            ContentType = contentType;
        }
        
        /// <summary>
        /// The query parameters configured for the request
        /// </summary>
        public List<KeyValuePair<string, string?>> QueryParameters { get; private set; } = new List<KeyValuePair<string, string?>>();

        /// <summary>
        /// The path and query of the request to match
        /// </summary>
        public string? PathAndQuery { get; private set; }
        /// <summary>
        /// The MIME type of the request to match
        /// </summary>
        public string? ContentType { get; private set; }
        /// <summary>
        /// The status code to respond with. Defaults to 500 Internal Server Error
        /// </summary>
        public HttpStatusCode StatusCode { get; private set; } = HttpStatusCode.InternalServerError;
        /// <summary>
        /// The HTTP verb of the request to match
        /// </summary>
        public HttpMethod? Method { get; private set; }
        /// <summary>
        /// Optional. The data to respond with. Use <see cref="AndContent"/> or <see cref="AndJsonContent"/> to set.
        /// </summary>
        public object? Data { get; private set; }
        /// <summary>
        /// Optional. The callback to invoke when generating the response to a request.
        /// </summary>
        public Func<HttpRequestMessage, object>? ResponseCallback { get; private set; }
        public Func<HttpRequestMessage, Task<object>>? AsyncResponseCallback { get; private set; }
        /// <summary>
        /// Optional. The MIME type of the content to respond with. Only applicable if <see cref="Data"/> is also provided, otherwise ignored.
        /// </summary>
        public string? MediaType { get; private set; }
        /// <summary>
        /// Optional. The headers to set on the response.
        /// </summary>
        public Dictionary<string, string> Headers { get; } = new Dictionary<string, string>();
        /// <summary>
        /// Optional. The time to delay before responding to the matching request. Use <see cref="Taking"/> to set.
        /// </summary>
        public TimeSpan Duration { get; private set; } = TimeSpan.Zero;
        /// <summary>
        /// Optional. An action that will be called when the request matches, before providing the response.
        /// </summary>
        public Action<HttpRequestMessage>? ActionWhenCalled { get; private set; }
        /// <summary>
        /// Optional. A list of cookies to set on the response. Use <see cref="AndCookie"/> to set.
        /// </summary>
        public List<string> Cookies { get; } = new List<string>();
        /// <summary>
        /// Optional. Collection of assertions to further match a request. See <see cref="IRequestBuilderForQueryString"/>.
        /// </summary>
        public List<QueryStringAssertion> QueryStringAssertions { get; } = new List<QueryStringAssertion>();
        /// <summary>
        /// The sequence of responses to give to the matching request. Configured using <see cref="WithSequence"/>.
        /// </summary>
        public List<RequestBuilder> ResponseSequence { get; } = new List<RequestBuilder>();
        /// <summary>
        /// The current index of the responses matching the request. Incremented at each matching call to the request.
        /// </summary>
        public int ResponseSequenceCounter { get; set; }
        /// <summary>
        /// Optional. JSON serializer settings to use when serializing <see cref="Data"/> when sending the response. Use <see cref="AndJsonContent"/> to set.
        /// </summary>
        public JsonSerializerSettings? SerializerSettings { get; private set; }
        /// <summary>
        /// Optional. The value of the Accept header to match.
        /// </summary>
        public string? Accept { get; private set; }
        
        
        /// <summary>
        /// Optional. The value of the Accept header to match.
        /// </summary>
        public AuthenticationHeaderValue? AuthorizationHeader { get; private set; }

        /// <summary>
        /// Optional. The expected content of the request.
        /// </summary>
        public string? ExpectedContent { get; private set; }
        
        /// <inheritdoc />
        public IResponseBuilder With(HttpStatusCode statusCode)
        {
            StatusCode = statusCode;
            return this;
        }
        
        /// <inheritdoc />
        [Obsolete("Use " + nameof(WithQueryStringParameter) + " instead")]
        public IRequestBuilderForQueryString ForQueryStringParameter(string key)
        {
            return WithQueryStringParameter(key);
        }

        /// <inheritdoc />
        public IRequestBuilderForQueryString WithQueryStringParameter(string key)
        {
            return new RequestBuilderForQueryString(this, key);
        }
        
        /// <inheritdoc />
        public IRequestBuilder WithSequence(Action<IRequestBuilder> builder)
        {
            if (Method == null)
            {
                throw new ArgumentNullException(nameof(Method), "HTTP method must be configured");
            }

            if (string.IsNullOrEmpty(PathAndQuery))
            {
                throw new ArgumentNullException(nameof(PathAndQuery), "Request path must be configured");
            }

            var requestBuilder = new RequestBuilder(Method, PathAndQuery!, ContentType, _root ?? this);
            
            builder(requestBuilder);

            var root = _root ?? this;

            root.ResponseSequence.Add(requestBuilder);

            return requestBuilder;
        }
        
        /// <inheritdoc />
        public IRequestBuilder Get()
        {
            Method = HttpMethod.Get;
            return this;
        }
        
        /// <inheritdoc />
        public IRequestBuilder Put()
        {
            Method = HttpMethod.Put;
            return this;
        }
        
        /// <inheritdoc />
        public IRequestBuilder Post()
        {
            Method = HttpMethod.Post;
            return this;
        }
        
        /// <inheritdoc />
        public IRequestBuilder Delete()
        {
            Method = HttpMethod.Delete;
            return this;
        }
        
        /// <inheritdoc />
        public IRequestBuilder Head()
        {
            Method = HttpMethod.Head;
            return this;
        }
        
        /// <inheritdoc />
        public IRequestBuilder Options()
        {
            Method = HttpMethod.Options;
            return this;
        }
        
        /// <inheritdoc />
        public IRequestBuilder ForUrl(string url)
        {
            PathAndQuery = url;

            if (PathAndQuery.Contains("?"))
            {
                var parts = PathAndQuery.Split('?');

                PathAndQuery = parts[0];

                QueryParameters = parts[1]
                    .Split('&')
                    .Select(p => p.Split('='))
                    .Select(p => new KeyValuePair<string, string?>(p[0], p.Length == 2 ? p[1] : null))
                    .ToList();
            }

            return this;
        }
        
        /// <inheritdoc />
        public IRequestBuilder AndContentType(string contentType)
        {
            if (Method == HttpMethod.Get ||
                Method == HttpMethod.Head ||
                Method == HttpMethod.Delete)
            {
                throw new ArgumentException($"A request with method {Method} cannot have a Content-Type header");
            }

            ContentType = contentType;
            return this;
        }

        public IRequestBuilder AndAuthorization(AuthenticationHeaderValue authenticationHeader) {
            AuthorizationHeader = authenticationHeader;
            return this;
        }

        /// <inheritdoc />
        public IRequestBuilder Accepting(string mimeType)
        {
            Accept = mimeType;
            return this;
        }

        public IRequestBuilder ForContent(string content)
        {
            ExpectedContent = content;
            return this;
        }

        /// <inheritdoc />
        public IResponseBuilder AndContent(string mimeType, object data)
        {
            MediaType = mimeType;
            Data = data;

            return this;
        }

        public IResponseBuilder AndContent(string mimeType, Func<HttpRequestMessage, object> callback)
        {
            MediaType = mimeType;
            ResponseCallback = callback;

            return this;
        }

        public IResponseBuilder AndContent(string mimeType, Func<HttpRequestMessage, Task<object>> callback)
        {
            MediaType = mimeType;
            AsyncResponseCallback = callback;

            return this;
        }

        /// <inheritdoc />
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
        
        /// <inheritdoc />
        public IResponseBuilder Taking(TimeSpan time)
        {
            Duration = time;

            return this;
        }
        
        /// <inheritdoc />
        public IResponseBuilder WhenCalled(Action<HttpRequestMessage> action)
        {
            ActionWhenCalled = action;

            return this;
        }
        
        /// <inheritdoc />
        public IResponseBuilder AndCookie(string name,
            string value,
            DateTime? expiresAt = null,
            string? sameSite = null,
            bool? secure = null,
            string? path = null,
            string? domain = null,
            int? maxAge = null)
        {
            var parameters = new List<string>();

            if (expiresAt != null)
            {
                parameters.Add($"Expires={expiresAt:R}");
            }

            if (!string.IsNullOrEmpty(sameSite))
            {
                parameters.Add($"SameSite={sameSite}");
            }

            if (secure.GetValueOrDefault(false))
            {
                parameters.Add("Secure");
            }

            if (!string.IsNullOrEmpty(path))
            {
                parameters.Add($"Path={path}");
            }

            if (!string.IsNullOrEmpty(domain))
            {
                parameters.Add($"Domain={domain}");
            }

            if (maxAge != null)
            {
                parameters.Add($"MaxAge={maxAge.Value}");
            }

            var cookieString = $"{name}={value}";

            if (parameters.Any())
            {
                cookieString += "; " + string.Join(";", parameters);
            }

            Cookies.Add(cookieString);

            return this;
        }

        /// <inheritdoc />
        public IResponseBuilder AndJsonContent(object value, JsonSerializerSettings? serializerSettings = null)
        {
            if (serializerSettings != null)
            {
                SerializerSettings = serializerSettings;
            }

            MediaType = "application/json";

            // Set this as the actual object because serialization
            // is handled by the TestableMessageHandler itself.
            Data = value;

            return this;
        }

        /// <summary>
        /// Create a dictionary that contains all the request headers as configured on this <see cref="RequestBuilder"/>
        /// </summary>
        /// <returns>A <c>Dictionary&lt;string, string&gt;</c> containing all request headers</returns>
        internal Dictionary<string, string> BuildRequestHeaders()
        {
            var headers = new Dictionary<string, string>();

            if (AuthorizationHeader is not null)
            {
                headers.Add("Authorization", AuthorizationHeader.ToString());
            }
            
            if (!string.IsNullOrEmpty(Accept))
            {
                headers.Add("Accept", Accept!);
            }

            if (!string.IsNullOrEmpty(ContentType))
            {
                headers.Add("Content-Type", ContentType!);
            }

            return headers;
        }
    }
}
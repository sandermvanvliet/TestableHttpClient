using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;

namespace Codenizer.HttpClient.Testable
{
    /// <summary>
    /// Implements a builder to configure a response
    /// </summary>
    public class RequestBuilder : IRequestBuilder, IResponseBuilder
    {
        private readonly RequestBuilder _root;

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
        internal RequestBuilder(HttpMethod method, string pathAndQuery, string contentType, RequestBuilder root = null)
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
                    .Select(p => new KeyValuePair<string, string>(p[0], p.Length == 2 ? p[1] : null))
                    .ToList();
            }

            ContentType = contentType;
        }
        
        /// <summary>
        /// The query parameters configured for the request
        /// </summary>
        public List<KeyValuePair<string, string>> QueryParameters { get; private set; } = new List<KeyValuePair<string, string>>();

        /// <summary>
        /// The path and query of the request to match
        /// </summary>
        public string PathAndQuery { get; private set; }
        /// <summary>
        /// The MIME type of the request to match
        /// </summary>
        public string ContentType { get; private set; }
        /// <summary>
        /// The status code to respond with. Defaults to 500 Internal Server Error
        /// </summary>
        public HttpStatusCode StatusCode { get; private set; } = HttpStatusCode.InternalServerError;
        /// <summary>
        /// The HTTP verb of the request to match
        /// </summary>
        public HttpMethod Method { get; private set; }
        /// <summary>
        /// Optional. The data to respond with. Use <see cref="AndContent"/> or <see cref="AndJsonContent"/> to set.
        /// </summary>
        public object Data { get; private set; }
        /// <summary>
        /// Optional. The MIME type of the content to respond with. Only applicable if <see cref="Data"/> is also provided, otherwise ignored.
        /// </summary>
        public string MediaType { get; private set; }
        /// <summary>
        /// Optional. The headers to set on the response.
        /// </summary>
        public Dictionary<string, string> Headers { get; } = new Dictionary<string, string>();
        /// <summary>
        /// Optional. The time to delay before responding to the matching request. Use <see cref="Taking"/> to set.
        /// </summary>
        public TimeSpan Duration { get; private set; } = TimeSpan.Zero;
        /// <summary>
        /// Optional. An action that will be called when the request metches, before providing the response.
        /// </summary>
        public Action<HttpRequestMessage> ActionWhenCalled { get; private set; }
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
        public JsonSerializerSettings SerializerSettings { get; private set; }
        /// <summary>
        /// Optional. The value of the Accept header to match.
        /// </summary>
        public string Accept { get; private set; }

        /// <inheritdoc />
        public IResponseBuilder With(HttpStatusCode statusCode)
        {
            StatusCode = statusCode;
            return this;
        }
        
        /// <inheritdoc />
        public IRequestBuilderForQueryString ForQueryStringParameter(string key)
        {
            return new RequestBuilderForQueryString(this, key);
        }
        
        /// <inheritdoc />
        public IRequestBuilder WithSequence(Action<IRequestBuilder> builder)
        {
            var requestBuilder = new RequestBuilder(Method, PathAndQuery, ContentType, _root ?? this);
            
            builder(requestBuilder);

            var root = _root ?? this;

            if (root != null)
            {
                root.ResponseSequence.Add(requestBuilder);
            }

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
                    .Select(p => new KeyValuePair<string, string>(p[0], p.Length == 2 ? p[1] : null))
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

        /// <inheritdoc />
        public IRequestBuilder Accepting(string mimeType)
        {
            Accept = mimeType;
            return this;
        }

        /// <inheritdoc />
        public IResponseBuilder AndContent(string mimeType, object data)
        {
            MediaType = mimeType;
            Data = data;

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
            string sameSite = null,
            bool? secure = null,
            string path = null,
            string domain = null,
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
        public IResponseBuilder AndJsonContent(object value, JsonSerializerSettings serializerSettings = null)
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
    }

    /// <summary>
    /// A request builder to provide assertions on query string parameters
    /// </summary>
    public interface IRequestBuilderForQueryString
    {
        /// <summary>
        /// Match any value of the query string parameter
        /// </summary>
        /// <returns></returns>
        IRequestBuilder WithAnyValue();

        /// <summary>
        /// Only match the request if the query string parameter has this exact value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        IRequestBuilder WithValue(string value);
    }
    
    /// <summary>
    /// Implements a builder to configure a response
    /// </summary>
    public interface IRequestBuilder
    {
        /// <summary>
        /// Respond with the given HTTP status code
        /// </summary>
        /// <param name="statusCode">The HTTP status code</param>
        /// <returns>The current <see cref="IRequestBuilder"/> instance</returns>
        IResponseBuilder With(HttpStatusCode statusCode);

        /// <summary>
        /// Only match the request when the provided query string also matches.
        /// </summary>
        /// <param name="key">The name of the query string parameter</param>
        /// <returns>A <see cref="IRequestBuilderForQueryString"/> to further configure assertions on the query string parameter</returns>
        IRequestBuilderForQueryString ForQueryStringParameter(string key);

        /// <summary>
        /// Add a sequence of responses for this request
        /// </summary>
        /// <remarks>Using WithSequence allows you to have multiple calls to the same endpoint with different responses.</remarks>
        /// <param name="builder">A <see cref="IRequestBuilder"/> instance used to configure the response for this step in the sequence of responses</param>
        /// <returns>A <see cref="IRequestBuilder"/> instance</returns>
        IRequestBuilder WithSequence(Action<IRequestBuilder> builder);

        /// <summary>
        /// Respond to a GET request
        /// </summary>
        /// <returns>The current <see cref="IRequestBuilder"/> instance</returns>
        IRequestBuilder Get();

        /// <summary>
        /// Respond to a PUT request
        /// </summary>
        /// <returns>The current <see cref="IRequestBuilder"/> instance</returns>
        IRequestBuilder Put();

        /// <summary>
        /// Respond to a POST request
        /// </summary>
        /// <returns>The current <see cref="IRequestBuilder"/> instance</returns>
        IRequestBuilder Post();

        /// <summary>
        /// Respond to a DELETE request
        /// </summary>
        /// <returns>The current <see cref="IRequestBuilder"/> instance</returns>
        IRequestBuilder Delete();

        /// <summary>
        /// Respond to a HEAD request
        /// </summary>
        /// <returns>The current <see cref="IRequestBuilder"/> instance</returns>
        IRequestBuilder Head();

        /// <summary>
        /// Respond to a OPTIONS request
        /// </summary>
        /// <returns>The current <see cref="IRequestBuilder"/> instance</returns>
        IRequestBuilder Options();
        
        /// <summary>
        /// Respond to a request that matches the given URL
        /// </summary>
        /// <param name="url">The relative URL to match</param>
        /// <returns>The current <see cref="IRequestBuilder"/> instance</returns>
        IRequestBuilder ForUrl(string url);
        
        /// <summary>
        /// Respond to a request that matches the given content type
        /// </summary>
        /// <param name="contentType">A MIME content type (for example text/plain)</param>
        /// <returns>The current <see cref="IRequestBuilder"/> instance</returns>
        IRequestBuilder AndContentType(string contentType);
        
        /// <summary>
        /// Respond to a request that matches the accept header
        /// </summary>
        /// <param name="mimeType">A MIME content type (for example text/plain)</param>
        /// <returns>The current <see cref="IRequestBuilder"/> instance</returns>
        IRequestBuilder Accepting(string mimeType);
    }
    
    /// <summary>
    /// Implements a builder to configure a response
    /// </summary>
    public interface IResponseBuilder
    {
        /// <summary>
        /// Respond with the given content
        /// </summary>
        /// <param name="mimeType">The MIME type of the response</param>
        /// <param name="data">The response to return</param>
        /// <returns>The current <see cref="IRequestBuilder"/> instance</returns>
        /// <remarks>
        /// Depending on the <paramref name="mimeType"/> the supplied data can be a string, byte[] or object. When a byte[] is given the content will be a <see cref="ByteArrayContent"/>,
        /// for strings a <see cref="StringContent"/> is used. For object the MIME type needs to be set to application/json otherwise an <see cref="InvalidOperationException" /> will be thrown.</remarks>
        IResponseBuilder AndContent(string mimeType, object data);

        /// <summary>
        /// Add the given HTTP headers to the response
        /// </summary>
        /// <remarks>If the header already exists the supplied value will be added to it separated by a comma</remarks>
        /// <param name="headers">A dictionary containing the headers to add</param>
        /// <returns>The current <see cref="IRequestBuilder"/> instance</returns>
        IResponseBuilder AndHeaders(Dictionary<string, string> headers);

        /// <summary>
        /// Delay the response
        /// </summary>
        /// <param name="time">The time to delay</param>
        /// <returns>The current <see cref="IRequestBuilder"/> instance</returns>
        IResponseBuilder Taking(TimeSpan time);

        /// <summary>
        /// Invoke an action when this request is made
        /// </summary>
        /// <param name="action">The action to invoke</param>
        /// <returns>The current <see cref="IRequestBuilder"/> instance</returns>
        IResponseBuilder WhenCalled(Action<HttpRequestMessage> action);

        /// <summary>
        /// Include a cookie on the response
        /// </summary>
        /// <param name="name">The name of the cookie</param>
        /// <param name="value">The value of the cookie</param>
        /// <param name="expiresAt">UTC date time when the cookie expires</param>
        /// <param name="sameSite">Flag indicating whether this is a SameSite cookie</param>
        /// <param name="secure">Flag indicating whether this is a secure cookie (HTTPS only)</param>
        /// <param name="path">The path to which this cookie applies</param>
        /// <param name="domain">The domain to which this cookie applies</param>
        /// <param name="maxAge">Number of seconds that this cookie can be alive for</param>
        /// <returns></returns>
        IResponseBuilder AndCookie(string name, string value, DateTime? expiresAt = null, string sameSite = null,
            bool? secure = null, string path = null, string domain = null, int? maxAge = null);

        /// <summary>
        /// Respond with the given value as a JSON serialized response
        /// </summary>
        /// <param name="value">The response to return</param>
        /// <param name="serializerSettings">Optional. The JSON serialization settings to use when serializing <paramref name="value"/></param>
        /// <returns>The current <see cref="IRequestBuilder"/> instance</returns>
        /// <remarks>If <paramref name="serializerSettings"/> is not given the JSON serialization settings of the <see cref="TestableMessageHandler"/> will be used.</remarks>
        IResponseBuilder AndJsonContent(object value, JsonSerializerSettings serializerSettings = null);
    }
}
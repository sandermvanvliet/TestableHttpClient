using System;
using System.Net;
using System.Net.Http.Headers;

namespace Codenizer.HttpClient.Testable
{
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
        [Obsolete("Use " + nameof(WithQueryStringParameter) + " instead")]
        IRequestBuilderForQueryString ForQueryStringParameter(string key);

        /// <summary>
        /// Only match the request when the provided query string also matches.
        /// </summary>
        /// <param name="key">The name of the query string parameter</param>
        /// <returns>A <see cref="IRequestBuilderForQueryString"/> to further configure assertions on the query string parameter</returns>
        IRequestBuilderForQueryString WithQueryStringParameter(string key);

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
        /// Respond to a request that matches the authorization header
        /// </summary>
        /// <param name="authenticationHeader">A authorization header value</param>
        /// <returns>The current <see cref="IRequestBuilder"/> instance</returns>
        IRequestBuilder AndAuthorization(AuthenticationHeaderValue authenticationHeader);
        
        /// <summary>
        /// Respond to a request that matches the accept header
        /// </summary>
        /// <param name="mimeType">A MIME content type (for example text/plain)</param>
        /// <returns>The current <see cref="IRequestBuilder"/> instance</returns>
        IRequestBuilder Accepting(string mimeType);

        /// <summary>
        /// Respond to a request that matches the request content
        /// </summary>
        /// <param name="content"></param>
        /// <returns>The current <see cref="IRequestBuilder"/> instance</returns>
        IRequestBuilder ForContent(string content);
    }
}
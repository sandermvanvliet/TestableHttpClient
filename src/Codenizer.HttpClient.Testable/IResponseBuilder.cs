using System;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;

namespace Codenizer.HttpClient.Testable
{
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
        IResponseBuilder AndCookie(string name, string value, DateTime? expiresAt = null, string? sameSite = null,
            bool? secure = null, string? path = null, string? domain = null, int? maxAge = null);

        /// <summary>
        /// Respond with the given value as a JSON serialized response
        /// </summary>
        /// <param name="value">The response to return</param>
        /// <param name="serializerSettings">Optional. The JSON serialization settings to use when serializing <paramref name="value"/></param>
        /// <returns>The current <see cref="IRequestBuilder"/> instance</returns>
        /// <remarks>If <paramref name="serializerSettings"/> is not given the JSON serialization settings of the <see cref="TestableMessageHandler"/> will be used.</remarks>
        IResponseBuilder AndJsonContent(object value, JsonSerializerSettings? serializerSettings = null);
    }
}
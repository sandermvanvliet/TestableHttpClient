using System;

namespace Codenizer.HttpClient.Testable
{
    /// <summary>
    /// A request builder to provide assertions on query string parameters
    /// </summary>
    public interface IRequestBuilderForQueryString
    {
        /// <summary>
        /// Match any value of the query string parameter
        /// </summary>
        /// <returns></returns>
        [Obsolete("Use " + nameof(HavingAnyValue) + " instead")]
        IRequestBuilder WithAnyValue();

        /// <summary>
        /// Only match the request if the query string parameter has this exact value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [Obsolete("Use " + nameof(HavingValue) + " instead")]
        IRequestBuilder WithValue(string value);

        /// <summary>
        /// Match any value of the query string parameter
        /// </summary>
        /// <returns></returns>
        IRequestBuilder HavingAnyValue();

        /// <summary>
        /// Only match the request if the query string parameter has this exact value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        IRequestBuilder HavingValue(string value);
    }
}